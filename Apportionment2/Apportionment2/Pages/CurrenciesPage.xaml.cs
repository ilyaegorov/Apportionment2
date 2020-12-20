using System;
using System.Collections.Generic;
using Apportionment2.Sqlite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Linq;

namespace Apportionment2.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CurrenciesPage : ContentPage
    {
        public IList<CurrencyDictionary> Currencies;

        public CurrenciesPage()
        {
            Initialize();
           
        }

        public CurrenciesPage(CostValues сostValues)
        { 
            Initialize();
            CostValue = сostValues;
        }

        public CurrenciesPage(Trips trip , bool isBaseCurrency)
        {
            Initialize();
            Trip = trip;
            IsBaseCurrency = isBaseCurrency;
        }

        private void Initialize()
        {
            InitializeComponent();
            Currencies = App.Database.Table<CurrencyDictionary>().ToList();
            CurrenciesListAll.ItemsSource = Currencies;
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.NewTextValue))
                CurrenciesListAll.ItemsSource = Currencies;
            else
                CurrenciesListAll.ItemsSource = 
                    Currencies.Where(currencies => currencies.Name.ToLower().Contains(e.NewTextValue.ToLower()));
        }

        private async void CurrenciesList_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            CurrencyDictionary currency = e.Item as CurrencyDictionary;
            string tripId = (CostValue == null) ? Trip.id : CostValue.TripId;
            string sync = (CostValue == null) ? Trip.Sync : CostValue.Sync;

            TripCurrencies newCurrency =
                App.Database.Table<TripCurrencies>().FirstOrDefault(c => c.CurrencyId == currency.id && c.TripId == tripId);
           
            if (currency != null)
            {
                if (newCurrency == null)
                    newCurrency = CreateTripCurrencies(tripId, sync, currency.id);

                if (CostValue != null)
                    CostValue.CurrencyId = currency.id;

                if (IsBaseCurrency)
                {
                    SetBaseCurrency(newCurrency);
                   // TripCurrenciesPage tcp = new TripCurrenciesPage(Trip);
                   //await Navigation.PushModalAsync(tcp);
                }

            }

            await Navigation.PopModalAsync(false);
        }

        private TripCurrencies CreateTripCurrencies(string tripId, string sync, string currencyId)
        {
            TripCurrencies newTripCurrency = new TripCurrencies
            {
                id = Guid.NewGuid().ToString(),
                TripId = tripId,
                CurrencyId = currencyId,
                DateCreate = DateTime.Now.ToString(App.DateFormat),
                Sync = sync
            };

            SqlCrudUtils.Save(newTripCurrency);

            return newTripCurrency;
        }

        private void SetBaseCurrency(TripCurrencies tripCurr)
        {
            //ObjectAttrs a = App.Database.Table<ObjectAttrs>()
            //    .FirstOrDefault(c => c.AttrId == "1");

            var attrs = App.Database.Table<ObjectAttrs>()
                .Where(c => c.AttrId == "1" && tripCurr.id != c.ObjectId);

            foreach (var a in attrs)
                SqlCrudUtils.Delete(a);

            ObjectAttrs newAttr = new ObjectAttrs
            {
                id = Guid.NewGuid().ToString(),
                AttrId = "1",
                AttrValue = "BaseCurrency",
                ObjectId = tripCurr.id,
                Sync = tripCurr.Sync
            };

            SqlCrudUtils.Save(newAttr);

            //var sql = "SELECT tc.* FROM TripCurrencies tc JOIN ObjectAttrs oa on oa.ObjectId = tc.id and oa.AttrId='1' " +
            //          "where tc.TripId= '" + Trip.id + "' ";
            //var cmd = App.Database.CreateCommand(sql);
            //var cur = cmd.ExecuteQuery<TripCurrencies>();
        }

        private CostValues CostValue { get; }
        Trips Trip { get;}
        private bool IsBaseCurrency { get; }
    }
}



