using System;
using Xamarin.Forms;

namespace Apportionment2.Extensions
{
    /// <summary>
    /// Implementation of the ListViewItem long tap. 
    /// </summary>
    public class ListViewMod : ListView
    {
        public event EventHandler<SelectedItemChangedEventArgs> LongClicked;

        public ListViewMod() : base()
        {
        }

        public ListViewMod(ListViewCachingStrategy strategy) : base(strategy)
        {
        }

        /// <summary>
        /// Adds the long tap event.
        /// </summary>
        /// <param name="sender">The list view item</param>
        public void OnLongClicked(object sender, object item)
        {
            LongClicked?.Invoke(this, new SelectedItemChangedEventArgs(item));
        }
    }
}
