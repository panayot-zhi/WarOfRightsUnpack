using CommandLine;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using WarOfRightsUnpack.Common;
using WarOfRightsUnpack.Models;
using WarOfRightsUnpack.Models.JSON;
using WarOfRightsUnpack.Utility;

namespace WarOfRightsUnpack.Main
{
    public static class ExtractRegiments
    {
        private static string GetPattern(string identifier)
        {
            return $@"<Row.*>\r\n\s+(<Cell.*>\r\n\s+)?<Cell.*><Data.*>{identifier}</Data></Cell>\r\n\s+<Cell.*><Data.*>(?<data>.*)</Data></Cell>\r\n\s+<Cell.*><Data.*>(.*)</Data></Cell>\r\n\s+(<Cell.*>\r\n\s+)?</Row>";
        }

        public static void Run(Options options)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(options.DirectoryPath);
                if (!directoryInfo.Exists)
                {
                    Console.WriteLine(
                        $"Specified directory for the unpacked game files does not exist '{options.DirectoryPath}'.");
                    return;
                }

                var mapRegiments = JsonConvert.DeserializeObject<List<WoRMapRegiment>>(File.ReadAllText(options.MapRegiments));

                var textUiRegimentDisplayNames =
                    directoryInfo.GetFiles("text_ui_regiment_display_names.xml", SearchOption.AllDirectories).ToArray();
                if (textUiRegimentDisplayNames.Length > 1)
                {
                    Console.WriteLine(
                        $"Found more than one '\"text_ui_ingame.xml\"' file within the specified unpacked game directory '{options.DirectoryPath}'.");
                    Console.WriteLine("Taking the first occurrence an proceeding...");
                }
                else if (textUiRegimentDisplayNames.Length == 0)
                {
                    throw new ArgumentNullException(
                        message:
                        $"Cannot find '\"text_ui_ingame.xml\"' file within the specified unpacked game directory '{options.DirectoryPath}'.",
                        paramName: nameof(textUiRegimentDisplayNames));
                }

                var textUiInGameFile = textUiRegimentDisplayNames.First();

                var regiments = ExtractBasicRegimentsBasicInfo(textUiInGameFile);                

                var mapRegimentWeapons = ExtractBasicRegimentsDetailedInfo(directoryInfo, regiments, mapRegiments);

                Console.WriteLine($"Number of regiments: {regiments.Count}");

                WriteJSON(regiments, mapRegimentWeapons);
                WriteCSV(regiments, mapRegimentWeapons);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }


        private static List<WoRRegiment> ExtractBasicRegimentsBasicInfo(FileSystemInfo textUiRegimentDisplayNames)
        {
            var xml = File.ReadAllText(textUiRegimentDisplayNames.FullName);

            var regiments = new List<WoRRegiment>();

            var matches = Regex.Matches(xml, GetPattern(@"ui_(?<id>.+)Title"));
            Console.WriteLine("Total items: " + matches.Count + Environment.NewLine);
            foreach (Match match in matches)
            {
                var id = match.Groups["id"].ToString();
                var name = match.Groups["data"].ToString();

                Console.WriteLine("Gathering information for: " + name);

                var description = Regex.Match(xml, GetPattern($@"ui_{id}Description"), RegexOptions.IgnoreCase)
                    .Groups["data"].ToString();

                var identifier = NormalizeIdentifier(id);
                var entry = new WoRRegiment
                {
                    ID = identifier,

                    Name = name,

                    Description = description,
                    Type = identifier.Contains(Constants.Battery) ? 
                        "Artillery" : 
                        "Infantry"
                };

                regiments.Add(entry);
            }

            Console.WriteLine();

            return regiments;
        }

        private static List<WoRMapRegimentWeapon> ExtractBasicRegimentsDetailedInfo(
            DirectoryInfo textUiRegimentDisplayNames, ICollection<WoRRegiment> regiments,
            List<WoRMapRegiment> mapRegiments)
        {
            var equipmentDirectoryPath = Path.Combine(textUiRegimentDisplayNames.FullName, "Scripts", "Definitions", "Outfitter");
            var jsonFiles = new DirectoryInfo(equipmentDirectoryPath).GetFiles("*.json");

            Console.WriteLine("Found " + jsonFiles.Length + " json files with information about regiments equipment." + Environment.NewLine);

            var result = new List<WoRMapRegimentWeapon>();

            foreach (var jsonFile in jsonFiles)
            {
                Console.WriteLine($"Extracting information from '{jsonFile.Name}' about regiment weapon pool.");

                var jsonText = File.ReadAllText(jsonFile.FullName);

                // TODO: Remove this line when the developers fix the JSON issue
                if (jsonFile.Name is "csa_infantry_2nd_mississippi.json" or 
                        "usa_infantry_23rd_ohio.json")
                {
                    jsonText = jsonText.Replace("\"PlayerThreshold\": 0", "\"PlayerThreshold\": 0,");
                    jsonText = jsonText.Replace("\"PlayerThreshold\": 200", "\"PlayerThreshold\": 200,");
                }

                var root = JsonConvert.DeserializeObject<EquipmentScript.Root>(jsonText);

                if (root == null)
                {
                    throw new ArgumentNullException(nameof(root), $"Could not convert '{jsonFile.Name}' to JSON.");
                }

                var regimentNumber = root.Identifier;
                var regimentName = root.Name;
                var regimentType = root.Type;
                var regimenFaction = root.Team;
                var regimentState = root.State;

                var companies = root.Companies.Select(x => $"Co. {x.Identifier}");

                var regimentID = NormalizeRegimentName(regimentName + regimentNumber);

                var existingRegiment = regiments.SingleOrDefault(x => x.ID == regimentID);

                if (existingRegiment == null)
                {
                    ConsoleHelper.WriteWarning("WARNING: Following regiment does not have a correspondence in the gathered regiments!");
                    ConsoleHelper.WriteWarning($"{regimenFaction}, {regimentNumber}, {regimentName} -- {regimentID} [{jsonFile.Name}]");

                    existingRegiment = new WoRRegiment()
                    {
                        ID = regimentID,
                        Name = regimentName
                    };

                    regiments.Add(existingRegiment);
                }

                existingRegiment.Faction = regimenFaction;
                existingRegiment.Companies = string.Join(", ", companies);
                if (regimentType.Equals(Constants.Artillery))
                {
                    existingRegiment.Type = Constants.Artillery;
                    // existingRegiment.State = regimentState;
                }
                else
                {
                    existingRegiment.Type = Constants.Infantry;
                    existingRegiment.State = regimentState;
                    existingRegiment.Number = regimentNumber;
                }

                var weapons = root.ClassTypes
                    .Where(x => x.Type.Equals(Constants.Private))
                    .SelectMany(x => x.Classes)
                    .Select(x => x.Loadout)
                    .SelectMany(x => x.Primaries);

                foreach (var weaponName in weapons)
                {
                    var weaponID = NormalizeWeaponName(weaponName);
                    if (weaponID is Constants.ShellRammer or Constants.SpongeRammer)
                    {
                        continue;
                    }

                    var mapsWithThisRegiment = mapRegiments.Where(x => x.RegimentID == existingRegiment.ID).ToList();
                    foreach (var mapRegiment in mapsWithThisRegiment)
                    {
                        result.Add(new WoRMapRegimentWeapon()
                        {
                            ID = mapRegiment.ID + weaponID,
                            MapRegimentID = mapRegiment.ID,
                            WeaponID = weaponID
                            // Percent = 100
                        });
                    }

                }
            }

            Console.WriteLine();

            return result;
        }


        private static string NormalizeWeaponName(string name)
        {
            return name?.Remove("_").Remove(".");
        }

        private static string NormalizeRegimentName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            name = name
                .Remove(" ")
                .Remove(",")
                .Remove(".");

            if (name.EndsWith(Constants.Battery))
            {
                name = Constants.Battery + name.Remove(Constants.Battery);
            }

            if (name.EndsWith(Constants.HeavyArtillery))
            {
                name = Constants.HeavyArtillery + name.Remove(Constants.HeavyArtillery);
            }

            if (name.StartsWith(Constants.Legion))
            {
                name = name.Remove(Constants.Legion) + Constants.Legion;
            }

            if (name.StartsWith(Constants.Sharpshooters))
            {
                name = name.Remove(Constants.Sharpshooters) + Constants.Sharpshooters;
            }

            return name;

        }

        private static string NormalizeIdentifier(string id)
        {
            var propertyName = id.Remove(".").Remove(",");

            var badNameMatch = Regex.Match(propertyName, @"^(?<number>\d+(st|nd|rd|th))(?<name>\w+)");
            if (badNameMatch.Success)
            {
                var regimentName = badNameMatch.Groups["name"];
                var regimentNumber = badNameMatch.Groups["number"];
                propertyName = $"{regimentName}{regimentNumber}";
            }

            return propertyName;
        }


        private static void WriteCSV(IEnumerable<WoRRegiment> regiments, IEnumerable<WoRMapRegimentWeapon> mapRegimentWeapons)
        {
            const string mapsCsvFileName = "warofrights.regiments.csv";
            const string mapRegimentsCsvFileName = "warofrights.mapregimentweapons.csv";

            Console.WriteLine($"Writing CSV DB files: '{mapsCsvFileName}', '{mapRegimentsCsvFileName}'");

            var csvSerializerSettings = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                Encoding = Encoding.UTF8
            };

            using (var writer = new StreamWriter(mapsCsvFileName))
            using (var csv = new CsvWriter(writer, csvSerializerSettings))
            {
                csv.Context.RegisterClassMap<GenericAutoMap<WoRRegiment>>();
                csv.WriteRecords(regiments.OrderBy(x => x.ID).ToList());
            }

            using (var writer = new StreamWriter(mapRegimentsCsvFileName))
            using (var csv = new CsvWriter(writer, csvSerializerSettings))
            {
                csv.Context.RegisterClassMap<GenericAutoMap<WoRMapRegimentWeapon>>();
                csv.WriteRecords(mapRegimentWeapons.OrderBy(x => x.WeaponID).ToList());
            }
        }

        private static void WriteJSON(IEnumerable<WoRRegiment> regiments, IEnumerable<WoRMapRegimentWeapon> mapRegimentWeapons)
        {
            const string regimentsJsonFileName = "warofrights.regiments.json";
            const string mapRegimentWeaponsJsonFileName = "warofrights.mapregimentweapons.json";

            Console.WriteLine($"Writing JSON DB files: '{regimentsJsonFileName}', '{mapRegimentWeaponsJsonFileName}'");

            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>()
                {
                    new MySqlDecimalConverter()
                },

                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };

            File.WriteAllText(regimentsJsonFileName,
                JsonConvert.SerializeObject(regiments.OrderBy(x => x.ID).ToList(), jsonSerializerSettings));
            File.WriteAllText(mapRegimentWeaponsJsonFileName,
                JsonConvert.SerializeObject(mapRegimentWeapons.OrderBy(x => x.WeaponID).ToList(), jsonSerializerSettings));
        }


        [Verb("extractRegiments", HelpText = @"
Extracts a db-ready json/csv structure for regiments from the following in-game files:    
    localization\english_xml\text_ui_regiment_display_names.xml
    Libs\UI\Textures\icons\Regiments
    Scripts\equipment    
")]
        public class Options
        {
            [Option(longName: "path", shortName: 'p', Required = true, HelpText = "File path to the unpacked game directory.")]
            public string DirectoryPath { get; set; }

            [Option(longName: "mapRegiments", shortName: 'r', Required = true, HelpText = "File path to extracted mappings for maps and regiments.")]
            public string MapRegiments { get; set; }
        }
    }
}
