using SQLite;

namespace Apportionment2.Sqlite
{
    /// <summary>
    /// Provides access to the SQL table "CostValues"  
    /// </summary>
    [Table("TripCurrencies")]
    public class TripCurrencies
    {
        [PrimaryKey, Column("id")]
        public string id { get; set; }
        public string TripId { get; set; }
        public string CurrencyId { get; set; }
        public string Sync { get; set; }
        public string DateCreate { get; set; }
    }
}
