using System.Globalization;
using Apportionment2.Interfaces;
using Xamarin.Forms;

[assembly: Dependency(typeof(Apportionment2.UWP.LocalizeUwp))]
namespace Apportionment2.UWP
{
    public class LocalizeUwp : ILocalize
    {
        public void SetLocale(CultureInfo ci) { }

        public CultureInfo GetCurrentCultureInfo()
        {
            return CultureInfo.CurrentUICulture;
        }
    }
}
