using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Apportionment2.Extensions;
using Apportionment2.UWP.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(ListViewMod), typeof(ListViewModRendererUwp))]
namespace Apportionment2.UWP.Extensions
{
    public class ListViewModRendererUwp: ListViewRenderer
    {
        ListViewMod listViewMod;
        private bool isHolding;

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.ListView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                listViewMod = e.NewElement as ListViewMod;

                isHolding = false;

                Control.IsHoldingEnabled = true;

                if (Device.Idiom == TargetIdiom.Desktop)
                {
                    Control.RightTapped += RightClickOverride;
                }
                else if (Device.Idiom == TargetIdiom.Phone || Device.Idiom == TargetIdiom.Tablet)
                {
                    Control.Holding += HoldingOverride;
                }
            }
        }

        private void RightClickOverride(object sender, RightTappedRoutedEventArgs e)
        {
            var item = (ViewCell)((FrameworkElement)e.OriginalSource).DataContext;
            listViewMod.OnLongClicked(sender, item.BindingContext);
        }

        private void HoldingOverride(object sender, HoldingRoutedEventArgs e)
        {
            if (!isHolding)
            {
                isHolding = true;
                var item = (ViewCell)this.Control.GetType().GetRuntimeProperty("Instance").GetValue(this.Control);
                listViewMod.OnLongClicked(sender, item.BindingContext);
            }
            else
            {
                isHolding = false;
            }
        }
    }
}
