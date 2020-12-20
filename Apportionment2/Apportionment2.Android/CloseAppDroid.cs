using Apportionment2.Droid;
using Apportionment2.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: Dependency(typeof(CloseAppDroid))]
namespace Apportionment2.Droid
{
    public class CloseAppDroid : ICloseApplication
    {
        public void CloseApplication()
        {
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        }
    }
}