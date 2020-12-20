using SQLite;

namespace Apportionment2.Sqlite
{
    [Table("AlcoDict")]
    public class AlcoDict
    {
        [PrimaryKey, Column("ID")]
        public string id { get; set; }
        public string Text { get; set; }
    }
}
