using SQLite;

namespace Apportionment2.Sqlite
{
    /// <summary>
    /// Provides access to the SQL table "CurrencyExchangeRate"  
    /// </summary>
    [Table("CurrencyExchangeRate")]
    public class CurrencyExchangeRate
    {
        [PrimaryKey, Column("ID")]
        public string id { get; set; }
        public string TripId { get; set; }
        public string CurrencyIdFrom { get; set; }
        public string CurrencyIdTo { get; set; }
        public double Rate { get; set; }
        public string Sync { get; set; }
        public string DateCreate { get; set; }
    }
}
