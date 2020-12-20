using SQLite;

namespace Apportionment2.Sqlite
{
    /// <summary>
    /// Provides access to the SQL table "UserCostShares"  
    /// </summary>
    [Table("UserCostShares")]
    public class UserCostShares
    {
        [PrimaryKey, Column("id")]
        public string id { get; set; }
        public string CostId { get; set; }
        public string UserId { get; set; }
        public string TripId { get; set; }
        public double Share { get; set; }
        public string Sync { get; set; }
    }
}
