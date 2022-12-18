using Newtonsoft.Json;

namespace WarOfRightsUnpack.Models
{
    public class WoRMap
    {
        [JsonProperty(Order = 1)]
        public string ID { get; set; }

        [JsonProperty(Order = 2)]
        public string Name { get; set; }

        [JsonProperty(Order = 3)]
        public string AreaName { get; set; }

        [JsonProperty(Order = 4)]
        public string Description { get; set; }

        [JsonProperty(Order = 5)]
        public string DateTimeDescription { get; set; }

        [JsonProperty(Order = 6)]
        public string DefendingTeam { get; set; }

        // [JsonProperty(Order = 7)]
        // public decimal? TransferOnDeath { get; set; }
        //
        // [JsonProperty(Order = 8)]
        // public int? RoundTime { get; set; }
        //
        // [JsonProperty(Order = 9)]
        // public int? WaveTime { get; set; }
        //
        // [JsonProperty(Order = 10)]
        // public decimal? CaptureSpeed { get; set; }
        //
        // [JsonProperty(Order = 11)]
        // public decimal? NeutralizeSpeed { get; set; }

        [JsonProperty(Order = 12)]
        public int? TicketsUSA { get; set; }

        [JsonProperty(Order = 13)]
        public int? TicketsCSA { get; set; }

        [JsonProperty(Order = 14)]
        public int? FinalPushTime { get; set; }


        [JsonProperty(Order = 15)]
        public string SkirmishImagePath { get; set; }

        [JsonProperty(Order = 16)]
        public string SpawnImagePath { get; set; }

        [JsonProperty(Order = 17)]
        public string LoadingImagePath { get; set; }


        [JsonProperty(Order = 18)]
        public int Order { get; set; }

        [JsonProperty(Order = 19)]
        public string NarratorInfo { get; set; }

        [JsonProperty(Order = 20)]
        public string MapType { get; set; }
    }

}
