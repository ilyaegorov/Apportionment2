using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace Apportionment2.Sqlite
{
    /// <summary>
    /// Provides access to the SQL table "Users"  
    /// </summary>
    [Table("TripUsers")]
    public class TripUsers
    {
        [PrimaryKey, Column("id")]
        public string id { get; set; }
        public string TripId { get; set; }
        public string UserId { get; set; }
        public string DateBegin { get; set; }
        public string DateEnd { get; set; }
        public string Sync { get; set; }
    }
}
