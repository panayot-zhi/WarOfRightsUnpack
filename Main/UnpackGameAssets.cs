using CommandLine;
using System.Diagnostics;
using System.IO.Compression;

namespace WarOfRightsUnpack.Main
{
    public class UnpackGameAssets
    {
        public static readonly List<string> FilesOfInterest = new()
        {
            "Levels/",
            "localization/",
            "Audio.pak",
            "EntitiesXML.pak",
            "GameData.pak",
            "LevelsLooseFiles.pak",
            "Objects.pak",
            "Objects_Artillery.pak",
            "Objects_Characters.pak",
            "Objects_Characters_Equipment.pak",
            "Objects_Colors.pak",
            "Objects_EdgedWeapons.pak",
            "Objects_Revolvers.pak",
            "Objects_Rifles.pak",
            "Probes_Antietam.pak",
            "Probes_DrillCamp.pak",
            "Probes_HarpersFerry.pak",
            "Probes_Showroom_1.pak",
            "Probes_SouthMountain.pak",
            "Scripts.pak",
            "Textures.pak",
            "UI.pak",
            "UI_Actions.pak",
            "UI_Textures.pak",
            "UI_Textures_CSA.pak",
            "UI_Textures_USA.pak",
        };

        public static void Run(Options options)
        {
            try
            {
                var current = AppContext.BaseDirectory;
                var packed = Directory.CreateDirectory("packed").FullName;
                var assets = Path.Combine(options.GameDirectoryPath, "Assets");

                if (!Directory.Exists(assets))
                {
                    Console.WriteLine($"Specified directory for the game file sources does not exist '{assets}'.");
                    return;
                }

                Console.WriteLine();
                Console.WriteLine("Begin copying files to packed directory...");
                Console.WriteLine();
                
                foreach (var fileOfInterest in FilesOfInterest)
                {
                    var sourcePath = Path.Combine(assets, fileOfInterest.TrimEnd('/'));
                    var destination = sourcePath.Replace(assets, packed);
                
                    Console.WriteLine($"Copying {sourcePath}...");
                
                    if (fileOfInterest.EndsWith("/"))
                    {
                        // fileOfInterest is actually a directory, copy all
                        CopyFilesRecursively(sourcePath, destination);
                    }
                    else
                    {
                        File.Copy(sourcePath, destination, overwrite: true);
                    }
                
                    Console.WriteLine($"Done {destination}.");
                }

                var crySDK = Path.Combine(current, "tools", "win_x64");
                Console.WriteLine($"Copying CryGameSDK from {crySDK}...");
                var output = Path.Combine(packed, "win_x64");
                CopyFilesRecursively(crySDK , output);
                Console.WriteLine($"Done {output}...");

                var unpacked = Directory.CreateDirectory("unpacked").FullName;
                var wolcen_extractor = Path.Combine(current, "tools", "wolcen_extractor");

                Console.WriteLine();
                Console.WriteLine("Unpacking files...");
                
                using (var process = new Process())
                {
                    process.StartInfo.CreateNoWindow = false;
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = Path.Combine(wolcen_extractor, "dist", "wolcen_extractor.exe");
                    process.StartInfo.WorkingDirectory = wolcen_extractor;
                    process.StartInfo.Arguments = $"extract --source \"{packed}\" --dest \"{unpacked}\"";
                    
                    // process.StartInfo.RedirectStandardOutput = true;
                    // process.StartInfo.RedirectStandardError = true;
                    
                    // process.OutputDataReceived += ProcessOnOutputDataReceived;
                    // process.ErrorDataReceived += ProcessOnOutputDataReceived;
                
                    process.Start();
                    // process.BeginOutputReadLine();
                    // process.BeginErrorReadLine();
                    process.WaitForExit();
                }

                Console.WriteLine();
                Console.WriteLine("Done.");
                Console.WriteLine();

                Console.WriteLine("Cleaning up...");
                Directory.Delete(packed, recursive: true);

                Console.WriteLine();
                Console.WriteLine("Copying tools to unpacked directory...");
                Console.WriteLine();

                var audio = Path.Combine(unpacked, "audio", "wwise");
                var ww2ogg022 = Path.Combine(current, "tools", "ww2ogg022.zip");
                var bnkextr = Path.Combine(current, "tools", "bnkextr.exe");
                var revorb = Path.Combine(current, "tools", "revorb.exe");
                var convert_wem_to_ogg = Path.Combine(current, "tools", "convert_wem_to_ogg.bat");
                var startCitizenConverter = Path.Combine(current, "tools", "star-citizen-texture-converter");

                Console.WriteLine("Copying star-citizen-texture-converter...");
                CopyFilesRecursively(startCitizenConverter, unpacked);
                startCitizenConverter = Path.Combine(unpacked, "sctexconv_1.3.exe");
                Console.WriteLine($"Unzipping ww2ogg022...");
                ZipFile.ExtractToDirectory(ww2ogg022, audio, overwriteFiles: true);
                ww2ogg022 = Path.Combine(audio, "ww2ogg.exe");
                Console.WriteLine("Copying bnkextr.exe...");
                File.Copy(bnkextr, Path.Combine(audio, "bnkextr.exe"), overwrite: true);
                bnkextr = Path.Combine(audio, "bnkextr.exe");
                Console.WriteLine("Copying revorb.exe...");
                File.Copy(revorb, Path.Combine(audio, "revorb.exe"), overwrite: true);
                revorb = Path.Combine(audio, "revorb.exe");
                Console.WriteLine("Copying convert_wem_to_ogg.bat...");
                File.Copy(convert_wem_to_ogg, Path.Combine(audio, "convert_wem_to_ogg.bat"), overwrite: true);
                convert_wem_to_ogg = Path.Combine(audio, "convert_wem_to_ogg.bat");

                Console.WriteLine();
                Console.WriteLine("Converting textures...");
                Console.WriteLine("Running sctexconv_1.3.exe with config.txt...");

                using (var process = new Process())
                {
                    process.StartInfo.CreateNoWindow = false;
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = startCitizenConverter;
                    process.StartInfo.WorkingDirectory = unpacked;

                    // process.StartInfo.RedirectStandardOutput = true;
                    // process.StartInfo.RedirectStandardError = true;

                    // process.OutputDataReceived += ProcessOnOutputDataReceived;
                    // process.ErrorDataReceived += ProcessOnOutputDataReceived;

                    process.Start();
                    // process.BeginOutputReadLine();
                    // process.BeginErrorReadLine();
                    process.WaitForExit();
                }

                Console.WriteLine();
                Console.WriteLine("Done.");
                Console.WriteLine();
                Console.WriteLine("Converting audio...");
                Console.WriteLine("Running bnkextr.exe...");

                using (var process = new Process())
                {
                    process.StartInfo.CreateNoWindow = false;
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = bnkextr;
                    process.StartInfo.WorkingDirectory = audio;
                    process.StartInfo.Arguments = "sounds.bnk";

                    process.Start();
                    process.WaitForExit();
                }

                Console.WriteLine("Running convert_wem_to_ogg.bat...");

                using (var process = new Process())
                {
                    process.StartInfo.CreateNoWindow = false;
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = convert_wem_to_ogg;
                    process.StartInfo.WorkingDirectory = audio;

                    process.Start();
                    process.WaitForExit();
                }

                Console.WriteLine();
                Console.WriteLine("Done.");
                Console.WriteLine();
                Console.WriteLine("Running extractMaps.");
                Console.WriteLine();

                ExtractMaps.Run(new ExtractMaps.Options()
                {
                    DirectoryPath = unpacked
                });

                Console.WriteLine();
                Console.WriteLine("Done.");
                Console.WriteLine();
                Console.WriteLine("Running extractWeapons.");
                Console.WriteLine();

                ExtractWeapons.Run(new ExtractWeapons.Options()
                {
                    FileName = Path.Combine(unpacked, "localization", "english_xml", "text_ui_items.xml")
                });

                Console.WriteLine();
                Console.WriteLine("Done.");
                Console.WriteLine();
                Console.WriteLine("Running extractRegiments.");
                Console.WriteLine();

                ExtractRegiments.Run(new ExtractRegiments.Options()
                {
                    DirectoryPath = unpacked,
                    MapRegiments = "warofrights.mapregiments.json"
                });

                Console.WriteLine();
                Console.WriteLine("Done.");
                Console.WriteLine();
                Console.WriteLine("Running extractRegimentsImages.");
                Console.WriteLine();

                ExtractRegimentsImages.Run(new ExtractRegimentsImages.Options()
                {
                    PathName = Path.Combine(unpacked, "Libs", "UI", "Textures", "icons", "Regiments")
                });

                Console.WriteLine();
                Console.WriteLine("Done.");
                Console.WriteLine();
                Console.WriteLine("Running extractMisc.");
                Console.WriteLine();

                ExtractMisc.Run(new ExtractMisc.Options()
                {
                    FileName = Path.Combine(unpacked, "localization", "english_xml", "text_ui_menus.xml")
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

        }

        private static void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            Directory.CreateDirectory(targetPath);

            // Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            // Copy all the files & Replaces any files with the same name
            foreach (string path in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                var newPath = path.Replace(sourcePath, targetPath);
                File.Copy(path, newPath, overwrite: true);
            }
        }

        [Verb("unpackGameAssets", HelpText = "Copies game files of interest to a prepared directory for processing.")]
        public class Options
        {
            [Option(longName: "gameDir", shortName: 'd', Required = true, HelpText = "Directory path to the target game files (text_ui_menus.xml).")]
            public string GameDirectoryPath { get; set; }
        }
    }
}
