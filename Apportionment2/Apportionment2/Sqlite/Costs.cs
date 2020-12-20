using SQLite;

namespace Apportionment2.Sqlite
{
    /// <summary>
    /// Provides access to the SQL table "Costs"  
    /// </summary>
    [Table("Costs")]
    public class Costs
    {
        [PrimaryKey, Column("id")]
        public string id { get; set; }
        public string TripId { get; set; }
        public string CostName { get; set; }
        public string Sync { get; set; }
        public string DateCreate { get; set; }
    }
}
