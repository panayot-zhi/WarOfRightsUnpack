
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global

namespace WarOfRightsUnpack.Models.JSON
{
    public class EquipmentScript
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

        public class Company
        {
            public string Identifier { get; set; }
            public string Name { get; set; }
            public int PlayerThreshold { get; set; }
        }

        public class Loadout
        {
            public List<string> Primaries { get; set; }
            public List<string> Secondaries { get; set; }
            public List<string> Specials { get; set; }
        }

        public class Class
        {
            public string Rank { get; set; }
            public int RestrictionLimit { get; set; }
            public List<string> Skins { get; set; }
            public Loadout Loadout { get; set; }
        }

        public class ClassType
        {
            public string Type { get; set; }
            public int RestrictionLimit { get; set; }
            public bool Demotable { get; set; }
            public List<Class> Classes { get; set; }
        }

        public class Root
        {
            public string Identifier { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Team { get; set; }
            public string State { get; set; }
            public List<Company> Companies { get; set; }
            public List<ClassType> ClassTypes { get; set; }
        }
    }
}