using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Apportionment2.iOS;
using Apportionment2.Interfaces;
using Xamarin.Forms;

[assembly: Dependency(typeof(SqliteiOs))]
namespace Apportionment2.iOS
{
    class SqliteiOs : ISqLite
    {
        public string GetDatabasePath(string sqliteFilename)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string libraryPath = Path.Combine(documentsPath, "..", "Library"); 
            var path = Path.Combine(libraryPath, sqliteFilename);

            if (File.Exists(path))
                return path;

            File.Copy(sqliteFilename, path);
            return path;
        }
    }
}