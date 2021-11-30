using Apportionment2.CustomElements;
using Apportionment2.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Apportionment2.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class UsersPage : ContentPage
	{
		public UsersPage (string trip_Id)
		{
            _tripId = trip_Id;
            _users = Utils.GetTripUsers(_tripId);
            InitializeComponent ();
		}

        protected override void OnAppearing()
        {
            Title = App.Database.Table<Trips>().FirstOrDefault(n => n.id == _tripId).Name;
            base.OnAppearing();
            RefreshPage();
        }

        private void RefreshPage()
        {
            StackLayoutScroll.Children.Clear();
            nameEntries.Clear();
            CreateUsersList();

            if (EmptyNameEntry != null)
                EmptyNameEntry.Focus();
        }

        private void CreateUsersList()
        {
            UsersLabel = GetTitleLabel(null, Resource.UsersPageTitle);
            //var userLabelTapGestureRecognizer = new TapGestureRecognizer();
            //userLabelTapGestureRecognizer.Tapped += (s, e) => userItemLayout_OnTapped(null, null);
            //UsersLabel.GestureRecognizers.Add(userLabelTapGestureRecognizer);

            StackLayoutScroll.Children.Add(UsersLabel);
            StackLayout paymentsListLayout = GetNewStackLayout(null);
            int colorIndex = 0;

            foreach (var user in _users)
            {
                Color backGroundColor = (colorIndex % 2 == 0) ? Color.FromHex("#F5F5F5") : Color.FromHex("#FFFFFF");
                StackLayout paymentItemLayout = Utils.GetNewStackLayout(user, StackOrientation.Horizontal);
                paymentItemLayout.BackgroundColor = backGroundColor;

                //Elements of the paymentItemLayout.
                Button bRemove = GetRemoveButton(user);
                CustomEntry userNameEntry = GetNameEntry(user, 140, 15, Color.Black);
                nameEntries.Add(userNameEntry);
                userNameEntry.BackgroundColor = backGroundColor;
                                
                string userName = user.Name;

                if (string.IsNullOrEmpty(userName))
                    EmptyNameEntry = userNameEntry;

                userNameEntry.Text = userName;
                userNameEntry.Unfocused += EntryName_Unfocused;

                paymentItemLayout.Children.Add(bRemove);
                paymentItemLayout.Children.Add(userNameEntry);
                paymentsListLayout.Children.Add(paymentItemLayout);
                StackLayoutScroll.Children.Add(paymentsListLayout);
                colorIndex++;
            }
        }

        private StackLayout GetNewStackLayout(object bindingContext) =>
           Utils.GetNewStackLayout(bindingContext, StackOrientation.Vertical);

        private Button GetRemoveButton(object bindingContext)
        {
            Button bRemove = GetButton(bindingContext);
            bRemove.WidthRequest = 35;
            bRemove.TextColor = Color.Red;
            bRemove.Clicked += RemoveButton_OnClicked;
            bRemove.ImageSource = "Delete.png";
            return bRemove;
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
        }
        private CustomEntry GetNameEntry(object bindingContext, double width, double fontSize, Color color)
        {
            CustomEntry entry = Utils.GetEntry(bindingContext, width, fontSize, color);
            entry.HorizontalTextAlignment = TextAlignment.Start;
            entry.HorizontalOptions = LayoutOptions.FillAndExpand;
            return entry;
        }

        private async void RemoveButton_OnClicked(object sender, EventArgs e)
        {
            Button bRemove = sender as Button;

            if (bRemove != null 
                && bRemove.BindingContext is Users user 
                && (Utils.SumByUser(user, _tripId) == 0)
                && (!App.Database.Table<UserCostShares>().Where(n => n.TripId == _tripId && n.UserId == user.id).Any())
               )
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (await DisplayAlert(null, Resource.UsersPageUserDeleteMessage, Resource.Yes, Resource.No))
                    {
                        CostValues[] costValues = App.Database.Table<CostValues>().
                        Where(n => n.TripId == _tripId && n.UserId == user.id).ToArray();

                        foreach (var costValue in costValues)
                            SqlCrudUtils.Delete(costValue);

                        UserCostShares[] userShares = App.Database.Table<UserCostShares>().
                        Where(n => n.TripId == _tripId && n.UserId == user.id).ToArray();

                        foreach (var userShare in userShares)
                            SqlCrudUtils.Delete(userShare);


                        TripUsers[] tripUsers = App.Database.Table<TripUsers>().
                         Where(n => n.TripId == _tripId && n.UserId == user.id).ToArray();

                        foreach (var tripUser in tripUsers)
                            SqlCrudUtils.Delete(tripUser);

                        if (!App.Database.Table<UserCostShares>().Where(n =>  n.UserId == user.id).Any() && 
                        (!App.Database.Table<CostValues>().Where(n => n.UserId == user.id).Any()))
                        _users.Remove(user);
                    }
                });
            }
            else
            {
                await DisplayAlert("", Resource.UsersPageUserCanNotBeDeleted, Resource.Cancel);
            }

            RefreshPage();
        }

        private Button GetButton(object bindingContext)
        {
            Button button = new Button
            {
                IsVisible = true,
                BindingContext = bindingContext,
                Style = (Style)this.Resources["ButtonEmpty"],
                HorizontalOptions = LayoutOptions.Start,
                HeightRequest = 35
            };

            return button;
        }

        private Label GetTitleLabel(object bindingContext, string text)
        {
            Label label = Utils.GetLabel(bindingContext, text, 100, 45, 15);
            label.BackgroundColor = Color.LightGray;
            return label;
        }

        private Label UsersLabel;
        private string _tripId;
        private List<Users> _users = new List<Users>();
        private List<CustomEntry> nameEntries = new List<CustomEntry>();
        private Entry EmptyNameEntry;
        private bool _hasUnsavedData = false;
    }
}