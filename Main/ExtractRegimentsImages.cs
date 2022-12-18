using CommandLine;
using System.Text.RegularExpressions;
using WarOfRightsUnpack.Common;

namespace WarOfRightsUnpack.Main
{
    public static class ExtractRegimentsImages
    {
        public static void Run(Options options)
        {
            var pathInfo = new DirectoryInfo(options.PathName);
            var ddsFiles = pathInfo.GetFiles("*.dds", SearchOption.AllDirectories);

            Console.WriteLine("Found " + ddsFiles.Length + " dds files with regimental representation.");

            if (string.IsNullOrEmpty(options.OutputDirectory) || !Directory.Exists(options.OutputDirectory))
            {
                if (!Directory.Exists("regiments"))
                {
                    Directory.CreateDirectory("regiments");
                }

                options.OutputDirectory = "regiments\\";
            }

            Console.WriteLine("Dumping images to: " + options.OutputDirectory + Environment.NewLine);

            foreach (var ddsFile in ddsFiles)
            {
                Console.WriteLine("Extracting: " + ddsFile.FullName);

                var regiment = ddsFile.Directory.Name;
                var branch = ddsFile.Directory.Parent.Name;
                var faction = ddsFile.Directory.Parent.Parent.Name;
                var rank = Path.GetFileNameWithoutExtension(ddsFile.FullName);

                regiment = NormalizeRegimentName(regiment);
                faction = faction.ToLowerInvariant();
                branch = branch.ToLowerInvariant();

                var ddsImage = new DDSImage(ddsFile.FullName);

                var targetFileName = Path.Combine(options.OutputDirectory, $"{faction}_{branch}_{regiment}_{rank}.png");
                if (File.Exists(targetFileName))
                {
                    Console.WriteLine($"File '{targetFileName}' exists, skipping.");
                    continue;
                }

                Console.WriteLine("Writing to file: " + targetFileName);
                ddsImage.Save(targetFileName);
            }
        }

        private static string NormalizeRegimentName(string regiment)
        {
            var propertyName = regiment.Remove(" ");

            var badNameMatch = Regex.Match(propertyName, @"^(?<number>\d+(st|nd|rd|th))(?<name>\w+)");
            if (badNameMatch.Success)
            {
                var regimentName = badNameMatch.Groups["name"];
                var regimentNumber = badNameMatch.Groups["number"];
                propertyName = $"{regimentName}{regimentNumber}";
            }

            return propertyName;
        }

        [Verb("extractRegimentsImages", HelpText = "Extracts and converts to .png all regimental character images to (optional) a combined output path.")]
        public class Options
        {
            [Option(longName: "pathname", shortName: 'p', Required = true, HelpText = "File path to the target directory containing regiment images (e.g. Libs\\UI\\Textures\\icons\\Regiments).")]
            public string PathName { get; set; }

            [Option(longName: "output", shortName: 'o', Required = false, HelpText = "Optional output path to extract all the combined images, if omitted, images will be dumped in the executing directory.")]
            public string OutputDirectory { get; set; }
        }
    }
}
