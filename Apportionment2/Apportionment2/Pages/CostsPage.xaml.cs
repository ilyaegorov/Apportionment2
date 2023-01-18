using System;
using System.Collections.ObjectModel;
using Apportionment2.Sqlite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Linq;
using Apportionment2.Interfaces;
using Apportionment2.ViewModels;
using System.Collections.Generic;

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
	        //base.OnAppearing();

	        if (string.IsNullOrEmpty(_trip.Name))
	        {
	            TripNamePage page = new TripNamePage(_trip);
                Navigation.PushAsync(page);
            }

	        Title = _trip.Name;
           
            RefreshPage();
            base.OnAppearing();
        }

        private void RefreshPage()
        {
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
	            CostPage pip = new CostPage(itemViewModel.Cost);
	            await Navigation.PushAsync(pip, false);
	        }
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
                , Resource.CostsPageDeleteUser
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
                AddButton_OnClicked(null, null);
            }
            else if (action == Resource.CostsPageDeleteUser)
            {
                UsersPage page = new UsersPage(_trip.id);
                await Navigation.PushAsync(page);
            }
            else if (action == Resource.CalcApportionPageCalc)
            {
                ButtonCalculate_OnClicked(sender, e);
            }
            else if (action == Resource.CostsPageCurrencies)
            {
                TripCurrencies baseCurrency = Utils.GetBaseTripCurrencies(_trip.id);

                if (baseCurrency == null)
                {
                    CurrencyDictionary defaultCurrency = App.Database.Table<CurrencyDictionary>().FirstOrDefault(n => n.Code == Resource.DefaultCurrencyCode);
                    TripCurrencies tripCurr = SqlCrudUtils.CreateTripCurrencies(_trip.id, _trip.Sync, defaultCurrency.id); ;
                    SqlCrudUtils.SetBaseCurrency(tripCurr);
                }

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
                SqlCrudUtils.Save(_trip);
                await DisplayAlert(null, Resource.CalcApportionPageEndJourneyMessage, Resource.Ok);
            }
            else if (action == Resource.CalcApportionPageContinueJourney)
            {
                _trip.DateEnd = App.DataEnd;
                SqlCrudUtils.Save(_trip);
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
	            TripCurrenciesPage currenciesPage = new TripCurrenciesPage(_trip, true);
	            await Navigation.PushAsync(currenciesPage , true);
	            return;
	        }

	        if (Utils.NotAllRatesAreSet(_trip.id))
	        {
	            await DisplayAlert(null, Resource.CostsPageAddAllRatesMessage, Resource.Ok);
	            TripCurrenciesPage currenciesPage = new TripCurrenciesPage(_trip, true);
	            await Navigation.PushAsync(currenciesPage, true);
	            return;
            }

	        TripResultPage page = new TripResultPage();
	        page.BindingContext =_trip;
	        await Navigation.PushAsync(page);
        }

        private async void AddButton_OnClicked(object sender, EventArgs e)
	    {
            var userCostShares = App.Database.Table<UserCostShares>().Where(n => n.TripId == _trip.id).ToList();

            if (userCostShares.Count < 1)
            {
                OpenCostPage();
            }
            else
            {
                var result = await DisplayAlert("", Resource.CopyApportionmentScheme,
                      Resource.Yes, Resource.No);

                if (result)
                {
                    CostPage costPage = new CostPage(_trip.id, userCostShares.Last().CostId);
                    await Navigation.PushAsync(costPage);
                }
                else
                {
                    OpenCostPage();
                }
            }
        }

        private async void OpenCostPage()
        {
            CostPage costPage = new CostPage(_trip.id);
            await Navigation.PushAsync(costPage);
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