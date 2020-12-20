using System;
using System.Collections.ObjectModel;
using Apportionment2.Sqlite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Linq;
using Apportionment2.Interfaces;
using Apportionment2.ViewModels;

namespace Apportionment2.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CostsPage : ContentPage
	{
		public CostsPage ()
		{
			InitializeComponent ();
		}

	    public CostsPage(Trips trip)
	    {
	        InitializeComponent();
	        //TripNameEntry.Placeholder = Resource.CostsPageATripNameEntryPlaceholder;
            _trip = trip;
	    }

	    protected override void OnAppearing()
	    {
	        base.OnAppearing();

	        if (string.IsNullOrEmpty(_trip.Name))
	        {

	            TripNamePage page = new TripNamePage(_trip);
	            Navigation.PushAsync(page);
            }

	        Title = _trip.Name;

            RefreshPage();
        }

        private void RefreshPage()
        {
          //  TripNameEntry.Text = _trip.Name;

            // Gets list of trip costs.
            var costs = App.Database.Table<Costs>().Where(n => n.TripId == _trip.id).ToList().OrderBy(n => n.DateCreate);
            var costsCollection =new ObservableCollection<CostView>();
            int color = 0;
          
            foreach (var cost in costs)
            {
                bool isOdd = (color % 2 == 0);
                costsCollection.Add(new CostView(cost, isOdd));
                color++;
            }

            CostsList.ItemsSource = costsCollection;
            CostsList.SeparatorVisibility = SeparatorVisibility.Default;
            CostsList.SeparatorColor = Color.White;

            
           //TripItogLabel.Text = string.Format(CultureInfo.InvariantCulture, "{1}: {0:0.00}", sumPot, Resource.PotPageItog);
        }

	    private void TripNameEntry_OnCompleted(object sender, EventArgs e)
	    {
	        //do nothing;
        }

	    private void TripNameEntry_OnUnfocused(object sender, FocusEventArgs e)
	    {
	        //do nothing;
	    }


        private async void CostListItem_OnItemTapped(object sender, ItemTappedEventArgs e)
	    {
	        CostView itemViewModel = e.Item as CostView;

            if (itemViewModel != null)
	        {
                //var pip = new PotItemPage(itemViewModel.PotItems);
                //await Navigation.PushAsync(pip);


	            CostPage pip = new CostPage(itemViewModel.Cost);
	            await Navigation.PushModalAsync(pip, false);
	        }
	        //else if (itemViewModel.AddNewItem)
	        //{
	        //    ButtonAddPot_OnClicked(null, null);
	        //}

        }

        /// <summary>
        /// Shows "Menu"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuItem_OnClicked(object sender, EventArgs e)
        {
            string statusJourney = _trip.DateEnd == App.DataEnd
                ? Resource.CalcApportionPageEndJourney 
                : Resource.CalcApportionPageContinueJourney;

            string syncOrCopyId = _trip.Sync == "True" 
                ? Resource.CalcApportionPageGetAppId 
                : Resource.CalcApportionPageSync;

            var action = await DisplayActionSheet(
                Resource.CalcApportionPageMenuTitle
                , Resource.Cancel
                , null
                , Resource.CalcApportionPageAdd
                , Resource.CostsPageCurrencies
                , Resource.CostsPageRenameTrip
                //, Resource.CalcApportionPageAddPot
                //, Resource.CalcApportionPageEditPot
                //, Resource.CalcApportionPageAddUser
                //, Resource.CalcApportionPageDebt
                , Resource.CalcApportionPageCalc
                //, syncOrCopyId
                , Resource.CalcApportionPageExitFromProj
                , statusJourney
                , Resource.CalcApportionPageExit);

            if (action == Resource.CalcApportionPageAdd)
            {
                //ButtonAdd_OnClicked(sender, e);
            }
            else if (action == Resource.CalcApportionPageCalc)
            {
                ButtonCalculate_OnClicked(sender, e);
            }
            else if (action == Resource.CostsPageCurrencies)
            {
                TripCurrenciesPage page = new TripCurrenciesPage(_trip);

                await Navigation.PushAsync(page);
            }
            else if (action == Resource.CostsPageRenameTrip)
            {
                TripNamePage page = new TripNamePage(_trip);
                await Navigation.PushAsync(page);
            }

            else if (action == Resource.CalcApportionPageSync)
            {
               await DisplayAlert(null, Resource.CalcApportionPageJourneyIsSyncMessage + ". " + _trip.id, Resource.Ok);

            }
            else if (action == Resource.CalcApportionPageEndJourney)
            {
                _trip.DateEnd = DateTime.Now.ToString(App.DateFormat);
                await DisplayAlert(null, Resource.CalcApportionPageEndJourneyMessage, Resource.Ok);
            }
            else if (action == Resource.CalcApportionPageContinueJourney)
            {
                _trip.DateEnd = App.DataEnd;
                await DisplayAlert(null, Resource.CalcApportionPageContinueJourneyMessage, Resource.Ok);
            }
            else if (action == Resource.CalcApportionPageExitFromProj)
            {
                await Navigation.PushAsync(new StartPage());
            }
            else if (action == Resource.CalcApportionPageExit)
            {
                var closer = DependencyService.Get<ICloseApplication>();

                closer?.CloseApplication();
            }
        }
        
        private async void ButtonCalculate_OnClicked(object sender, EventArgs e)
	    {
	        if (Utils.GetBaseTripCurrencies(_trip.id) == null)
	        {
	           // await DisplayAlert(null, Resource.TripCurrenciesPageSetBaseCurrencyMessage, Resource.Ok);
	            TripCurrenciesPage currenciesPage = new TripCurrenciesPage(_trip);
	            await Navigation.PushAsync(currenciesPage);
	            return;
	        }

	        if (Utils.AreAllRatesSet(_trip.id))
	        {
	            await DisplayAlert(null, Resource.CostsPageAddAllRatesMessage, Resource.Ok);
	            TripCurrenciesPage currenciesPage = new TripCurrenciesPage(_trip);
	            await Navigation.PushAsync(currenciesPage);
	            return;
            }

	        TripResultPage page = new TripResultPage();
	        page.BindingContext =_trip;
	        await Navigation.PushAsync(page);
        }

        private async void AddButton_OnClicked(object sender, EventArgs e)
	    {
	        CostPage pip = new CostPage(_trip.id);
	        await Navigation.PushAsync(pip);
        }

        private async void CostListItem_OnLongClicked(object sender, SelectedItemChangedEventArgs e)
        {
            CostView costView = e.SelectedItem as CostView;

            if (costView == null)
                return;

            var action = await DisplayActionSheet(
	            null
	            , Resource.Cancel
	            , null
	            , Resource.CostsPageDeleteCost
                , Resource.CostsPageCopyCost);


	        if (action == Resource.CostsPageDeleteCost)
	        {
	            DeleteCost(costView.Cost);
	        }
	        else if (action == Resource.CostsPageCopyCost)
	        {
	            CopyCost(costView.Cost);
            }
        }

	    private async void DeleteCost(Costs cost)
	    {
	        if (await DisplayAlert(null, Resource.CostsPageDeleteCostMessage, Resource.Yes, Resource.No))
	        {
                SqlCrudUtils.DeleteCost(cost);
                RefreshPage();
            }
	    }

	    private void CopyCost(Costs cost)
	    {
	        Costs copiedCost = SqlCrudUtils.GetNewCost(_trip.id);
	        copiedCost.CostName = $"Copy-{cost.CostName}";
            SqlCrudUtils.Save(copiedCost);

            var costShares = App.Database.Table<UserCostShares>().Where(n => n.CostId == cost.id);

	        foreach (var costShare in costShares)
	        {
	            UserCostShares copiedUserShare = SqlCrudUtils.GetNewUserCostShare(costShare.UserId, copiedCost.id, _trip.id);
	            copiedUserShare.Share = costShare.Share;
                SqlCrudUtils.Save(copiedUserShare);
            }

	        var costValues = App.Database.Table<CostValues>().Where(n => n.CostId == cost.id);

	        foreach (var costValue in costValues)
	        {
	            CostValues copiedCostValue =
	                SqlCrudUtils.GetNewUserCostValue(costValue.UserId, copiedCost.id, _trip.id, costValue.CurrencyId);

	            copiedCostValue.Value = costValue.Value;
                SqlCrudUtils.Save(copiedCostValue);
            }

            RefreshPage();
	    }

        private Trips _trip;
	}
}