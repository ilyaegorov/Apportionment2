using System;
using System.Collections.ObjectModel;
using Apportionment2.Sqlite;
using Apportionment2.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Apportionment2.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TripsListPage : ContentPage
	{
		public TripsListPage ()
		{
			InitializeComponent();
		}

	    protected override void OnAppearing()
	    {
	        base.OnAppearing();
	     
	        RefreshPage();
	    }

	    private void RefreshPage()
	    {
	        var trips = App.Database.Table<Trips>();
	        var tripsCollection = new ObservableCollection<TripView>();
	        int color = 0;

	        foreach (var trip in trips)
	        {
	            bool isOdd = (color % 2 == 0);
	            tripsCollection.Add(new TripView(trip, isOdd));
	            color++;
	        }

	        TripsList.ItemsSource = tripsCollection;
	        TripsList.SeparatorVisibility = SeparatorVisibility.Default;
	        TripsList.SeparatorColor = Color.White;
        }


	    private async void TripsList_OnItemTapped(object sender, ItemTappedEventArgs e)
	    {
	        TripView itemViewModel = e.Item as TripView;

	        if (itemViewModel == null)
	            return;

            CostsPage costsPage = new CostsPage(itemViewModel.Trip);
	        await Navigation.PushAsync(costsPage);
        }

	    private async void TripsList_OnLongClicked(object sender, SelectedItemChangedEventArgs e)
	    {
	        TripView itemViewModel = e.SelectedItem as TripView;

	        if (itemViewModel == null)
	            return;

            var action = await DisplayActionSheet(
	            null
	            , Resource.Cancel
	            , null
	            , Resource.TripListPageDeleteTrip
                , Resource.TripListPageRenameTrip);

	        if (action == Resource.TripListPageDeleteTrip)
	        {
	            if (await DisplayAlert(null, Resource.TripListPageDeleteTripMessage, Resource.Yes, Resource.No))
	            {
	                var tripCurrencies = App.Database.Table<TripCurrencies>().Where(n => n.TripId == itemViewModel.Trip.id);

	                foreach (var tripCurrency in tripCurrencies)
	                    SqlCrudUtils.Delete(tripCurrency);

                    var costs = App.Database.Table<Costs>().Where(n => n.TripId == itemViewModel.Trip.id);

	                foreach (var cost in costs)
	                    SqlCrudUtils.DeleteCost(cost);


                    SqlCrudUtils.Delete(itemViewModel.Trip);
                    RefreshPage();
                }
            }
	        else if (action == Resource.TripListPageRenameTrip)
	        {
	            TripNamePage page = new TripNamePage(itemViewModel.Trip);
	            await Navigation.PushAsync(page);
            }
        }

	    private void ImageButton_OnClicked(object sender, EventArgs e)
	    {
	        throw new NotImplementedException();
	    }
	}
}