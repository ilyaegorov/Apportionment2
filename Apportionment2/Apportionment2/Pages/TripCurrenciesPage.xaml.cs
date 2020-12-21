using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apportionment2.Sqlite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Apportionment2.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TripCurrenciesPage : ContentPage
    {
        public TripCurrenciesPage(Trips trip)
        {
            InitializeComponent();
            _trip = trip;
        }

        protected override void OnAppearing()
        {
            //base.OnAppearing();
            Refresh();
            base.OnAppearing();
        }

        private void SetBaseCurrencyElements()
        {
            TripCurrencies baseCurrency = GetBaseTripCurrencies();

            if (baseCurrency == null)
            {
                _baseCurrency = App.Database.Table<CurrencyDictionary>().FirstOrDefault(n => n.Code == Resource.DefaultCurrencyCode);
                TripCurrencies tripCurr = SqlCrudUtils.CreateTripCurrencies(_trip.id, _trip.Sync, _baseCurrency.id); ;
                SqlCrudUtils.SetBaseCurrency(tripCurr);
            }
            else
            {
                _baseCurrency = App.Database.Table<CurrencyDictionary>().FirstOrDefault(n => n.id == baseCurrency.CurrencyId);
            }

            SetControlName(_baseCurrency);
        }

        private void Refresh()
        {
            Title = _trip.Name;
            LabelBaseCurrency.Text = Resource.TripCurrenciesPageBaseCurrency;
            ExchangeRatesLabel.Text = Resource.TripCurrenciesPageExchangeRates;
            SetBaseCurrencyElements();
            GetAllTripCurrencies();
            CreateExchangeRateList();
        }

        private void SetControlName(CurrencyDictionary curr)
        {
            CurrencyCode.Text = curr.Code;
            CurrencyNameLabel.Text = curr.Name;
        }

        internal TripCurrencies GetBaseTripCurrencies()
        {
            
            return Utils.GetBaseTripCurrencies(_trip.id);
        }

        public void GetAllTripCurrencies()
        {
            if (_baseCurrency == null)
                return;

            //var sql = "Select distinct cd.* from CostValues cv join CurrencyDictionary cd on cv.CurrencyId = cd.id " +
            //"where cd.id <> '" + _baseCurrency.id + "' ";
            //var cmd = App.Database.CreateCommand(sql);

            //var sql = "SELECT tc.* FROM TripCurrencies tc JOIN ObjectAttrs oa on oa.ObjectId = tc.id and oa.AttrId='1' " +
            //          "where tc.TripId = '" + _trip.id + "' ";

            var sql = "Select distinct cd.* from TripCurrencies tv join CurrencyDictionary cd on cd.id = tv.CurrencyId " +
            "where cd.id <> '" + _baseCurrency.id + "' and tv.TripId = '" + _trip.id + "'";

            var cmd = App.Database.CreateCommand(sql);
           _allOtherCurrencies = cmd.ExecuteQuery<CurrencyDictionary>().ToList();
        }

        private void AddCurrencyButton_OnClicked(object sender, EventArgs e)
        {
            CurrenciesPage currenciesPage = new CurrenciesPage(_trip, false);
            Navigation.PushAsync(currenciesPage);
        }

        private void BaseCurrencyStackLayout_OnTapped(object sender, EventArgs args)
        {
            CurrenciesPage currenciesPage = new CurrenciesPage(_trip, true);
            Navigation.PushAsync(currenciesPage);
        }

        private async void SelectCurrencyButton_OnClicked(object sender, EventArgs e)
        {
            //Button currencyButton = sender as Button;
            //CostValues cv = currencyButton.BindingContext as CostValues;
            //List<string> currenciesListStrings = new List<string>();
            //List<CurrencyDictionary> currenciesList = Utils.GetCurrencies(_tripId);
            //currenciesListStrings.Add(Resource.CostPageAddNewCurrencies);

            //foreach (CurrencyDictionary currency in currenciesList)
            //    currenciesListStrings.Add($"{currency.id} {currency.Code} {currency.Name}");

            //var currencyString = await DisplayActionSheet(null, Resource.Cancel, null, currenciesListStrings.ToArray());

            //if (currencyString == Resource.Cancel || currencyString == null)
            //{
            //    // do nothing
            //}
            //else if (currencyString == Resource.CostPageAddNewCurrencies)
            //{
            //    CurrenciesPage currenciesPage = new CurrenciesPage(cv);
            //    await Navigation.PushModalAsync(currenciesPage);
            //}
            //else
            //{
            //    int indexOfFirstSpace = currencyString.IndexOf(spaceChar, StringComparison.Ordinal);
            //    string id = currencyString.Substring(0, indexOfFirstSpace);
            //    CurrencyDictionary currency = currenciesList.FirstOrDefault(n => n.id == id);

            //    if (cv.CurrencyId != currency.id)
            //    {
            //        currencyButton.Text = currency.Code;
            //        cv.CurrencyId = currency.id;
            //        _hasUnsavedData = true;
            //    }
            //}
        }

        private void CreateExchangeRateList()
        {
            if (_baseCurrency == null)
                return;

            if (_allOtherCurrencies.Count == 0)
                return;

            _itemLayouts.Clear();

            // CurrenciesAbsoluteLayout.SetLa
            AbsoluteLayout.SetLayoutFlags(CurrenciesScroll, AbsoluteLayoutFlags.SizeProportional);
            AbsoluteLayout.SetLayoutBounds(CurrenciesScroll,new Rectangle(0, 140, 1, 0.75));

            StackLayoutScroll.Children.Clear();

            //Label exchangeRateLabel = GetTitleLabel(null, Resource.TripCurrenciesPageExchangeRates);
            //StackLayoutScroll.Children.Add(exchangeRateLabel);

            StackLayout exchangeRatesListLayout = Utils.GetNewStackLayout(null, StackOrientation.Vertical);
            int colorIndex = 0;

            foreach (var currency in _allOtherCurrencies)
            {
                CurrencyExchangeRate rate = App.Database.Table<CurrencyExchangeRate>()
                    .FirstOrDefault(n => n.CurrencyIdTo == _baseCurrency.id 
                                         && n.CurrencyIdFrom == currency.id
                                         && n.TripId == _trip.id);

                Color backGroundColor = GetBackgroundColor(((rate == null) ? 0d : rate.Rate), colorIndex);
                   
                if (rate == null)
                {
                    CurrencyExchangeRate newRate = new CurrencyExchangeRate
                    {
                        id = Guid.NewGuid().ToString(),
                        TripId = _trip.id,
                        CurrencyIdFrom = currency.id,
                        CurrencyIdTo = _baseCurrency.id,
                        Rate = 0d,
                        Sync = _trip.Sync
                    };

                    SqlCrudUtils.Save(newRate);
                    rate = newRate;
                }

                StackLayout exchangeRateItemLayout = Utils.GetNewStackLayout(currency, StackOrientation.Horizontal);
                exchangeRateItemLayout.BackgroundColor = backGroundColor;
                _itemLayouts.Add(exchangeRateItemLayout);
                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += (s, e) => ExchangeRateItemLayout_OnTapped(currency, null);
                //tapGestureRecognizer.Tapped += (s, e) => 
                //{
                //    var result =  DisplayAlert(Resource.CostPageUnsavedDataAlertTitle, Resource.CostPageUnsavedDataAlertMessage,
                //        Resource.Yes, Resource.No);

                //    if (result)
                //        SaveButton_OnClicked(null, null);
                //    else
                //        await Navigation.PopModalAsync(false);

                //};

                exchangeRateItemLayout.GestureRecognizers.Add(tapGestureRecognizer);

                //Elements of the paymentItemLayout.
                Label codeTo = Utils.GetLabel(currency, currency.Code, 50, 40, 15);
                Label nameTo = Utils.GetLabel(currency, currency.Name, 200, 40, 15);
                nameTo.HorizontalTextAlignment = TextAlignment.Start;
                //Label codeFrom= Utils.GetLabel(_baseCurrency, _baseCurrency.Code, 50, 40, 15);
                //Label nameFrom = Utils.GetLabel(_baseCurrency, _baseCurrency.Name, 200, 40, 15);
                Entry exchangeRateEntry = Utils.GetDoubleEntry(rate, 100, 15, Color.Black);

                exchangeRateEntry.BackgroundColor = backGroundColor;
                codeTo.BackgroundColor = backGroundColor;
                nameTo.BackgroundColor = backGroundColor;
                //codeFrom.BackgroundColor = backGroundColor;
                //nameFrom.BackgroundColor = backGroundColor;

                exchangeRateEntry.BackgroundColor = backGroundColor;
                exchangeRateEntry.TextChanged += exchangeRateEntry_Replaced;
                exchangeRateEntry.Focused += exchangeRateEntry_Focused;
                exchangeRateEntry.Text = $"{rate.Rate :0.00}";
                exchangeRateEntry.Unfocused += exchangeRateEntry_Unfocused;

                exchangeRateItemLayout.Children.Add(codeTo);
                exchangeRateItemLayout.Children.Add(nameTo);
                //exchangeRateItemLayout.Children.Add(codeFrom);
                //exchangeRateItemLayout.Children.Add(nameFrom);
                exchangeRateItemLayout.Children.Add(exchangeRateEntry);
                exchangeRatesListLayout.Children.Add(exchangeRateItemLayout);
                StackLayoutScroll.Children.Add(exchangeRatesListLayout);
                colorIndex++;
            }
        }

        private async void ExchangeRateItemLayout_OnTapped(object sender, TextChangedEventArgs e)
        {
            var result = await DisplayAlert(null, Resource.TripCurrenciesPageDeleteCurrencyFromTrip,
                Resource.Yes, Resource.No);

            if (!result)
                return;
            
            CurrencyDictionary currency = sender as CurrencyDictionary;

            if (currency == null)
                return;

            if (App.Database.Table<CostValues>().Any(n => n.CurrencyId == currency.id))
            {
                await DisplayAlert(null, Resource.TripCurrenciesPageCanNotDeleteCurrencyFromTrip, Resource.Ok);
                return;
            }
            else
            {
                TripCurrencies tripCurrency = 
                    App.Database.Table<TripCurrencies>().FirstOrDefault(n => n.CurrencyId == currency.id);

                SqlCrudUtils.Delete(tripCurrency);
                Refresh();
            }
        }

        private void exchangeRateEntry_Replaced(object sender, TextChangedEventArgs e)
        {
            Entry entry = sender as Entry;
            entry.Text = entry.Text.Replace(",", ".");
        }

        private void exchangeRateEntry_Focused(object sender, FocusEventArgs e)
        {
            Entry entry = sender as Entry;

            if (entry.Text == "0.00")
                entry.Text = "";
        }

        public void exchangeRateEntry_Unfocused(object sender, FocusEventArgs e)
        {
            Entry entry = sender as Entry;

            if (entry == null)
                return;

            CurrencyExchangeRate rate = entry.BindingContext as CurrencyExchangeRate;

            double inputSum = 0;

            if (rate == null)
                return;
            
            if (double.TryParse(entry.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out inputSum))
            {
                rate.Rate = inputSum;
                SqlCrudUtils.Save(rate);
                SetColor(entry, inputSum);
            }
            else
            {
                entry.Text = $"{rate.Rate:0.00}";
            }
        }

        private void SetColor(Entry entry, double value)
        {
            if (entry.Parent is StackLayout itemLayout)
            {
                int index = _itemLayouts.IndexOf(itemLayout);
                Color backGroundColor = GetBackgroundColor(value, index);
                itemLayout.BackgroundColor = backGroundColor;

                foreach (var child in itemLayout.Children)
                    child.BackgroundColor = backGroundColor;
            }
        }

        private Color GetBackgroundColor(double value, int index)
        {
            if (value == 0)
                return Color.FromHex("#FCDCDC");

            return (index % 2 == 0) ? Color.FromHex("#F5F5F5") : Color.FromHex("#FFFFFF");
        }

        Trips _trip;
        private CurrencyDictionary _baseCurrency;
        private List<CurrencyDictionary> _allOtherCurrencies;
        private List<StackLayout> _itemLayouts = new List<StackLayout>();
    }
}