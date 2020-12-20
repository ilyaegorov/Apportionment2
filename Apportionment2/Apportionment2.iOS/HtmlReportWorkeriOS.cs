using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using Xamarin.Forms;
using System.Threading.Tasks;
using Apportionment2.iOS;
using Apportionment2.Interfaces;

[assembly: Dependency(typeof(HtmlReportWorkeriOS))]
namespace Apportionment2.iOS
{
    class HtmlReportWorkeriOS : IHtmlReport
    {
        public string GetPath(string filename)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string libraryPath = Path.Combine(documentsPath, "..", "Library");
            var path = Path.Combine(libraryPath, filename);

            if (!File.Exists(path))
            {
                File.Copy(filename, path);
            }
            return path;
        }

        public Task<List<string>> LoadFromTemlate(string filename)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var filePath = Path.Combine(documentsPath, filename);
            return Task.FromResult(File.ReadAllLines(filePath).ToList());
        }

        public async Task SaveReportAsync(string filename, List<string> result)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var path = Path.Combine(documentsPath, filename);
            File.WriteAllLines(path, result);
        }

        public async Task SaveReportDocAsync(string filename, List<string> result)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var path = Path.Combine(documentsPath, filename);
            File.WriteAllLines(path, result);
        }

        public Task ShareHtmlAsync(string filename, List<string> result)
        {
            throw new NotImplementedException();
        }
    }
}