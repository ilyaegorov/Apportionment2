using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Apportionment2.Sqlite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Apportionment2.CustomElements;
using System.Threading.Tasks;

namespace Apportionment2.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CostPage : ContentPage
    {
        public CostPage(string tripId)
        {
            _tripId = tripId;
            _isNewCostCreated = true;
            _cost = SqlCrudUtils.GetNewCost(_tripId);
            _users = Utils.GetUsers(_tripId);
            _defaultCurrency = App.Database.Table<CurrencyDictionary>().FirstOrDefault(n => n.Code == Resource.DefaultCurrencyCode);
            NavigationPage.SetHasBackButton(this, false);
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
        }

      

        public CostPage(Costs cost) 
        {
            _tripId = cost.TripId;
            _cost = cost;
            _costValues = App.Database.Table<CostValues>().Where(n => n.CostId == _cost.id).ToList();
            _userCostShares = App.Database.Table<UserCostShares>().Where(n => n.CostId == _cost.id).ToList();
            _defaultCurrency = App.Database.Table<CurrencyDictionary>().FirstOrDefault(n => n.Code == Resource.DefaultCurrencyCode);
            _users = Utils.GetUsers(_tripId);
            NavigationPage.SetHasBackButton(this, false);
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
           // Title = App.Database.Table<Trips>().FirstOrDefault(n => n.id == _tripId).Name;
            SetButtonName();
            CostName.Text = _cost.CostName;
            CostDate.Date = Utils.DateFromString(_cost.DateCreate);
            RefreshPage();

            if (_isNewCostCreated)
            {
                CostName.Focus();
                _isNewCostCreated = false;
            }

            base.OnAppearing();
        }

        private void SetButtonName()
        {
            CostName.Placeholder = Resource.CostPageCostNamePlaceholder;
            CostName.PlaceholderColor = Color.Gray;
        }

        private void RefreshPage()
        {
            StackLayoutScroll.Children.Clear();
            nameEntries.Clear();
            nameLabels.Clear();
            CreatePaymentsList();
            CreateParticipantsList();

            if (emptyNameEntry != null)
            {
                {
                    // Hack to show the keyboard. 
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await System.Threading.Tasks.Task.Delay(250);
                        emptyNameEntry.Focus();
                    });
                }
            }
        }

        private void CreatePaymentsList()
        {
            paymentsLabel = GetTitleLabel(null, Resource.CostPagePayments);
            var paymentsLabelTapGestureRecognizer = new TapGestureRecognizer();
            paymentsLabelTapGestureRecognizer.Tapped += (s, e) => paymentItemLayout_OnTapped(null, null);
            paymentsLabel.GestureRecognizers.Add(paymentsLabelTapGestureRecognizer);

            StackLayoutScroll.Children.Add(paymentsLabel);
            StackLayout paymentsListLayout = GetNewStackLayout(null);
            int colorIndex = 0;

            foreach (var costValue in _costValues)
            {
                Color backGroundColor = (colorIndex % 2 == 0) ? Color.FromHex("#F5F5F5") : Color.FromHex("#FFFFFF");
                StackLayout paymentItemLayout = Utils.GetNewStackLayout(costValue, StackOrientation.Horizontal);
                paymentItemLayout.BackgroundColor = backGroundColor;
                paymentItemLayout.Padding = new Thickness(5, 0, 0, 0);
                Users user = _users.FirstOrDefault(n => n.id == costValue.UserId);
                Button currencyButton = GetCurrencyButton(costValue);
                currencyButton.BackgroundColor = backGroundColor;

                CustomEntry sum = Utils.GetDoubleEntry(costValue, 100, 15, Color.Green);
                sum.BackgroundColor = backGroundColor;
                sum.TextChanged += EntryValue_Replaced;
                sum.Focused += EntryValue_Focused;

                string currencyCode = App.Database.Table<CurrencyDictionary>()
                    .FirstOrDefault(n => n.id == costValue.CurrencyId).Code;
                string userName = user.Name;
                currencyButton.Text = currencyCode;
                sum.Text = $"{costValue.Value:0.00}";
                sum.Unfocused += EntryCostValue_Unfocused;
              
                if (string.IsNullOrEmpty(userName) || (renamedUserObject == costValue))
                {
                    CustomEntry payerNameEntry = NameEntry(user, userName, backGroundColor);
                    renamedUserObject = null;
                    emptyNameEntry = payerNameEntry;
                    nameEntries.Add(payerNameEntry);
                    paymentItemLayout.Children.Add(payerNameEntry);
                }
                else
                {
                    Label payerNameLabel = NameLabel(costValue, userName, backGroundColor);
                    nameLabels.Add(payerNameLabel);
                    paymentItemLayout.Children.Add(payerNameLabel);
                }

                paymentItemLayout.Children.Add(currencyButton);
                paymentItemLayout.Children.Add(sum);
                paymentsListLayout.Children.Add(paymentItemLayout);
                StackLayoutScroll.Children.Add(paymentsListLayout);
                colorIndex++;
            }
        }

        private CustomEntry NameEntry(object bindingContext, string userName, Color backGroundColor)
        {
            CustomEntry payerNameEntry = GetNameEntry(bindingContext, 140, 15, Color.Black);
            payerNameEntry.Text = userName;
            payerNameEntry.Unfocused += EntryName_Unfocused;
            payerNameEntry.BackgroundColor = backGroundColor;

            return payerNameEntry;
        }

        private Label NameLabel(object bindingContext, string userName, Color backGroundColor)
        {
            Label payerNameLabel = GetNameLabel(bindingContext, userName, 140, 15, backGroundColor);
            var payerNameLabelTapGestureRecognizer = new TapGestureRecognizer();
            payerNameLabelTapGestureRecognizer.Tapped += (s, e) => nameLabel_OnTapped(payerNameLabel, null);
            payerNameLabel.GestureRecognizers.Add(payerNameLabelTapGestureRecognizer);
            return payerNameLabel;
        }

        private async void nameLabel_OnTapped(object sender, TextChangedEventArgs e)
        {
            List<string> dialogNameLabel = new List<string>();
            dialogNameLabel.Add(Resource.CostPageDeleteItem);
            dialogNameLabel.Add(Resource.CostPageRenameParticipant);

          
            var dialogAddPayment = await DisplayActionSheet(null, Resource.Cancel, null, dialogNameLabel.ToArray());

            if (dialogAddPayment == Resource.Cancel || dialogAddPayment == null)
            {
                // do nothing
            }
            else if (dialogAddPayment == Resource.CostPageDeleteItem)
            {
                Label nameLabel = sender as Label;

                if (nameLabel == null)
                    return;

                DelteItem(nameLabel.BindingContext);
            }
            else if (dialogAddPayment == Resource.CostPageRenameParticipant)
            {
                Label nameLabel = sender as Label;

                if (nameLabel == null)
                    return;

                renamedUserObject = nameLabel.BindingContext;
                RefreshPage();
            }
        }

        private void paymentItemLayout_OnTapped(object sender, TextChangedEventArgs e)
        {
            AddPayment();
        }

        private void CreateParticipantsList()
        {
            Label participantsTitle = GetTitleLabel(null, Resource.Participants);
            var participantsTitleTapGestureRecognizer = new TapGestureRecognizer();
            participantsTitleTapGestureRecognizer.Tapped += (s, e) => UserShareLayout_OnTapped(null, null);
            participantsTitle.GestureRecognizers.Add(participantsTitleTapGestureRecognizer);
            StackLayoutScroll.Children.Add(participantsTitle);
            StackLayout paymentsListLayout = GetNewStackLayout(null);

            int colorIndex = 0;

            foreach (UserCostShares userShare in _userCostShares)
            {
                Color backGroundColor = (colorIndex % 2 == 0) ? Color.FromHex("#F5F5F5") : Color.FromHex("#FFFFFF");
                StackLayout userShareLayout = Utils.GetNewStackLayout(userShare, StackOrientation.Horizontal);
                userShareLayout.Padding = new Thickness(5, 0, 0, 0);
                userShareLayout.BackgroundColor = backGroundColor;
                Users user = _users.FirstOrDefault(n => n.id == userShare.UserId);

                CustomEntry share = Utils.GetDoubleEntry(userShare, 100, 15, Color.Green);
                share.BackgroundColor = backGroundColor;
                share.TextChanged += EntryValue_Replaced;
                share.Focused += EntryValue_Focused;
                string userName = user.Name;
                
                if (string.IsNullOrEmpty(userName) || (renamedUserObject == userShare))
                {
                    CustomEntry payerNameEntry = NameEntry(user, userName, backGroundColor);
                    renamedUserObject = null;
                    emptyNameEntry = payerNameEntry;
                    nameEntries.Add(payerNameEntry);
                    userShareLayout.Children.Add(payerNameEntry);
                }
                else
                {
                    Label payerNameLabel = NameLabel(userShare, userName, backGroundColor);
                    nameLabels.Add(payerNameLabel);
                    userShareLayout.Children.Add(payerNameLabel);
                }

                share.Text = $"{userShare.Share:0.00}";
                share.Unfocused += EntryUserShare_Unfocused;

                StackLayoutScroll.Children.Add(paymentsListLayout);
                paymentsListLayout.Children.Add(userShareLayout);
                userShareLayout.Children.Add(share);

                StackLayoutScroll.Children.Add(paymentsListLayout);
                colorIndex++;
            }
        }

        private void UserShareLayout_OnTapped(object sender, TextChangedEventArgs e)
        {
            AddUserShare();
        }

        private Button GetCurrencyButton(object bindingContext)
        {
            Button currencyButton = GetButton(bindingContext);
            currencyButton.WidthRequest = 55;
            currencyButton.TextColor = Color.Black;
            currencyButton.Clicked += SelectCurrencyButton_OnClicked;
            currencyButton.FontSize = 10;
            currencyButton.Margin = 0;

            return currencyButton;
        }

        private Button GetRemoveButton(object bindingContext)
        {
            Button bRemove = GetButton(bindingContext);
            bRemove.WidthRequest = 35;
            bRemove.TextColor = Color.Red;
            bRemove.Clicked += RemoveButton_OnClicked;
            bRemove.ImageSource = "Delete.png";
            return bRemove;
        }

        private Button GetButton(object bindingContext)
        {
            Button button = new Button
            {
                IsVisible = true,
                BindingContext = bindingContext,
                Style = (Style) this.Resources["ButtonEmpty"],
                HorizontalOptions = LayoutOptions.Start,
                HeightRequest = 35
            };

            return button;
        }
        
        private StackLayout GetNewStackLayout(object bindingContext) =>
            Utils.GetNewStackLayout(bindingContext, StackOrientation.Vertical);

        private Label GetTitleLabel(object bindingContext, string text)
        {
            Label label = Utils.GetLabel(bindingContext, text, 100, 45, 15);
            label.BackgroundColor = Color.LightGray;
            return label;
        }

        private BoxView GetLine(object bindingContext)
        {
            BoxView line = new BoxView
            {
                BindingContext = bindingContext,
                WidthRequest = 100,
                HeightRequest = 1,
                //Color = Color.FromHex("#708090")
                Color = Color.Red
            };

            return line;
        }

        private Label GetNameLabel(object bindingContext, string name, double width, double fontSize, Color color)
        {
            Label label = Utils.GetLabel(bindingContext, name, width, 38, fontSize);
            label.BackgroundColor = color;
            label.HorizontalTextAlignment = TextAlignment.Start;
            label.VerticalTextAlignment = TextAlignment.Center;
            label.HorizontalOptions = LayoutOptions.FillAndExpand;
            return label;
        }

        private CustomEntry GetNameEntry(object bindingContext, double width, double fontSize, Color color)
        {
            CustomEntry entry = Utils.GetEntry(bindingContext, width, fontSize, color);
            entry.HorizontalTextAlignment = TextAlignment.Start;
            entry.VerticalTextAlignment = TextAlignment.Center;
            entry.HorizontalOptions = LayoutOptions.FillAndExpand;
            return entry;
        }

        private void MenuItem_OnClicked(object sender, EventArgs e)
        {
            //do nothing;
        }

        private async void AddButton_OnClicked(object sender, EventArgs e)
        {
            var actionAdd = await DisplayActionSheet(null, Resource.Cancel, null, Resource.CostPageAddPayment,
                Resource.CostPageAddParticipant);

            if (actionAdd == Resource.Cancel || actionAdd == null)
            {
                // do nothing
            }
            else if (actionAdd == Resource.CostPageAddPayment)
            {
                AddPayment();
            }
            else if (actionAdd == Resource.CostPageAddParticipant)
            {
                AddUserShare();
            }
        }

        private async void AddUserShare()
        {
            List<Users> availableUsers = _users.Where(n => _userCostShares.All(p => p.UserId != n.id)).ToList();
            List<string> dialogAddParticipantValues = new List<string>();
            dialogAddParticipantValues.Add(Resource.AddNewParticipant);

            for (var i = 0; i < availableUsers.Count(); i++)
                dialogAddParticipantValues.Add($"{i + 1} {availableUsers[i].Name}");

            dialogAddParticipantValues.Add(Resource.ParticipiantFromAnotherTrips);

            var dialogAddParticipant = await DisplayActionSheet(null, Resource.Cancel, null, dialogAddParticipantValues.ToArray());

            if (dialogAddParticipant == Resource.Cancel || dialogAddParticipant == null)
            {
                // do nothing
            }
            else if (dialogAddParticipant == Resource.AddNewParticipant)
            {
                Users newUser = GetNewUser();
                _users.Add(newUser);
                AddNewUserCostShare(newUser.id);
                RefreshPage();
            }
            else if (dialogAddParticipant == Resource.AddNewParticipant)
            {

            }
            else if (dialogAddParticipant == Resource.ParticipiantFromAnotherTrips)
            {
                AddUserFromOtherTrips(false);
            }
            else
            {
                int index = GetIndexFromString(dialogAddParticipant);

                if (index <= 0)
                    return;

                AddNewUserCostShare(availableUsers[index - 1].id);
                RefreshPage();
            }
        }

        private async void AddUserFromOtherTrips(bool isPayment)
        {
            List<Users> availableUsers = Utils.GetOtherUsers(_tripId);
            List<string> dialogAddParticipantValues = new List<string>();

            for (var i = 0; i < availableUsers.Count(); i++)
                dialogAddParticipantValues.Add($"{i + 1} {availableUsers[i].Name}");

            var dialogAddParticipant = await DisplayActionSheet(null, Resource.Cancel, null, dialogAddParticipantValues.ToArray());

            if (dialogAddParticipant == Resource.Cancel || dialogAddParticipant == null)
            {
                // do nothing
            }
            else
            {
                int index = GetIndexFromString(dialogAddParticipant);

                if (index <= 0)
                    return;

                _users.Add(availableUsers[index - 1]);

                if ( isPayment)
                   AddNewUserCostValue(availableUsers[index - 1].id);
                else
                   AddNewUserCostShare(availableUsers[index - 1].id);

                RefreshPage();
            }
        }

        private async void AddPayment()
        {
            List<string> dialogAddPaymentValues = new List<string>();
            dialogAddPaymentValues.Add(Resource.AddNewPayer);

            for (var i = 0; i < _users.Count(); i++)
                dialogAddPaymentValues.Add($"{i + 1} {_users[i].Name}");

            dialogAddPaymentValues.Add(Resource.ParticipiantFromAnotherTrips);

            var dialogAddPayment = await DisplayActionSheet(null, Resource.Cancel, null, dialogAddPaymentValues.ToArray());

            if (dialogAddPayment == Resource.Cancel || dialogAddPayment == null)
            {
                // do nothing
            }
            else if (dialogAddPayment == Resource.AddNewPayer)
            {
                Users newUser = GetNewUser();
                _users.Add(newUser);
                AddNewUserCostValue(newUser.id);
                RefreshPage();
            }
            else if (dialogAddPayment == Resource.ParticipiantFromAnotherTrips)
            {
                AddUserFromOtherTrips(true);
            }
            else
            {
                int index = GetIndexFromString(dialogAddPayment);

                if (index <= 0)
                    return;

                AddNewUserCostValue(_users[index - 1].id);
                RefreshPage();
            }
        }

        private Users GetNewUser()
        {
            Users user = new Users
            {
                id = Guid.NewGuid().ToString(),
                Name = "",
                Sync = _cost.Sync
            };

            return user;
        }

        private void AddNewUserCostShare(string userId)
        {
            UserCostShares newUserCostShare = SqlCrudUtils.GetNewUserCostShare(userId, _cost.id, _tripId);
            _userCostShares.Add(newUserCostShare);
            _hasUnsavedData = true;
        }

        private void AddNewUserCostValue(string userId)
        {
            CostValues newCostValue = SqlCrudUtils.GetNewUserCostValue(userId, _cost.id, _tripId, _defaultCurrency.id);
            _costValues.Add(newCostValue);
            _hasUnsavedData = true;
        }

        private int GetIndexFromString(string stringWithUser)
        {
            int indexOfFirstSpace = stringWithUser.IndexOf(spaceChar, StringComparison.Ordinal);
            var ind = stringWithUser.Substring(0, indexOfFirstSpace);
            int.TryParse(ind, out int index);
            return index;
        }

        private async void SaveButton_OnClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_cost.CostName))
            {
                await DisplayAlert(Resource.CostPageCostNameEmptyAlert
                    , Resource.CostPageCostNameEmptyAlertMessage
                    , Resource.Ok);
            }
            else
            {
                SaveCost();
                SaveCostValues();
                SaveUserCostShares();
                SaveUsers();
                // Exception is thrown if PopModalAsync is used.
                await Navigation.PopAsync(true);
            }
        }

        private void SaveCost()
        {
            SqlCrudUtils.Save(_cost);
        }

        private void SaveCostValues()
        {
            foreach (CostValues costValue in _costValues)
                SqlCrudUtils.Save(costValue);
        }

        private void SaveUserCostShares()
        {
            foreach (UserCostShares userCostShare in _userCostShares)
                SqlCrudUtils.Save(userCostShare);
        }

        private void SaveUsers()
        {
            foreach (Users user in _users)
                SqlCrudUtils.Save(user);
        }

        void OnDateSelected(object sender, DateChangedEventArgs args)
        {
            if (_cost.DateCreate == args.NewDate.ToString(App.DateFormat))
                return;
            
            _cost.DateCreate = args.NewDate.ToString(App.DateFormat);
            _hasUnsavedData = true;
        }

        protected  override bool OnBackButtonPressed()
        {
            if (_hasUnsavedData)
            {
                // Begin an asyncronous task on the UI thread because we intend to ask the users permission.
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (await DisplayAlert("Exit page?", "Are you sure you want to exit this page? You will not be able to continue it.", "Yes", "No"))
                    {
                        await Navigation.PopAsync(false);
                    }
                });

                //Do not navigate backwards by pressing the button
                return true;
            }
            else
            {
                //base.OnBackButtonPressed();
                Device.BeginInvokeOnMainThread(async () => { await Navigation.PopAsync(false); });
                 //   Navigation.PopModalAsync(false);
                //Do not navigate backwards by pressing the button
                return true;
            }
        }

        private async void CancelButton_OnClicked(object sender, EventArgs e)
        {
            if (_hasUnsavedData)
            {
                var result = await DisplayAlert(Resource.CostPageUnsavedDataAlertTitle, Resource.CostPageUnsavedDataAlertMessage,
                    Resource.Yes, Resource.No);

                if (result)
                    SaveButton_OnClicked(null, null);
                else
                    // Exception is thrown if PopModalAsync is used.
                    await Navigation.PopAsync();
            }
            else
            {
                // Exception is thrown if PopModalAsync is used.
                await Navigation.PopAsync();
            }
        }

        private void CostNameEntry_OnCompleted(object sender, EventArgs e)
        {
            Entry entry = sender as Entry;
            entry?.Unfocus();
        }

        private void CostNameEntry_OnUnfocused(object sender, FocusEventArgs e)
        {
            Entry entry = sender as Entry;

            if (entry.Text != _cost.CostName)
            {
                _cost.CostName = entry.Text;
                _hasUnsavedData = true;
            }
        }

        private void EntryName_Unfocused(object sender, FocusEventArgs e)
        {
            Entry entry = sender as Entry;

            Users user = entry?.BindingContext as Users;

            if (user == null)
                return;

            if (entry.Text != user.Name)
            {
                user.Name = entry.Text;
                _hasUnsavedData = true;
            }

            entry.Unfocus();

            foreach (Entry nameEntry in nameEntries)
            {
                if (nameEntry.BindingContext == user)
                    nameEntry.Text = user.Name;
            }

            Device.BeginInvokeOnMainThread(async () =>
            {
                    RefreshPage();
            });
            // RefreshPage();
        }

        private void EntryValue_Replaced(object sender, TextChangedEventArgs e)
        {
            Entry entry = sender as Entry;
            entry.Text = entry.Text.Replace(",", ".");
        }

        private void EntryValue_Focused(object sender, FocusEventArgs e)
        {
            Entry entry = sender as Entry;

            if (entry.Text == "0.00")
                entry.Text = "";
        }

        public void EntryCostValue_Unfocused(object sender, FocusEventArgs e)
        { 
            Entry entry = sender as Entry;

            if (entry == null)
                return;

            CostValues costValues = entry.BindingContext as CostValues;
            double inputSum = 0;

            if (costValues == null)
                return;
            else if (double.TryParse(entry.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out inputSum))
                costValues.Value = inputSum;
            else
                entry.Text = "0.00";

            _hasUnsavedData = true;
        }

        public void EntryUserShare_Unfocused(object sender, FocusEventArgs e)
        {
            Entry entry = sender as Entry;

            if (entry == null)
                return;

            UserCostShares userCostShare = entry.BindingContext as UserCostShares;
            double share = 0;

            if (userCostShare == null)
                return;
            else if (double.TryParse(entry.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out share))
                userCostShare.Share = share;
            else
                entry.Text = "0.00";

            _hasUnsavedData = true;
        }

        private void DelteItem(object bindingContext)
        {
            if (bindingContext is CostValues cv)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (await DisplayAlert(null, Resource.CostsPageDeleteCostValue, Resource.Yes, Resource.No))
                    {
                        _costValues.Remove(cv);
                        SqlCrudUtils.Delete(cv);
                        RefreshPage();
                    }
                });
            }
            else if (bindingContext is UserCostShares ucs)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (await DisplayAlert(null, Resource.CostsPageDeleteUserShareMessage, Resource.Yes, Resource.No))
                    {
                        _userCostShares.Remove(ucs);
                        SqlCrudUtils.Delete(ucs);
                        RefreshPage();
                    }
                });
            }
            else
            {
                return;
            }
        }

        private async void RemoveButton_OnClicked(object sender, EventArgs e)
        {
            Button bRemove = sender as Button;

            if (bRemove == null)
                return;

            DelteItem(bRemove.BindingContext);

           // RefreshPage();
        }

        private async void SelectCurrencyButton_OnClicked(object sender, EventArgs e)
        {
            Button currencyButton = sender as Button;
            CostValues cv = currencyButton.BindingContext as CostValues;
            List<string> currenciesListStrings = new List<string>();
            List<CurrencyDictionary> currenciesList = Utils.GetCurrencies(_tripId);
            currenciesListStrings.Add(Resource.CostPageAddNewCurrencies);

            foreach (CurrencyDictionary currency in currenciesList)
                currenciesListStrings.Add($"{currency.id} {currency.Code} {currency.Name}");
            
            var currencyString = await DisplayActionSheet(null, Resource.Cancel, null, currenciesListStrings.ToArray());

            if (currencyString == Resource.Cancel || currencyString == null)
            {
                // do nothing
            }
            else if (currencyString == Resource.CostPageAddNewCurrencies)
            {
                CurrenciesPage currenciesPage = new CurrenciesPage(cv);
                await Navigation.PushModalAsync(currenciesPage);
            }
            else
            {
                int indexOfFirstSpace = currencyString.IndexOf(spaceChar, StringComparison.Ordinal);
                string id = currencyString.Substring(0, indexOfFirstSpace);
                CurrencyDictionary currency = currenciesList.FirstOrDefault(n=>n.id == id);

                if (cv.CurrencyId != currency.id)
                {
                    currencyButton.Text = currency.Code;
                    cv.CurrencyId = currency.id;
                    _hasUnsavedData = true;
                }
            }
        }

        bool _isNewCostCreated = false;
        private Label paymentsLabel;
        private Entry emptyNameEntry;
        private CurrencyDictionary _defaultCurrency;
        private List<CostValues> _costValues = new List<CostValues>();
        private List<UserCostShares> _userCostShares = new List<UserCostShares>();
        private List<Users> _users = new List<Users>();
        private List<CustomEntry> nameEntries = new List<CustomEntry>();
        private List<Label> nameLabels = new List<Label>();
        private string spaceChar = ((char)32).ToString();
        private string _tripId;
        private Costs _cost;
        private bool _hasUnsavedData = false;
        private object renamedUserObject;
    }
}