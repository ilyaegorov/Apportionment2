using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Android;
using Android.Content.PM;
using Android.Support.V4.Content;
using Apportionment2.Droid;
using Apportionment2.Interfaces;
using Xamarin.Essentials;
using Xamarin.Forms;
using Environment = System.Environment;
using File = System.IO.File;

[assembly: Dependency(typeof(HtmlReportWorkerDroid))]
namespace Apportionment2.Droid
{
    class HtmlReportWorkerDroid : IHtmlReport
    {
        public string GetPathOld(string filename)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var path = Path.Combine(documentsPath, filename);
            var fileAssetsStream = Android.App.Application.Context.Assets.Open(filename);
            var fileItogStream = new System.IO.FileStream(path, System.IO.FileMode.OpenOrCreate);
            var buffer = new byte[1024];
            int b = buffer.Length;
            int length;

            while ((length = fileAssetsStream.Read(buffer, 0, b)) > 0)
                fileItogStream.Write(buffer, 0, length);

            fileItogStream.Flush();
            fileItogStream.Close();
            fileAssetsStream.Close();

            return path;
        }

        public string GetPath(string filename)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var path = Path.Combine(documentsPath, filename);
            return path;
        }

        public Task<List<string>> LoadFromTemlate(string filename)
        {
            return Task.FromResult(System.IO.File.ReadAllLines(GetPathOld(filename)).ToList());
        }

        public async Task SaveReportAsync(string filename, List<string> result)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var path = Path.Combine(documentsPath, filename);
            File.WriteAllLines(path, result);
        }

        public async Task SaveReportDocAsync(string filename, List<string> result)
        {
            // Get the path to a file on internal storage
            //var backingFile = Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "count.txt");
            // var dirPath = Android.App.Application.Context.GetExternalFilesDir(null)+ "/TwoApportion2";
            //// var dirPath = Android.App.Application.Context.GetExternalFilesDir(null) + "/TwoApportion";
            // var exists = Directory.Exists(dirPath);

            // if (!exists)
            // Directory.CreateDirectory(dirPath);

            // var filepath = dirPath + "/" + filename;

            // //var newfile = new Java.IO.File(dirPath, filename);

            // FileWriter writer = new FileWriter(filepath);
            // foreach (string str in result)
            // {
            //     writer.Write(str + "\n");
            // }
            // writer.Close();

            //       using (FileOutputStream outfile = new FileOutputStream(newfile))
            //       {
            //           outfile.
            //           outfile.Write(System.Text.Encoding.ASCII.G(result));
            //           outfile.Close();
            //       }
            //   }

            //   string pathToDir =Android.App.Application.Context.GetExternalFilesDir(null).AbsolutePath;
            ////   string path = Path.Combine(pathToDir,filename);
            //   File.WriteAllLines(path, result);
            //  Android.OS.DropBoxManager

            //ContentValues values = new ContentValues();
            //values.Put(MediaStore.MediaColumns.Title, );

            //var musicUri = Android.Provider.MediaStore.Audio.Media.ExternalContentUri;
            //var musicCursor = musicResolver.Query(musicUri, null, null, null, null);

            //Android.OS.BuildVersionCodes version = Android.OS.Build.VERSION.SdkInt;
            //string sdk = (Android.OS.Build.VERSION.Sdk);
            CreateFile(filename, result);
        }

        public async Task ShareHtmlAsync(string filename, List<string> result)
        {
            CreateFile(filename, result);
            var path = GetFullFilePath(filename);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = filename,
                File = new ShareFile(path)
            });


        }

        public async Task ShareDb()
        {
            string fileName = "Trips.db";
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string dbPath = Path.Combine(documentsPath, fileName);
            var bytes = await Task.FromResult(System.IO.File.ReadAllBytes(dbPath));
            int.TryParse(Android.OS.Build.VERSION.Sdk, out int apiVersion);
            CheckAppPermissions(apiVersion);
            string newfileName = "TripsExp.zip";
            var newPath = GetFullFilePath(newfileName);

            System.IO.File.WriteAllBytes(newPath, bytes);
            //string path = DependencyService.Get<ISqLite>().GetDatabasePath(App.DatabaseName);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = fileName,
                File = new ShareFile(newPath)
            });
        }

        private void CheckAppPermissions(int version)
        {
            if (version < 23)
                return;

            ContextCompat.CheckSelfPermission(Android.App.Application.Context,Manifest.Permission.ReadExternalStorage);
            ContextCompat.CheckSelfPermission(Android.App.Application.Context,Manifest.Permission.WriteExternalStorage);
        }

        private string GetFullFilePath(string filename)
        {
            int.TryParse(Android.OS.Build.VERSION.Sdk, out int apiVersion);

            string documentsPath = (apiVersion < 23)
                ? Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).Path
                : Android.App.Application.Context.GetExternalFilesDir(null).AbsolutePath;

            return Path.Combine(documentsPath, filename);
        }

        private void CreateFile(string filename , List<string> result)
        {
            int.TryParse(Android.OS.Build.VERSION.Sdk, out int apiVersion);
            CheckAppPermissions(apiVersion);
            var path = GetFullFilePath(filename);
            File.WriteAllLines(path, result);
        }

        public string GetVersion()
        {
            var context = global::Android.App.Application.Context;

            PackageManager manager = context.PackageManager;
            PackageInfo info = manager.GetPackageInfo(context.PackageName, 0);

            return info.VersionName;
        }

        public int GetBuild()
        {
            var context = global::Android.App.Application.Context;
            PackageManager manager = context.PackageManager;
            PackageInfo info = manager.GetPackageInfo(context.PackageName, 0);

            return info.VersionCode;
        }

        public async Task ReplaceMySqlDb()
        {
            //var customFileType =

            //    (new Dictionary<DevicePlatform, IEnumerable<string>>
            //{
            //    { DevicePlatform.iOS, new[] { "public.my.comic.extension" } }, // or general UTType values
            //    { DevicePlatform.Android, new[] { "*.db" } },
            //    { DevicePlatform.UWP, new[] { "*.db"} },
            //});

            //var options = await CrossFilePicker.Current.PickFile(fileTypes);
            //{
            //    PickerTitle = "Select file with data base",
            //    FileTypes = customFileType,
            //};

            //var result = await CrossFilePicker.Current.PickFile(new[] string[] { "*.db" });

            //if (result != null)
            //{
            //    var stream = await result.OpenReadAsync();
            //    string fileName = "Trips.db";
            //    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            //    string dbPath = Path.Combine(documentsPath, fileName);
            //    byte[] bytesInStream = new byte[stream.Length];
            //    stream.Read(bytesInStream, 0, bytesInStream.Length);
            //    System.IO.File.WriteAllBytes(dbPath, bytesInStream);
            //}
        }
        //private void saveHtml()
        //{
        //    this.ContentResolver.Insert(Android.Provider.MediaStore.Images.Media.ExternalContentUri, values);

        //    File imageDirectory = new File("/sdcard/signifio");
        //    String path = imageDirectory.toString().toLowerCase();
        //    String name = imageDirectory.getName().toLowerCase();
        //    ContentValues values = new ContentValues();
        //    values.put(MediaStore.Images.Media.TITLE, "Image");
        //    values.put(MediaStore.Images.Media.BUCKET_ID, path.hashCode());
        //    values.put(MediaStore.Images.Media.BUCKET_DISPLAY_NAME, name);

        //    values.put(MediaStore.Images.Media.MIME_TYPE, "image/jpeg");
        //    values.put(MediaStore.Images.Media.DESCRIPTION, "Image capture by camera");
        //    values.put("_data", "/sdcard/signifio/1111.jpg");
        //    uri = getContentResolver().insert(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, values);
        //    Intent i = new Intent("android.media.action.IMAGE_CAPTURE");

        //    i.putExtra(MediaStore.EXTRA_OUTPUT, uri);

        //    startActivityForResult(i, 0);
        //}
    }
}