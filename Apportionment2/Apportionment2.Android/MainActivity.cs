using System;
using Android;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V4.App;
using Rg.Plugins.Popup.Extensions;
using Xamarin.Forms.Platform.Android;

namespace Apportionment2.Droid
{
    [Activity(Label = "Apportion", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            LoadApplication(new App());

            // CheckAppPermissions();
        }

        private void CheckAppPermissions()
        {
            int.TryParse(Android.OS.Build.VERSION.Sdk, out int apiVersion);

            if (apiVersion < 23)
                return;

            var context = Application.Context;
            PackageManager manager = context.PackageManager;

            if (manager.CheckPermission(Manifest.Permission.ReadExternalStorage, context.PackageName) != Permission.Granted && 
                manager.CheckPermission(Manifest.Permission.WriteExternalStorage, context.PackageName) != Permission.Granted)
            {
                var permissions = new string[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage };
                ActivityCompat.RequestPermissions(this, permissions, 1);
            }
        }
    }


}