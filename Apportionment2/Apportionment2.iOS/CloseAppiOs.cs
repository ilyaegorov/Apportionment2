using System.Threading;
using Apportionment2.iOS;
using Apportionment2.Interfaces;
using Xamarin.Forms;

[assembly: Dependency(typeof(CloseAppiOs))]

namespace Apportionment2.iOS
{
    public class CloseAppiOs : ICloseApplication
    {
        public void CloseApplication()
        {
            Thread.CurrentThread.Abort();
        }
    }
}