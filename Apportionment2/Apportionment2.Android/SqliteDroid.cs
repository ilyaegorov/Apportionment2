using Apportionment2.Droid;
using Apportionment2.Interfaces;
using Xamarin.Forms;
using System.IO;
using System;

[assembly: Dependency(typeof(SqliteDroid))]
namespace Apportionment2.Droid
{
    public class SqliteDroid : ISqLite
    {
        public string GetDatabasePath(string sqliteFilename)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var path = Path.Combine(documentsPath, sqliteFilename);

            if (File.Exists(path))
                return path;

            var dbAssetStream = Forms.Context.Assets.Open(sqliteFilename);
            var dbFileStream = new System.IO.FileStream(path, System.IO.FileMode.OpenOrCreate);
            var buffer = new byte[1024];

            int b = buffer.Length;
            int length;

            while ((length = dbAssetStream.Read(buffer, 0, b)) > 0)
                dbFileStream.Write(buffer, 0, length);

            dbFileStream.Flush();
            dbFileStream.Close();
            dbAssetStream.Close();

            return path;
        }
    }
}