using System;
using System.Collections.Generic;
using System.Linq;
using Apportionment2.ViewModels;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Apportionment2.Sqlite;
using Apportionment2.CustomElements;
using Apportionment2.Extensions;


namespace Apportionment2.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SelectUserDialogPage : ContentPage
    {
		public SelectUserDialogPage (string tripId)
		{
			InitializeComponent ();
            NavigationPage.SetHasBackButton(this, false);
            NavigationPage.SetHasNavigationBar(this, false);
            _tripId = tripId;
            _isPayers = true;
        }
        public SelectUserDialogPage(string tripId, bool areUsersFormOtherProjets)
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);
            NavigationPage.SetHasNavigationBar(this, false);
            _tripId = tripId;
            _isPayers = true;
            _areUsersFromOtherProjects = areUsersFormOtherProjets;
        }
        public SelectUserDialogPage(string tripId, string costId, bool isPayers)
        {
            InitializeComponent();
            _tripId = tripId;
            _isPayers = isPayers;
            _costId = costId;
            NavigationPage.SetHasBackButton(this, false);
            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override void OnAppearing()
        {
            TitleLabel.Text = _areUsersFromOtherProjects
                ? Resource.UsersFromOtherTrips 
                :_isPayers ? Resource.CostPagePayments : Resource.Participants;

            SelectButton.Text = Resource.Select;
            CancelButton.Text = Resource.Cancel;

            // Gets list of trip costs.
            string sql = _isPayers
                ? _areUsersFromOtherProjects ? usersFromOtherTripsSql() : usersForPaymentSql()
                : usersForShareSql();

            var cmd = App.Database.CreateCommand(sql);

            _users = cmd.ExecuteQuery<Users>().ToList();
            Refresh();
        }

        private string usersForPaymentSql()
        {
            return "Select distinct us.* from Users us " +
               "join TripUsers tu on us.id = tu.UserId and tu.TripId = '" + _tripId + "' ";
        }

        private string usersForShareSql()
        {
            return "Select distinct us.* from Users us " +
               " join TripUsers tu on us.id = tu.UserId and tu.TripId = '" + _tripId + "' " +
               " left join UserCostShares ucs on ucs.UserId = us.id and ucs.CostId ='" + _costId + "' " +
               " Where ucs.UserId is null"; 
        }

        private string usersFromOtherTripsSql()
        {
            return "Select distinct us.* from Users us " +
               " join TripUsers tu on us.id = tu.UserId and tu.TripId <> '" + _tripId + "' " +
               " left join TripUsers tu2 on us.id = tu2.UserId and tu2.TripId = '" + _tripId + "' "+
               " Where tu2.UserId is null";
        }

        private void Refresh()
        {
            StackLayoutScroll.Children.Clear();
            CreateUsersList();

            if (focusedEntry == null)
                return;

            // Hack to show the keyboard. 
            Device.BeginInvokeOnMainThread(async () =>
            {
                await System.Threading.Tasks.Task.Delay(250);
                focusedEntry.Focus();
                focusedEntry.CursorPosition = focusedEntry.Text.Length;
            });

        }

        private void CreateUsersList()
        {
            StackLayout usersListStacklayout = GetNewStackLayout(null);
            int colorIndex = 0;

            if (!_areUsersFromOtherProjects)
                AddNewUserLayout();
          
            colorIndex++;

            foreach (Users user in _users)
            {
                bool isUserSelected = _selectedUsers.Contains(user);
                Color backGroundColor = isUserSelected ? selectedColor : GetBackgroundColor(colorIndex);

                StackLayout userLayout = Utils.GetNewStackLayout(user, StackOrientation.Horizontal);
                userLayout.Padding = new Thickness(5, 0, 0, 0);
                userLayout.BackgroundColor = backGroundColor;

                Switch userSwitch = UserSwithch(user);
                userSwitch.IsToggled = isUserSelected;
                userSwitch.Toggled += (s, e) => userSwitchOnToggled(user, e);
                string userName = user.Name;

                userLayout.Children.Add(userSwitch);

                if (renamedUser == user)
                {
                    CustomEntry newOrEditedNameEntry = NameEntry(user, userName, backGroundColor);
                    userSwitch.IsToggled = true;
                    userLayout.Children.Add(userSwitch);
                    userLayout.Children.Add(newOrEditedNameEntry);
                    userLayout.BackgroundColor = selectedColor;
                    focusedEntry = newOrEditedNameEntry;
                }
                else
                {
                    LabelWithLongClick userNameLabel = NameLabel(user, userName, backGroundColor);
                    userLayout.Children.Add(userNameLabel);
                }
                                
                StackLayoutScroll.Children.Add(usersListStacklayout);
                usersListStacklayout.Children.Add(userLayout);

                colorIndex++;
            }

            if (!_areUsersFromOtherProjects)
                AddUserFromAnotherTripsLayout(colorIndex);
        }

        private void AddNewUserLayout()
        {
            StackLayout addNewUserLayout = GetNewStackLayout(null);
            LabelWithLongClick addNewUserLabel = GetNameLabel(null, Resource.AddNewParticipant, 140, 15, GetBackgroundColor(0));
            addNewUserLayout.Children.Add(addNewUserLabel);
            addNewUserLabel.Clicked += (s, e) => AddNewUserLabel_OnTapped(addNewUserLabel, e);
            StackLayoutScroll.Children.Add(addNewUserLayout);
        }

        private void AddUserFromAnotherTripsLayout(int colorIndex)
        {
            StackLayout addUserFromAnotherTripsLayout = GetNewStackLayout(null);
            LabelWithLongClick addUserFromAnotherTripsLabel =
                 GetNameLabel(null, Resource.ParticipiantFromAnotherTrips, 140, 15, GetBackgroundColor(colorIndex));
            addUserFromAnotherTripsLayout.Children.Add(addUserFromAnotherTripsLabel);
            addUserFromAnotherTripsLabel.Clicked += (s, e) => AddUserFromAnotherTripsLabel_OnTapped(addUserFromAnotherTripsLabel, e);
            StackLayoutScroll.Children.Add(addUserFromAnotherTripsLayout);
        }

        private async void AddUserFromAnotherTripsLabel_OnTapped(object sender, EventArgs e)
        {
            var page = new SelectUserDialogPage(_tripId, true);
            await Navigation.PushAsync(page);
        }

        private void AddNewUserLabel_OnTapped(object sender, EventArgs e)
        {
            Users newUser = SqlCrudUtils.GetNewUser(_tripId);
            _users.Add(newUser);
            _selectedUsers.Add(newUser);

            renamedUser = newUser;
            Refresh();
        }

        private void SaveUsers()
        {
            foreach (Users user in _selectedUsers)
            {
                SqlCrudUtils.Save(user);

                if (App.Database.Table<TripUsers>().Any(n => n.TripId == _tripId && n.UserId == user.id))
                    continue;

                TripUsers tripUser = SqlCrudUtils.GeTripUsers(user.id,_tripId);
                SqlCrudUtils.Save(tripUser);
            }
        }

        private Color GetBackgroundColor(int index)
        {
            return (index % 2 == 0) ? Color.FromHex("#F5F5F5") : Color.FromHex("#FFFFFF");
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
            }

            entry.Unfocus();
            renamedUser = null;
            Device.BeginInvokeOnMainThread(() => { Refresh(); });
        }

        private Switch UserSwithch(object bindingContext)
        {
            Switch userSwitch = new Switch();
            userSwitch.BindingContext = bindingContext;
            return userSwitch;
        }

        private void userSwitchOnToggled(object sender, ToggledEventArgs e)
        {
            Users user = sender as Users;

            if (user == null)
                return;

            if (e.Value)
            {
                if (!_selectedUsers.Contains(user))
                    _selectedUsers.Add(user);
            }
            else
            {
                if (_selectedUsers.Contains(user))
                    _selectedUsers.Remove(user);
            }

            Device.BeginInvokeOnMainThread(async () =>
            {
                Refresh();
            });
        }

        private CustomEntry NameEntry(object bindingContext, string userName, Color backGroundColor)
        {
            CustomEntry payerNameEntry = GetNameEntry(bindingContext, 140, 15, Color.Black);
            payerNameEntry.Text = userName;
            payerNameEntry.Unfocused += EntryName_Unfocused;
            payerNameEntry.BackgroundColor = backGroundColor;

            return payerNameEntry;
        }

        private LabelWithLongClick NameLabel(object bindingContext, string userName, Color backGroundColor)
        {
            LabelWithLongClick payerNameLabel = GetNameLabel(bindingContext, userName, 140, 15, backGroundColor);
            //var payerNameLabelTapGestureRecognizer = new TapGestureRecognizer();
            //payerNameLabel.GestureRecognizers.Add(payerNameLabelTapGestureRecognizer);
            //payerNameLabelTapGestureRecognizer.Tapped += (s, e) => nameLabel_OnTapped(payerNameLabel, e);
            payerNameLabel.Clicked += (s, e) => NameLabel_OnTapped(payerNameLabel, e);
            payerNameLabel.LongClicked += (s, e) => NameLabel_OnLongTapped(payerNameLabel, e);
            return payerNameLabel;
        }

        private void NameLabel_OnTapped(object sender, EventArgs e)
        {
            LabelWithLongClick nameLabel = sender as LabelWithLongClick;

            if (nameLabel == null)
                return;

            Users user = nameLabel.BindingContext as Users;

            if (user == null)
                return;


            if (_selectedUsers.Contains(user))
                _selectedUsers.Remove(user);
            else
               _selectedUsers.Add(user);


            Refresh();

        }

        private async void NameLabel_OnLongTapped(object sender, EventArgs e)
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

                renamedUser = nameLabel.BindingContext as Users;
                Refresh();
            }
        }

        private void DelteItem(object bindingContext)
        {
            if (bindingContext is Users user)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (await DisplayAlert(null, Resource.UsersPageUserDeleteMessage, Resource.Yes, Resource.No))
                    {
                        if ((Utils.SumByUser(user, _tripId) == 0)
                        && (!App.Database.Table<UserCostShares>().Where(n => n.TripId == _tripId && n.UserId == user.id).Any()))
                        {
                            DeleteUser(user);
                        }
                        else
                        {
                            await DisplayAlert("", Resource.UsersPageUserCanNotBeDeleted, Resource.Cancel);
                        }

                    }
                });
            }
        }

        private void DeleteUser(Users user)
        {
            _users.Remove(user);
            CostValues[] costValues = App.Database.Table<CostValues>().
            Where(n => n.TripId == _tripId && n.UserId == user.id).ToArray();

            foreach (var costValue in costValues)
                SqlCrudUtils.Delete(costValue);

            // Delete form trip.
            TripUsers tripuser = App.Database.Table<TripUsers>().
            First(n => n.TripId == _tripId && n.UserId == user.id);
            SqlCrudUtils.Delete(tripuser);

            // Delete form all trips.
            if (!App.Database.Table<UserCostShares>().Where(n => n.UserId == user.id).Any() &&
            !App.Database.Table<CostValues>().Where(n => n.UserId == user.id).Any() &&
            !App.Database.Table<TripUsers>().Where(n => n.UserId == user.id).Any())
            {
                SqlCrudUtils.Delete(user);
            }

            Refresh();
        }

        private StackLayout GetNewStackLayout(object bindingContext) =>
         Utils.GetNewStackLayout(bindingContext, StackOrientation.Vertical);

        private LabelWithLongClick GetNameLabel(object bindingContext, string name, double width, double fontSize, Color color)
        {
            LabelWithLongClick label = Utils.GetLabelWithLongClick(bindingContext, name, width, 38, fontSize);
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

        private async void SelectButton_Clicked(object sender, EventArgs e)
        {
            SaveUsers();

            foreach (Users user in _selectedUsers)
            {
                if (_isPayers)
                {
                    string defaultCurrency = App.Database.Table<CurrencyDictionary>().FirstOrDefault(n => n.Code == Resource.DefaultCurrencyCode).id;
                    CostValues newCostValue = SqlCrudUtils.GetNewUserCostValue(user.id, _costId, _tripId, defaultCurrency);
                    SqlCrudUtils.Save(newCostValue);
                }
                else
                {
                    UserCostShares newUserCostShare = SqlCrudUtils.GetNewUserCostShare(user.id, _costId, _tripId);
                    SqlCrudUtils.Save(newUserCostShare);
                }
            }

            await Navigation.PopAsync(false);
        }

        private async void CancelButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync(false);
        }

        private string _tripId;
        private List<Users> _selectedUsers = new List<Users>();
        private CustomEntry focusedEntry;
        private Users renamedUser;
        private List<Users> _users;
        private Color selectedColor = Color.LightBlue;
        private string _costId;
        private bool _isPayers = false;
        private bool _areUsersFromOtherProjects = false;
    }
}