using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Apportionment2.Extensions
{
    public class LabelWithLongClick: Label
    {
        public event EventHandler LongClicked;
        public event EventHandler Clicked;

        public LabelWithLongClick() : base()
        {
        }
        /// <summary>
        /// Adds the long tap event.
        /// </summary>
        /// <param name="sender">The list view item</param>
        public void OnClicked(object sender, EventArgs e)
        {
           Clicked?.Invoke(this, new EventArgs());
        }
        /// <summary>
        /// Adds the long tap event.
        /// </summary>
        /// <param name="sender">The list view item</param>
        public void OnLongClicked(object sender, EventArgs e)
        {

            LongClicked?.Invoke(this, new EventArgs());
        }
    }

}
