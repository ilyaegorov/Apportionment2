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
	        base.OnAppearing();
	        Refresh();
	    }

	    private void Refresh()
	    {
	        if (string.IsNullOrEmpty(_trip.Name))
	            TripNameEntry.Placeholder = Resource.TripNamePageTripNamePlaceholder;
	        else
	            TripNameEntry.Text = _trip.Name;

	        DateStartLabel.Text = Resource.TripNamePageDataStartLabel;
	        DateEndLabel.Text = Resource.TripNamePageDataStartLabel;
	        DateTime dateStart = Utils.DateFromString(_trip.DateBegin);
	        DateStartDatePicker.Date = dateStart;
            DateTime dateEnd = Utils.DateFromString(_trip.DateEnd);
            DateEndDatePicker.Date = dateEnd;
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

	    private void TripNameEntry_OnUnfocused(object sender, FocusEventArgs e)
	    {
	        Entry entry = sender as Entry;

	        _trip.Name = entry.Text;
	        SqlCrudUtils.Save(_trip);
        }
	}
}