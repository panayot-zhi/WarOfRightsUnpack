
// ReSharper disable StringLiteralTypo

namespace WarOfRightsUnpack.Common
{
    public static class Constants
    {
        public const string Private = "Private";

        public const string Infantry = "Infantry";
        public const string Artillery = "Artillery";
        public const string Battery = "Battery";
        public const string Legion = "Legion";
        public const string Sharpshooters = "Sharpshooters";
        public const string HeavyArtillery = "HeavyArtillery";

        public const string SpongeRammer = "ArtilleryToolSpongeRammer";
        public const string ShellRammer = "ArtilleryToolShellRammer";

        public static Dictionary<string, string[]> MapAlternativeNames = new()
        {
            { "HookersPush", new[] { "HookersPush", "SkirmishHookersPush" } },
            { "HagerstownTurnpike", new[] { "HagerstownTurnpike", "SkirmishHagerstownTurnpike" } },
            { "MillersCornfield", new[] { "MillersCornfield", "MillersCornfieldHoodPush", "SkirmishMillersCornfieldHoodPush" } },
            { "EastWoods", new[] { "EastWoods", "EastWoodsHoodPush", "SkirmishEastWoodsHoodPush" } },
            { "EastWoodsSkirmish", new[] { "EastWoodsSkirmish", "EveningEastWoods", "SkirmishEveningEastWoods" } },
            { "BloodyLane", new[] { "BloodyLane", "RouletteBloodyLane", "SkirmishRouletteBloodyLane" } },
            { "PryFord", new[] { "PryFord", "SkirmishPryFord" } },
            { "PryGristMill", new[] { "PryGristMill", "SkirmishPryGristMill" } },
            { "PryHouse", new[] { "PryHouse", "SkirmishPryHouse" } },
            { "WestWoods", new[] { "WestWoods", "SkirmishWestWoods" } },
            { "DunkerChurch", new[] { "DunkerChurch", "SkirmishDunkerChurch" } },
            { "BurnsideBridge", new[] { "BurnsideBridge", "Burnside", "SkirmishBurnside" } },
            { "OttoSherrickFarm", new[] { "OttoSherrickFarm", "OttoSherrick", "SkirmishOttoSherrick" } },
            { "PiperFarm", new[] { "PiperFarm", "SkirmishPiperFarm" } },
            { "NicodemusHill", new[] { "NicodemusHill", "SkirmishNicodemusHill" } },
            { "CookesCountercharge", new[] { "CookesCountercharge", "CookesCounterattack", "SkirmishCookesCounterattack" } },
            { "RouletteLane", new[] { "RouletteLane", "SkirmishRouletteLane" } },
            { "HillsCounterattack", new[] { "HillsCounterattack", "SkirmishHillsCounterattack" } },
            { "MarylandHeights", new[] { "MarylandHeights", "SkirmishMarylandHeights", "Skirmish7MarylandHeights" } },
            { "RiverCrossing", new[] { "RiverCrossing", "SkirmishRiverCrossing", "Skirmish1RiverCrossing" } },
            { "Downtown", new[] { "Downtown", "SkirmishDowntown", "Skirmish2Downtown" } },
            { "HighStreet", new[] { "HighStreet", "SkirmishHighStreet", "Skirmish3HighStreet" } },
            { "ShenandoahStreet", new[] { "ShenandoahStreet", "SkirmishShenandoahStreet", "Skirmish4ShenandoahStreet" } },
            { "HarpersGraveyard", new[] { "HarpersGraveyard", "SkirmishHarpersGraveyard", "Skirmish5HarpersGraveyard" } },
            { "WashingtonStreet", new[] { "WashingtonStreet", "SkirmishWashingtonStreet", "Skirmish6WashingtonStreet" } },
            { "SchoolHouseRidge", new[] { "SchoolHouseRidge", "SchoolhouseRidge", "SkirmishSchoolHouseRidge", "Skirmish8SchoolHouseRidge" } },
            { "BolivarHeightsCamp", new[] { "BolivarHeightsCamp", "SkirmishBolivarHeightsCamp", "Skirmish9BolivarHeightsCamp" } },
            { "BolivarHeightsRedoubt", new[] { "BolivarHeightsRedoubt", "SkirmishBolivarHeightsRedoubt", "Skirmish10BolivarHeightsRedoubt" } },
            { "GarlandsStand", new[] { "GarlandsStand", "SkirmishFoxsGapGarlandsStand" } },
            { "HatchsAttack", new[] { "HatchsAttack", "SkirmishTurnersGapHatchsAttack" } },
            { "ColquittsDefence", new[] { "ColquittsDefence", "SkirmishTurnersGapColquittsDefence" } },
            { "RenosFall", new[] { "RenosFall", "SkirmishFoxsGapRenosFall" } },
            { "CoxsPush", new[] { "CoxsPush", "SkirmishFoxsGapCoxsPush" } },
            { "AndersonsCounterattack", new[] { "AndersonsCounterattack", "SkirmishFoxsGapAndersonsCounterattack" } },
            { "PicketPatrol", new[] { "PicketPatrol" } },
            { "HarpersFerryUSA", new[] { "HarpersFerryUSA" } },
            { "DrillCampUSA", new[] { "DrillCampUSA" } },
            { "DrillCampCSA", new[] { "DrillCampCSA" } }
        };
    }
}
