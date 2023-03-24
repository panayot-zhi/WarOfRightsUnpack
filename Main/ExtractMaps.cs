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
    public static class ExtractMaps
    {
        public static void Run(Options options)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(options.DirectoryPath);
                if (!directoryInfo.Exists)
                {
                    ConsoleHelper.WriteLine($"Specified directory for the unpacked game files does not exist '{options.DirectoryPath}'.");
                    return;
                }

                var textUiInGameFile = FileHelper.GetFile(directoryInfo, "text_ui_ingame.xml");
                var textUiHistoricalNarration = FileHelper.GetFile(directoryInfo, "text_ui_historical_narration.xml");

                var maps = ExtractMapsBasicInfo(textUiInGameFile);
                var mapRegiments = ExtractMapsDetailedInfo(directoryInfo, maps);
                ExtractMapsNarratorInfo(textUiHistoricalNarration, maps);
                ExtractMapsImages(directoryInfo, maps);

                WriteJSON(maps, mapRegiments);
                WriteCSV(maps, mapRegiments);
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError(ex.Message);
                ConsoleHelper.WriteError(ex.StackTrace);
            }
        }

        private static List<WoRMap> ExtractMapsBasicInfo(FileSystemInfo textUiInGameFileInfo)
        {
            var result = new List<WoRMap>();

            ConsoleHelper.WriteLine($"Gathering basic map information from file: '{textUiInGameFileInfo.Name}'.");

            var xml = File.ReadAllText(textUiInGameFileInfo.FullName);

            static string GetPattern(string identifier)
            {
                return $@"<Row.*>\r\n\s+(<Cell.*>\r\n\s+)?<Cell.*><Data.*>{identifier}</Data></Cell>\r\n\s+<Cell.*><Data.*>(?<data>.*)</Data></Cell>\r\n\s+<Cell.*><Data.*>(.*)</Data></Cell>\r\n\s+(<Cell.*>\r\n\s+)?</Row>";
            }

            static string GetDateTime(string id, MatchCollection dateTimes)
            {
                return dateTimes.SingleOrDefault(x => x.Groups["id"].ToString() == id)?.Groups["data"].ToString();
            }

            var dateTimes = Regex.Matches(xml, GetPattern(@"SkirmishInfo(?<id>.+)DateTime"), RegexOptions.IgnoreCase);
            var descriptions = Regex.Matches(xml, GetPattern(@"SkirmishInfo(?<id>.+)Description"), RegexOptions.IgnoreCase);
            ConsoleHelper.WriteLine($"Total descriptions found: {descriptions.Count}; Battle date times description: {dateTimes.Count}" + Environment.NewLine);

            foreach (Match descriptionMatch in descriptions)
            {
                var identifier = descriptionMatch.Groups["id"].ToString();
                var description = descriptionMatch.Groups["data"].ToString();

                var mapID = NormalizeMapIdentifier(identifier);
                description = NormalizeDescription(description);
                var dateTime = GetDateTime(identifier, dateTimes);
                var displayName = NormalizeMapName(identifier);

                ConsoleHelper.WriteLine("Information gathered for: " + displayName);

                var entry = result.SingleOrDefault(x => x.ID == mapID);
                if (entry != null)
                {
                    entry.ID = mapID;
                    entry.DateTimeDescription = dateTime;
                    entry.Name = displayName;
                    entry.Description = description;
                }
                else
                {
                    entry = new WoRMap()
                    {
                        ID = mapID,

                        DateTimeDescription = dateTime,
                        Name = displayName,
                        Description = description
                    };

                    result.Add(entry);
                }
            }

            ConsoleHelper.WriteLine();

            return result;
        }

        private static void ExtractMapsNarratorInfo(FileInfo textUiHistoricalNarration, List<WoRMap> maps)
        {
            ConsoleHelper.WriteLine($"Gathering narrator map information from file: '{textUiHistoricalNarration.Name}'.");

            var xml = File.ReadAllText(textUiHistoricalNarration.FullName);

            static string GetPattern(string identifier)
            {
                return $@"<Cell.*><Data.*>{identifier}</Data></Cell>\r\n\s+<Cell.*><Data.*>(?<data>.*)</Data></Cell>";
            }

            var narratorInformation = Regex.Matches(xml, GetPattern(@"ui_(?<id>.*)_Part\d+_Subtitle\d+"), RegexOptions.IgnoreCase);
            ConsoleHelper.WriteLine($"Total narrator lines found: {narratorInformation.Count};" + Environment.NewLine);

            foreach (Match narratorLineMatch in narratorInformation)
            {
                var identifier = narratorLineMatch.Groups["id"].ToString();
                var line = narratorLineMatch.Groups["data"].ToString();

                var mapName = NormalizeMapIdentifier(identifier);

                ConsoleHelper.WriteLine("Found line for: " + mapName);

                var entry = maps.SingleOrDefault(x => x.ID == mapName);
                if (entry != null)
                {
                    if (!string.IsNullOrEmpty(entry.NarratorInfo))
                    {
                        entry.NarratorInfo += "<br>";
                    }

                    entry.NarratorInfo += line;
                }
                else
                {
                    ConsoleHelper.WriteWarning($"No map found with mapID of '{mapName}'!");
                }
            }

            ConsoleHelper.WriteLine();
        }

        private static List<WoRMapRegiment> ExtractMapsDetailedInfo(DirectoryInfo unpackedGameFilesDirectory, ICollection<WoRMap> maps)
        {
            var result = new List<WoRMapRegiment>();

            var mapsFilePath = new DirectoryInfo(Path.Combine(unpackedGameFilesDirectory.FullName, "Levels"));

            var jsonDrillCampFiles = mapsFilePath.GetFiles("DrillCamp.json", SearchOption.AllDirectories);
            ConsoleHelper.WriteLine("Found " + jsonDrillCampFiles.Length + " json files with information about drill camp areas.");

            var jsonSkirmishFiles = mapsFilePath.GetFiles("Skirmish.json", SearchOption.AllDirectories);
            ConsoleHelper.WriteLine("Found " + jsonSkirmishFiles.Length + " json files with information about skirmish areas.");

            var jsonConquestFiles = mapsFilePath.GetFiles("Conquest.json", SearchOption.AllDirectories);
            ConsoleHelper.WriteLine("Found " + jsonConquestFiles.Length + " json files with information about conquest areas.");

            var jsonFiles = jsonSkirmishFiles.Concat(jsonConquestFiles).Concat(jsonDrillCampFiles).ToArray();

            foreach (var jsonFile in jsonFiles)
            {
                var mapOrder = 0;
                var areaName = jsonFile.Directory?.Parent?.Name;
                var mapType = Path.GetFileNameWithoutExtension(jsonFile.FullName);
                var json = TryReadJson(jsonFile);

                if (json == null)
                {
                    ConsoleHelper.WriteWarning($"Could not convert '{jsonFile.Name}' to JSON.");
                    continue;
                }

                var mainComponent = json.Components.FirstOrDefault();
                if (mainComponent == null)
                {
                    continue;
                }

                var gamePlayAreas = mainComponent.GameplayAreas;
                ConsoleHelper.WriteLine($"Found {gamePlayAreas.Count} areas in json file: '{jsonFile.FullName}'");

                gamePlayAreas.ForEach(area =>
                {
                    var mapID = NormalizeMapIdentifier(area.Name);
                    var entry = maps.SingleOrDefault(x => x.ID == mapID);

                    if (entry == null)
                    {
                        entry = new WoRMap()
                        {
                            ID = mapID,

                            Name = area.Name,
                            DateTimeDescription = string.Empty,
                            Description = string.Empty
                        };

                        maps.Add(entry);
                    }

                    //entry.ID = identifier;
                    entry.MapType = mapType;
                    entry.AreaName = areaName;

                    var skirmishDescriptor = area.Descriptors.SingleOrDefault(x => x.Type == "SkirmishDescriptor");
                    if (skirmishDescriptor != null)
                    {
                        entry.DefendingTeam = skirmishDescriptor.DefendingTeam;
                        entry.FinalPushTime = skirmishDescriptor.FinalPushTime;
                        entry.TicketsCSA = skirmishDescriptor.TicketsCSA;
                        entry.TicketsUSA = skirmishDescriptor.TicketsUSA;
                    }

                    var conquestDescriptor = area.Descriptors.SingleOrDefault(x => x.Type == "ConquestDescriptor");
                    if (conquestDescriptor != null)
                    {
                        entry.TicketsCSA = conquestDescriptor.TicketsCSA;
                        entry.TicketsUSA = conquestDescriptor.TicketsUSA;
                    }

                    entry.Order = mapOrder++;

                    var activeRegiments = area.Operations?.SingleOrDefault(x => x.Type == "SetActiveRegiments");
                    if (activeRegiments != null)
                    {
                        foreach (var regiment in activeRegiments.Regiments)
                        {
                            var regimentID = NormalizeRegimentIdentifier(regiment);
                            var regimentOrder = result.Count(x => x.MapID == mapID);

                            result.Add(new WoRMapRegiment
                            {
                                ID = mapID + regimentID,
                                MapID = mapID,
                                RegimentID = regimentID,

                                Order = regimentOrder
                            });
                        }
                    }
                });
            }

            ConsoleHelper.WriteLine();

            return result;
        }

        private static AreasSkirmish.Root TryReadJson(FileInfo jsonFile)
        {
            try
            {
                return JsonConvert.DeserializeObject<AreasSkirmish.Root>(File.ReadAllText(jsonFile.FullName));
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError(ex.Message);
            }

            return null;
        }

        private static void ExtractMapsImages(DirectoryInfo unpackedGameFilesDirectory, List<WoRMap> maps)
        {
            const string outputPath = "maps";
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            var texturesPath = Path.Combine(unpackedGameFilesDirectory.FullName, "Libs", "UI", "Textures");
            ConsoleHelper.WriteLine("Dumping images to: " + outputPath + Environment.NewLine);

            static string ExtractDDSFile(FileInfo ddsFile, string outputPath, string suffix, out string mapName)
            {
                ConsoleHelper.WriteLine("Extracting: " + ddsFile.FullName);

                mapName = Path.GetFileNameWithoutExtension(ddsFile.FullName);

                if (mapName.EndsWith("loading"))
                {
                    mapName = mapName.Remove("loading");
                }

                mapName = NormalizeMapIdentifier(mapName);

                var ddsImage = new DDSImage(ddsFile.FullName);
                var targetFileName = Path.Combine(outputPath, $"{mapName}_{suffix}.png");

                if (File.Exists(targetFileName))
                {
                    ConsoleHelper.WriteLine($"File '{targetFileName}' exists, skipping.");
                }
                else
                {
                    ConsoleHelper.WriteLine("Writing to file: " + targetFileName);
                    ddsImage.Save(targetFileName);
                }

                return targetFileName;
            }

            string mapName;

            var currentFilePath = Path.Combine(texturesPath, "LoadingScreens");
            var ddsFiles = new DirectoryInfo(currentFilePath).GetFiles("*.dds");

            ConsoleHelper.WriteLine($"Found {ddsFiles.Length} dds files with map's loading screen images.");
            ConsoleHelper.WriteLine();

            foreach (var ddsFile in ddsFiles)
            {
                var fileName = ExtractDDSFile(ddsFile, outputPath, "loading", out mapName);
                var map = maps.SingleOrDefault(x => x.ID == mapName);
                if (map != null)
                {
                    map.LoadingImagePath = fileName;
                }
                else
                {
                    ConsoleHelper.WriteWarning($"No map found for '{mapName}'!");
                }
            }

            currentFilePath = Path.Combine(texturesPath, "Maps");
            ddsFiles = new DirectoryInfo(currentFilePath).GetFiles("*.dds", SearchOption.AllDirectories);

            ConsoleHelper.WriteLine();
            ConsoleHelper.WriteLine($"Found {ddsFiles.Length} dds files with skirmish map images.");
            ConsoleHelper.WriteLine();

            foreach (var ddsFile in ddsFiles)
            {
                var fileName = ExtractDDSFile(ddsFile, outputPath, "skirmish", out mapName);
                var map = maps.SingleOrDefault(x => x.ID == mapName);
                if (map != null)
                {
                    map.SkirmishImagePath = fileName;
                }
                else
                {
                    ConsoleHelper.WriteWarning($"No map found for '{mapName}'!");
                }
            }

            currentFilePath = Path.Combine(texturesPath, "Spawn Screen");
            ddsFiles = new DirectoryInfo(currentFilePath).GetFiles("*.dds");

            ConsoleHelper.WriteLine();
            ConsoleHelper.WriteLine($"Found {ddsFiles.Length} dds files with map's loading screen images.");
            ConsoleHelper.WriteLine();

            foreach (var ddsFile in ddsFiles)
            {
                var fileName = ExtractDDSFile(ddsFile, outputPath, "spawn", out mapName);
                var map = maps.SingleOrDefault(x => x.ID == mapName);
                if (map != null)
                {
                    map.SpawnImagePath = fileName;
                }
                else
                {
                    ConsoleHelper.WriteWarning($"No map found for '{mapName}'!");
                }
            }

            ConsoleHelper.WriteLine();
        }


        private static string NormalizeMapIdentifier(string id)
        {
            var mapID = id?.Replace("&amp;", "&").Remove(" ").Remove("_").Remove("'").Remove("&");
            return Constants.MapAlternativeNames
                .SingleOrDefault(x => 
                    x.Key.Equals(mapID) || 
                    x.Value.Contains(mapID))
                .Key ?? mapID;
        }

        private static string NormalizeRegimentIdentifier(string equipment)
        {
            var regiment = equipment
                .Remove("usa")
                .Remove("csa")
                .Remove("_infantry")
                .Remove("_artillery")
                // .Remove(".")
                .Remove(",")
                // .Remove("_")
                .Remove("*");
                //.Remove(" ");

            regiment = regiment.Replace(".", "_");
            regiment = regiment.Replace(" ", "_");
            var matches = Regex.Matches(regiment, "_([a-z])");

            regiment = matches
                .Aggregate(regiment, (current, match) => 
                    current.Replace(match.Value, 
                        $"_{match.Groups[0].Value.ToUpper()}"));
            
            // place for dirty fixes
            if (regiment.Contains("Nc__Sharpshooters"))
            {
                regiment = regiment.Replace("Nc__Sharpshooters", "NC__Sharpshooters");
            }

            regiment = regiment.Remove("_");

            if (regiment.EndsWith("Battery"))
            {
                regiment = "Battery" + regiment.Remove("Battery");
            }

            if (regiment.StartsWith("Legion"))
            {
                regiment = regiment.Remove("Legion") + "Legion";
            }

            if (regiment.StartsWith("Sharpshooters"))
            {
                regiment = regiment.Remove("Sharpshooters") + "Sharpshooters";
            }

            return NormalizeIdentifier(regiment);
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

        private static string NormalizeMapName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            name = name.Replace("&amp;", "&");
            var newText = new StringBuilder(name.Length * 2);
            newText.Append(name[0]);

            for (int i = 1; i < name.Length; i++)
            {
                if (char.IsUpper(name[i]))
                {
                    if ((name[i - 1] != ' ' && !char.IsUpper(name[i - 1])) ||
                        (char.IsUpper(name[i - 1]) && i < name.Length - 1 && !char.IsUpper(name[i + 1])))
                    {
                        newText.Append(' ');
                    }
                }
                else if (name[i] == '&')
                {
                    newText.Append(' ');
                }

                newText.Append(name[i]);
            }

            return newText.ToString();
        }

        private static string NormalizeDescription(string description)
        {
            return description.Replace("&#10;\\n&#10;", "<br>");
        }


        private static void WriteCSV(IEnumerable<WoRMap> maps, IEnumerable<WoRMapRegiment> mapRegiments)
        {
            const string mapsCsvFileName = "warofrights.maps.csv";
            const string mapRegimentsCsvFileName = "warofrights.mapregiments.csv";

            ConsoleHelper.WriteLine($"Writing CSV DB files: '{mapsCsvFileName}', '{mapRegimentsCsvFileName}'");

            var csvSerializerSettings = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                Encoding = Encoding.UTF8
            };

            using (var writer = new StreamWriter(mapsCsvFileName))
            using (var csv = new CsvWriter(writer, csvSerializerSettings))
            {
                csv.Context.RegisterClassMap<GenericAutoMap<WoRMap>>();
                csv.WriteRecords(maps.OrderBy(x => x.ID).ToList());
            }

            using (var writer = new StreamWriter(mapRegimentsCsvFileName))
            using (var csv = new CsvWriter(writer, csvSerializerSettings))
            {
                csv.WriteRecords(mapRegiments.OrderBy(x => x.MapID).ToList());
            }
        }

        private static void WriteJSON(IEnumerable<WoRMap> maps, IEnumerable<WoRMapRegiment> mapRegiments)
        {
            const string mapsJsonFileName = "warofrights.maps.json";
            const string mapRegimentsJsonFileName = "warofrights.mapregiments.json";

            ConsoleHelper.WriteLine($"Writing JSON DB files: '{mapsJsonFileName}', '{mapRegimentsJsonFileName}'");

            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>()
                {
                    new MySqlDecimalConverter()
                },

                MissingMemberHandling = MissingMemberHandling.Ignore,
                // NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };

            File.WriteAllText(mapsJsonFileName,
                JsonConvert.SerializeObject(maps.OrderBy(x => x.ID).ToList(), jsonSerializerSettings));
            File.WriteAllText(mapRegimentsJsonFileName,
                JsonConvert.SerializeObject(mapRegiments.OrderBy(x => x.MapID).ToList(), jsonSerializerSettings));
        }


        [Verb("extractMaps", HelpText = @"
Extracts a db-ready json/csv structure for maps from the following in-game files:    
    localization\english_xml\text_ui_ingame.xml
    Libs\UI\Textures
    Levels\
")]
        public class Options
        {
            [Option(longName: "path", shortName: 'p', Required = true, HelpText = "File path to the unpacked game directory.")]
            public string DirectoryPath { get; set; }
        }
    }
}
