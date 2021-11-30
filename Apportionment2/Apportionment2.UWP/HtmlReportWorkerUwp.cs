using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Apportionment2.Interfaces;
using Apportionment2.UWP;
using Xamarin.Forms;

[assembly: Dependency(typeof(HtmlReportWorkerUwp))]
namespace Apportionment2.UWP
{
    class HtmlReportWorkerUwp : IHtmlReport
    {
        public string GetPath(string filename)
        {
            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, filename);
            return path;
        }

        public async Task<List<string>> LoadFromTemlate(string filename)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile sampleFile = await storageFolder.GetFileAsync(filename);
            var text = System.IO.File.ReadAllLines(sampleFile.Path);
            return new List<string>(text);
        }

        public Task ReplaceMySqlDb()
        {
            throw new NotImplementedException();
        }

        public async Task SaveReportAsync(string filename, List<string> result)
        {
            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
            var file = await local.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            using (StreamWriter writer = new StreamWriter(await file.OpenStreamForWriteAsync()))
            {
                foreach (var s in result)
                {
                    writer.WriteLine(s);
                }
            }
        }

        public async Task SaveReportDocAsync(string filename, List<string> result)
        {
            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
            var file = await local.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            using (StreamWriter writer = new StreamWriter(await file.OpenStreamForWriteAsync()))
            {
                foreach (var s in result)
                {
                    writer.WriteLine(s);
                }
            }
        }

        public Task ShareDb()
        {
             throw new NotImplementedException();
        }

        public Task ShareHtmlAsync(string filename, List<string> result)
        {
            throw new NotImplementedException();
        }
    }
}
