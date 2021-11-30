using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apportionment2.Interfaces
{
    public interface IHtmlReport
    {
        string GetPath(string filename);

        Task<List<string>> LoadFromTemlate(string filename);

        Task SaveReportAsync(string filename, List<string> result);
        Task SaveReportDocAsync(string filename, List<string> result);
        Task ShareHtmlAsync(string filename, List<string> result);
        Task ShareDb();
        Task ReplaceMySqlDb();
    }
}
