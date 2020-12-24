using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apportionment2.Sqlite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Apportionment2.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TripNamePage : ContentPage
	{
		public TripNamePage ()
		{
		    _trip = new Trips
            {
		        id = Guid.NewGuid().ToString(),
                DateBegin = DateTime.Now.ToString(App.DateFormat),
                DateEnd = App.DataEnd,
                Sync = "false"
		    };

            SqlCrudUtils.Save(_trip);

            InitializeComponent ();
		}

	    public TripNamePage(Trips trip)
	    {
	        _trip = trip;
            InitializeComponent();
	    }

        protected override void OnAppearing()
        {
            Refresh();
            base.OnAppearing();
	       
	    }

	    private void Refresh()
	    {
            TripNameEntry.Placeholder = Resource.TripNamePageTripNamePlaceholder;
            TripNameEntry.PlaceholderColor = Color.Gray;
            TripNameEntry.Text = _trip.Name;

            //   if (string.IsNullOrEmpty(_trip.Name))
            //    TripNameEntry.Placeholder = Resource.TripNamePageTripNamePlaceholder;
            //else
            //    TripNameEntry.Text = _trip.Name;

            //   TripNameEntry.PlaceholderColor = Color.Gray;

            DateStartLabel.Text = Resource.TripNamePageDataStartLabel;
	        DateEndLabel.Text = Resource.TripNamePageDataEndLabel;
	        DateTime dateStart = Utils.DateFromString(_trip.DateBegin);
	        DateStartDatePicker.Date = dateStart;
            DateTime dateEnd = Utils.DateFromString(_trip.DateEnd);
            DateEndDatePicker.Date = dateEnd;
        }

        protected override bool OnBackButtonPressed()
        {
            CostsPage costsPage = new CostsPage(_trip);
            Navigation.PushAsync(costsPage, false);
            return true;
        }

        private void DateEndDatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
	    {
	        _trip.DateEnd = e.NewDate.ToString(App.DateFormat);
            SqlCrudUtils.Save(_trip);
	    }

	    private void DateStartDatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
	    {
	        _trip.DateBegin = e.NewDate.ToString(App.DateFormat);
	        SqlCrudUtils.Save(_trip);
        }

	    private Trips _trip;

	    private  void TripNameEntry_OnUnfocused(object sender, FocusEventArgs e)
	    {
	        Entry entry = sender as Entry;

	        _trip.Name = entry.Text;
            SqlCrudUtils.Save(_trip);
            CurrencyDictionary defaultCurrency = 
                App.Database.Table<CurrencyDictionary>().FirstOrDefault(n => n.Code == Resource.DefaultCurrencyCode);
            TripCurrencies tripCurr = SqlCrudUtils.CreateTripCurrencies(_trip.id, _trip.Sync, defaultCurrency.id); ;
            SqlCrudUtils.SetBaseCurrency(tripCurr);

            //CostsPage costsPage = new CostsPage(_trip);
            //await Navigation.PushAsync(costsPage, false);
        }

        private void TripNameEntry_Completed(object sender, EventArgs e)
        {
            Entry entry = sender as Entry;
            entry?.Unfocus();
        }
    }
}