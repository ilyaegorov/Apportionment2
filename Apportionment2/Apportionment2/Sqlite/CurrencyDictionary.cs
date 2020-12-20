using SQLite;

namespace Apportionment2.Sqlite
{
    /// <summary>
    /// Provides access to the SQL table "CurrencyDictionary"  
    /// </summary>
    [Table("CurrencyDictionary")]
    public class CurrencyDictionary
    {
        [PrimaryKey, Column("ID")]
        public string id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
