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
            _isNewTripCreated = true;
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

            // Does not work. But works in CostPage.
            if (_isNewTripCreated)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await System.Threading.Tasks.Task.Delay(250);
                    TripNameEntry.Focus();
                });

                _isNewTripCreated = false;
            }

            base.OnAppearing();
        }

	    private void Refresh()
	    {
            TripNameEntry.Placeholder = Resource.TripNamePageTripNamePlaceholder;
            TripNameEntry.PlaceholderColor = Color.Gray;
            TripNameEntry.Text = _trip.Name;

            DateStartLabel.Text = Resource.TripNamePageDataStartLabel;
	        DateEndLabel.Text = Resource.TripNamePageDataEndLabel;
	        DateTime dateStart = Utils.DateFromString(_trip.DateBegin);
	        DateStartDatePicker.Date = dateStart;
            DateTime dateEnd = Utils.DateFromString(_trip.DateEnd);
            DateEndDatePicker.Date = dateEnd;
        }

        protected override bool OnBackButtonPressed()
        {
            Navigation.PopAsync();

            CostsPage costsPage = new CostsPage(_trip);
            Navigation.PushAsync(costsPage, false);

            List<Users> users = Utils.GetUsers(_trip.id);
            
            // Create users for the first time.
            if (users.Count < 1)
            {
                SelectUserDialogPage page = new SelectUserDialogPage(_trip.id);
                Navigation.PushAsync(page, false);
            }
         
            return true;
        }
        private void OnBackButtonPressed(object sender, EventArgs e)
        {
            OnBackButtonPressed();
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

        private Trips _trip;
        private bool _isNewTripCreated = false;
    }
}