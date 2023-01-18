using System;
using Apportionment2.Interfaces;
using Apportionment2.Sqlite;
using Xamarin.Forms;

namespace Apportionment2.Pages
{
   // [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StartPage : ContentPage
    {
        public StartPage()
        {
            InitializeComponent();

            //ButtonAbout.Text = Resource.StartMenuItemAbout;
            ButtonExit.Text = Resource.StartMenuItemExit;
            //ButtonHelp.Text = Resource.StartMenuItemHelp;
            ButtonStart.Text = Resource.StartMenuItemStart;
            ButtonLoad.Text = Resource.StartMenuItemLoad;
            ButtonExportDb.Text = "ExprotMySqlDb";
            //ButtonSyncAzure.Text = Resource.StartMenuItemSyncWithAzure;
        }

        private async void ButtonStart_Click(object sender, EventArgs e)
        {
            Trips newTrip = new Trips
            {
                DateBegin = DateTime.Now.ToString(App.DateFormat),
                DateEnd = App.DataEnd,
                id = Guid.NewGuid().ToString()
            };

            CostsPage costsPage = new CostsPage(newTrip);
            await Navigation.PushAsync(costsPage);
        }

        private async void ButtonLoad_Click(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TripsListPage());
        }

        private async void ButtonLoadDb_Click(object sender, EventArgs e)
        {
            await DependencyService.Get<IHtmlReport>().ReplaceMySqlDb();
        }

        private async void ButtonExportDb_Click(object sender, EventArgs e)
        {
            await DependencyService.Get<IHtmlReport>().ShareDb();
        }

        private async void ButtonAbout_Click(object sender, EventArgs e)
        {

        }

        private void ButtonHelp_Click(object sender, EventArgs e)
        {

        }

        private void ButtonExit_Click(object sender, EventArgs e)
        {
            var closer = DependencyService.Get<ICloseApplication>();

            closer?.CloseApplication();
        }

        private async void ButtonSyncAzure_OnClicked(object sender, EventArgs e)
        {
            //await Navigation.PushAsync(new SyncApportionmentPage());
        }
    }
}
