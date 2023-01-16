﻿using System;
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
        public List<CurrencyDictionary> Currencies;

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
            Currencies = new List<CurrencyDictionary>();
            Currencies.Add(App.Database.Table<CurrencyDictionary>().First(c => c.id == "643")); // Российский рубль
            Currencies.Add(App.Database.Table<CurrencyDictionary>().First(c => c.id == "840")); // Доллар
            Currencies.Add(App.Database.Table<CurrencyDictionary>().First(c => c.id == "978")); // Евро
            Currencies.Add(App.Database.Table<CurrencyDictionary>().First(c => c.id == "398")); // Теньге
            Currencies.Add(App.Database.Table<CurrencyDictionary>().First(c => c.id == "933")); // Белорусский рубль

            Currencies.AddRange(App.Database.Table<CurrencyDictionary>()
                .Where(c => c.id != "933" && c.id != "643" && c.id != "840" && c.id != "978" && c.id != "398").ToList());

            CurrenciesListAll.ItemsSource = Currencies;
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.NewTextValue))
                CurrenciesListAll.ItemsSource = Currencies;
            else
                CurrenciesListAll.ItemsSource = 
                    Currencies.Where(currencies => currencies.Name.ToLower().Contains(e.NewTextValue.ToLower())
                    || currencies.Code.ToLower().Contains(e.NewTextValue.ToLower())
                    || currencies.id.ToLower().Contains(e.NewTextValue.ToLower())
                    );
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
                    SetBaseCurrency(newCurrency);

            }

            {
                await Navigation.PopModalAsync(false);
            }
        }

        private TripCurrencies CreateTripCurrencies(string tripId, string sync, string currencyId)
        {
           return SqlCrudUtils.CreateTripCurrencies( tripId,  sync,  currencyId);
        }

        private void SetBaseCurrency(TripCurrencies tripCurr)
        {
            SqlCrudUtils.SetBaseCurrency(tripCurr);
        }

        private CostValues CostValue { get; }
        Trips Trip { get;}
        private bool IsBaseCurrency { get; }
    }
}



