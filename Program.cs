
using CommandLine;
using System.Globalization;
using WarOfRightsUnpack.Main;

Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

Parser.Default.ParseArguments<
        UnpackGameAssets.Options,
        ExtractMaps.Options,
        ExtractWeapons.Options,
        ExtractRegiments.Options,
        ExtractRegimentsImages.Options,
        ExtractMisc.Options
    >(args)
    .WithParsed<UnpackGameAssets.Options>(UnpackGameAssets.Run)
    .WithParsed<ExtractMaps.Options>(ExtractMaps.Run)
    .WithParsed<ExtractWeapons.Options>(ExtractWeapons.Run)
    .WithParsed<ExtractRegiments.Options>(ExtractRegiments.Run)
    .WithParsed<ExtractRegimentsImages.Options>(ExtractRegimentsImages.Run)
    .WithParsed<ExtractMisc.Options>(ExtractMisc.Run);

Console.WriteLine();
Console.WriteLine("Done.");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();