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

            var sql = "CREATE TABLE IF NOT EXISTS TripUsers (id text NOT NULL PRIMARY KEY, " +
                      " TripId text, UserId text, " +
                      " DateBegin text, DateEnd text, Sync text)";
            var cmd = App.Database.CreateCommand(sql);
            cmd.ExecuteNonQuery();

            sql = "Insert into TripUsers(id, UserId, TripId) select  ROW_NUMBER() OVER (ORDER BY UserId) row_num, * from ( "
                      + " select cv.UserId, cv.TripId from CostValues cv "
                      + " left join TripUsers tu on tu.UserId = cv.UserId and tu.TripId = cv.TripId "
                      + " where cv.UserId is not null and tu.UserId is null "
                      + " group by cv.UserId, cv.TripId "
                      + " Union ALL "
                      + " select ucs.UserId, ucs.TripId from UserCostShares ucs "
                      + " left join TripUsers tu on tu.UserId = ucs.UserId and tu.TripId = ucs.TripId "
                      + " where ucs.UserId is not null and tu.UserId is null group by ucs.UserId, ucs.TripId) "
                      + " group by UserId,TripId ";

            cmd = App.Database.CreateCommand(sql);
            cmd.ExecuteNonQuery();

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
        public static string DataEnd = new DateTime(2099, 01, 01).ToString(DateFormat);
    }
}
