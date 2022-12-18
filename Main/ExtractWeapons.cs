using CommandLine;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using WarOfRightsUnpack.Common;
using WarOfRightsUnpack.Models;

namespace WarOfRightsUnpack.Main
{
    public static class ExtractWeapons
    {
        private static string GetPattern(string identifier)
        {
            return $@"<Row.*>\r\n\s+(<Cell.*>\r\n\s+)?<Cell.*><Data.*>{identifier}</Data></Cell>\r\n\s+<Cell.*><Data.*>(?<data>.*)</Data></Cell>\r\n\s+<Cell.*><Data.*>(.*)</Data></Cell>\r\n\s+(<Cell.*>\r\n\s+)?</Row>";
        }


        public static void Run(Options options)
        {
            try
            {
                var fileInfo = new FileInfo(options.FileName);
                var xml = File.ReadAllText(fileInfo.FullName);

                var weapons = new List<WoRWeapon>();

                var matches = Regex.Matches(xml, GetPattern(@"ui_(?<id>\w+)_title"));
                Console.WriteLine("Total items: " + matches.Count + Environment.NewLine);
                foreach (Match match in matches)
                {
                    var id = match.Groups["id"].ToString();
                    var name = match.Groups["data"].ToString();
                    var propertyName = NormalizePropertyName(id);

                    Console.WriteLine("Gathering information for: " + name);

                    string parametersInfo = "";
                    var infoMatches = Regex.Matches(xml, GetPattern($@"ui_{id}_info\d"));
                    foreach (Match infoMatch in infoMatches)
                    {
                        parametersInfo += infoMatch.Groups["data"] + "\\n";
                    }

                    // fix info
                    parametersInfo = parametersInfo.Replace(" in\\n", "\"\\n");
                    parametersInfo = parametersInfo.Replace("\\n", "<br>");
                    // if (parametersInfo == string.Empty)
                    // {
                    //     parametersInfo = null;
                    // }

                    var description = Regex.Match(xml, GetPattern($@"ui_{id}_description"))
                        .Groups["data"].ToString();
                    // if (description == string.Empty)
                    // {
                    //     description = null;
                    // }

                    var entry = new WoRWeapon()
                    {
                        ID = propertyName,

                        Name = name,
                        Description = description,
                        ParametersDescription = parametersInfo,
                    };

                    weapons.Add(entry);
                }

                Console.WriteLine();

                WriteJSON(weapons);
                WriteCSV(weapons);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }


        private static string NormalizePropertyName(string id)
        {
            return id?.Remove("_");
        }


        private static void WriteCSV(IEnumerable<WoRWeapon> weapons)
        {
            const string csvFileName = "warofrights.weapons.csv";

            Console.WriteLine($"Writing CSV DB file: '{csvFileName}'");

            var csvSerializerSettings = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                Encoding = Encoding.UTF8
            };

            using (var writer = new StreamWriter(csvFileName))
            using (var csv = new CsvWriter(writer, csvSerializerSettings))
            {
                csv.Context.RegisterClassMap<GenericAutoMap<WoRWeapon>>();
                csv.WriteRecords(weapons.OrderBy(x => x.ID).ToList());
            }
        }

        private static void WriteJSON(IEnumerable<WoRWeapon> weapons)
        {
            const string jsonFileName = "warofrights.weapons.json";

            Console.WriteLine($"Writing JSON DB file: '{jsonFileName}'");

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

            File.WriteAllText(jsonFileName,
                JsonConvert.SerializeObject(weapons.OrderBy(x => x.ID).ToList(), jsonSerializerSettings));
        }


        [Verb("extractWeapons", HelpText = "Extracts a structured db-ready json/csv structure from the in-game xml.")]
        public class Options
        {
            [Option(longName: "filename", shortName: 'f', Required = true, HelpText = "File path to the target xml (text_ui_items.xml).")]
            public string FileName { get; set; }
        }
    }
}
