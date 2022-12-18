namespace WarOfRightsUnpack.Models
{
    public class WoRMapRegiment
    {
        /// <summary>
        /// Construct manually as a combination of MapID + RegimentID.
        /// </summary>
        public string ID { get; set; }


        public string MapID { get; set; }

        public string RegimentID { get; set; }

        public int Order { get; set; }
    }
}
