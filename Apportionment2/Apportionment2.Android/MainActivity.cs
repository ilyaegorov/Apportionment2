using System;
using Android;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.Support.V4.App;
using Rg.Plugins.Popup.Extensions;
using Xamarin.Forms.Platform.Android;
using Xamarin.Essentials;

namespace Apportionment2.Droid
{
    [Activity(Label = "Apportion", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]

    //[IntentFilter(new[] { Intent.ActionSend }, Categories = new[] { Intent.CategoryDefault }, DataMimeType = @"application/db")]
    [IntentFilter(new[] { Intent.ActionSend }, Categories = new[] { Intent.CategoryDefault }, 
        DataMimeTypes = new string[] { @"application/rar", @"application/db",  @"application/zip" })]
    // [IntentFilter(new[] { Intent.ActionSend }, Categories = new[] { Intent.CategoryDefault }, DataMimeType = @"application/rar")]

    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
          

            LoadApplication(new App());

            if (Intent.Action == Intent.ActionSend)
            {
                // Get Uri of export file.
                var uriFromExtras = Intent.GetParcelableExtra(Intent.ExtraStream) as Android.Net.Uri;
                var subject = Intent.GetStringExtra(Intent.ExtraSubject);

                // Get the info from ClipData 
                var db = Intent.ClipData.GetItemAt(0);

                // Open a stream from the URI 
                var dbStream = ContentResolver.OpenInputStream(db.Uri);

                byte[] bytesInStream = new byte[dbStream.Length];
                dbStream.Read(bytesInStream, 0, bytesInStream.Length);

                // Save it over 
                //var memOfDd = new System.IO.MemoryStream();
                //memOfDd.CopyTo(dbStream);

                string fileName = "Trips.db";
                string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                string dbPath = System.IO.Path.Combine(documentsPath, fileName);
                System.IO.File.WriteAllBytes(dbPath, bytesInStream);
               // System.IO.File.WriteAllBytes(dbPath, memOfDd.ToArray());

            }
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

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }



}