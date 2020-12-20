using SQLite;

namespace Apportionment2.Sqlite
{
    /// <summary>
    /// Provides access to the SQL table "TripResults"  
    /// </summary>
    [Table("TripResults")]
    public class TripResults
    {
        [PrimaryKey, Column("Id")]
        // [PrimaryKey, AutoIncrement, Column("id")]
        //[JsonProperty(PropertyName = "id")]
        public string id { get; set; }

        public string TripId { get; set; }
        public string CostId { get; set; }
        public string UserId { get; set; }
        public double Spend { get; set; }
        public double Share { get; set; }
        public double Delta { get; set; }
    }
}
