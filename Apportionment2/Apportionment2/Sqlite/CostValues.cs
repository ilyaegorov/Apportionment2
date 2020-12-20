using SQLite;

namespace Apportionment2.Sqlite
{
    /// <summary>
    /// Provides access to the SQL table "CostValues"  
    /// </summary>
    [Table("CostValues")]
    public class CostValues
    {
        [PrimaryKey, Column("id")]
        public string id { get; set; }
        public string TripId { get; set; }
        public string CostId { get; set; }
        public string UserId { get; set; }
        public string CurrencyId { get; set; }
        public double Value { get; set; }
        public string Sync { get; set; }
    }
}
