using SQLite;
using System.Runtime.Serialization;


namespace Apportionment2.Sqlite
{
    /// <summary>
    /// Provides access to the SQL table "Trips"  
    /// </summary>
    [Table("Trips")]
    public class Trips
    {
        [PrimaryKey, Column("id")]
        // [PrimaryKey, AutoIncrement, Column("id")]
       //[JsonProperty(PropertyName = "id")]
        public string id { get; set; }

       // [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

       // [JsonProperty(PropertyName = "DateBegin")]
        public string DateBegin { get; set; }

       //[JsonProperty(PropertyName = "DateEnd")]
        public string DateEnd { get; set; }

       // [JsonProperty(PropertyName = "Sync")]
        public string Sync { get; set; }
    }
}
