using Apportionment2.Interfaces;
using Apportionment2.UWP;
using Xamarin.Forms;

[assembly: Dependency(typeof(CloseAppUwp))]
namespace Apportionment2.UWP
{
    public class CloseAppUwp : ICloseApplication
    {
        public void CloseApplication()
        {
            Windows.UI.Xaml.Application.Current.Exit();
        }
    }
}
