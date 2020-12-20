using System.IO;
using Windows.Storage;
using Apportionment2.Interfaces;
using Apportionment2.UWP;
using Xamarin.Forms;

[assembly: Dependency(typeof(SqliteUwp))]
namespace Apportionment2.UWP
{
    public class SqliteUwp : ISqLite
    {
        public SqliteUwp() { }
        public string GetDatabasePath(string sqliteFilename)
        {
            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, sqliteFilename);
            return path;
        }
    }
}
