using SQLite;

namespace Apportionment2.Sqlite
{
    /// <summary>
    /// Provides access to the SQL table "ObjectAttrs"  
    /// </summary>
    [Table("ObjectAttrs")]
    public class ObjectAttrs
    {
        [PrimaryKey, Column("id")]
        public string id { get; set; }
        public string ObjectId { get; set; }
        public string AttrId { get; set; }
        public string AttrValue { get; set; }
        public string Sync { get; set; }
    }
}
