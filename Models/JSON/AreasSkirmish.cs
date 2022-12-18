// ReSharper disable IdentifierTypo

namespace WarOfRightsUnpack.Models.JSON
{
    public static class AreasSkirmish
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

        public class Handler
        {
            public string Type { get; set; }
            public string Team { get; set; }
            public string Spawnpoint { get; set; }
            public string Forward { get; set; }
            public string Staging { get; set; }
        }

        public class Map
        {
            public string ImagePath { get; set; }
            public int TopLeftX { get; set; }
            public int TopLeftY { get; set; }
            public int TopRightX { get; set; }
            public int TopRightY { get; set; }
            public int BottomRightX { get; set; }
            public int BottomRightY { get; set; }
        }

        public class Descriptor
        {
            public string Type { get; set; }
            public string Sequence { get; set; }
            public string DefendingTeam { get; set; }
            public int FinalPushTime { get; set; }
            public int TicketsUSA { get; set; }
            public int TicketsCSA { get; set; }
            public List<Handler> Handlers { get; set; }
            public Map Map { get; set; }
        }

        public class Operation
        {
            public string Type { get; set; }
            public List<string> Regiments { get; set; }
            public double? Value { get; set; }
        }

        public class GameplayArea
        {
            public string Name { get; set; }
            public string Layer { get; set; }
            public List<Descriptor> Descriptors { get; set; }
            public List<Operation> Operations { get; set; }
        }

        public class Component
        {
            public string Type { get; set; }
            public List<string> DisabledLayers { get; set; }
            public List<GameplayArea> GameplayAreas { get; set; }
        }

        public class Root
        {
            public List<string> Includes { get; set; }
            public List<Component> Components { get; set; }
        }
    }
}