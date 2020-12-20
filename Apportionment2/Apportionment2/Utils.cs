using System;
using System.Collections.Generic;
using System.Linq;
using Apportionment2.Sqlite;
using Xamarin.Forms;

namespace Apportionment2
{
    public static class Utils
    {
        public static string SyncStatus(string tripId)
        {
            var a = App.Database.Table<Trips>().First(n => n.id == tripId);
            var sync = a.Sync;
            return sync;
        }

        public static List<Users> GetUsers(string tripId)
        {
            var sql = "Select distinct u.* from Users u " +
                      "left join CostValues cs on cs.UserId = u.id and cs.TripId = '" + tripId + "' " +
                      "left join UserCostShares us on us.UserId = u.id and us.TripId = '" + tripId + "' " +
                      "where cs.UserId IS NOT NULL or us.UserId IS NOT NULL ";

            var cmd = App.Database.CreateCommand(sql);
            return cmd.ExecuteQuery<Users>().ToList();
        }

        public static List<Users> GetOtherUsers(string tripId)
        {
            var sql = "Select distinct u.* from Users u " +
                      "left join CostValues cs on cs.UserId = u.id and cs.TripId = '" + tripId + "' " +
                      "left join UserCostShares us on us.UserId = u.id and us.TripId = '" + tripId + "' " +
                      "where cs.UserId IS NULL and us.UserId IS NULL ";

            var cmd = App.Database.CreateCommand(sql);
            return cmd.ExecuteQuery<Users>().ToList();
        }

        public static List<CurrencyDictionary> GetCurrencies(string tripId)
        {
            var sql = "Select distinct cd.* from CurrencyDictionary cd " +
                      "join TripCurrencies tc on tc.CurrencyId = cd.id and tc.TripId = '" + tripId + "' ";

            var cmd = App.Database.CreateCommand(sql);
            return cmd.ExecuteQuery<CurrencyDictionary>().ToList();
        }

        internal static Label GetLabel(object bindingContext, string text, double width, double height, double fontSize)
        {
            Label label = new Label
            {
                BindingContext = bindingContext,
                Text = text,
                FontSize = fontSize,
                TextColor = Color.FromHex("#708090"),
                BackgroundColor = Color.FromHex("#F5F5F5"),
                WidthRequest = width,
                HeightRequest = height,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };

            return label;
        }
        
        internal static Entry GetEntry(object bindingContext, double width, double fontSize, Color color)
        {
            Entry entry = new Entry
            {
                IsVisible = true,
                BindingContext = bindingContext,
                FontSize = fontSize,
                TextColor = color,
                Margin = 0,
                BackgroundColor = Color.FromHex("#F5F5F5"),
                WidthRequest = width,
                HeightRequest = 38,
                ClearButtonVisibility = ClearButtonVisibility.WhileEditing
            };

            return entry;
        }

        internal static Entry GetDoubleEntry(object bindingContext, double width, double fontSize, Color color)
        {
            Entry entry = Utils.GetEntry(bindingContext, width, fontSize, color);
            entry.HorizontalTextAlignment = TextAlignment.Center;
            entry.HorizontalOptions = LayoutOptions.End;
            entry.Keyboard = Keyboard.Numeric;
            return entry;
        }

        internal static StackLayout GetNewStackLayout(object bindingContext, StackOrientation orientation)
        {
            StackLayout stackLayout = new StackLayout
            {
                IsVisible = true,
                BindingContext = bindingContext,
                Orientation = orientation,
                BackgroundColor = Color.FromHex("#F5F5F5"),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Padding = 0
            };

            return stackLayout;
        }

        internal static TripCurrencies GetBaseTripCurrencies(string tripId)
        {
            var sql = "SELECT tc.* FROM TripCurrencies tc JOIN ObjectAttrs oa on oa.ObjectId = tc.id and oa.AttrId='1' " +
                      "where tc.TripId = '" + tripId + "' ";
            var cmd = App.Database.CreateCommand(sql);
            var cur = cmd.ExecuteQuery<TripCurrencies>();
            return cur.FirstOrDefault();
        }

        internal static bool AreAllRatesSet(string tripId)
        {
            TripCurrencies baseCurrencies = GetBaseTripCurrencies(tripId);

            var sql = "Select distinct * from TripCurrencies tv " +
                      "left join CurrencyExchangeRate cer on cer.CurrencyIdFrom = tv.CurrencyId and cer.CurrencyIdTo = '" + baseCurrencies.CurrencyId + "' " +
                      "where tv.TripId = '" + tripId + "' and tv.CurrencyId <> '" + baseCurrencies.CurrencyId+ "' "
            + "and (cer.id is null or cer.rate = 0)";
            var cmd = App.Database.CreateCommand(sql);
            var cur = cmd.ExecuteQuery<TripCurrencies>();
            return cur.Count > 0;
        }

            internal static DateTime DateFromString(string date)
        {
            if (string.IsNullOrEmpty(date) || date == "null")
                return DateTime.Today;
            else
                return  DateTime.ParseExact(date, App.DateFormat, System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
