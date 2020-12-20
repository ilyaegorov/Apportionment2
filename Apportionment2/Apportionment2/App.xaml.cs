using System;
using System.Linq;
using Apportionment2.Interfaces;
using Apportionment2.Pages;
using Apportionment2.Sqlite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SQLite;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Apportionment2
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            Resource.Culture = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();

            // If exists open trips - load it.
            if (Database.Table<Trips>().Any(n => n.DateEnd == DataEnd))
            {
                Trips openTrip =Database.Table<Trips>().
                    Where(n => n.DateEnd == DataEnd).OrderByDescending(n => n.DateBegin).
                    FirstOrDefault();

                CostsPage costsPage = new CostsPage(openTrip);
                MainPage = new NavigationPage(costsPage);
            }
            else
            {
                MainPage = new NavigationPage(new StartPage());
            }
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        public static SQLiteConnection Database = new SQLiteConnection(DependencyService.Get<ISqLite>().GetDatabasePath(DatabaseName));
        public const string DatabaseName = "Trips.db";
        public const string DateFormat = "yyyy-MM-dd HH-mm-ss";
        public static string DataEnd = SystemDate.ToString(DateFormat);
        public static DateTime SystemDate = new DateTime(2099, 01, 01);
    }
}
