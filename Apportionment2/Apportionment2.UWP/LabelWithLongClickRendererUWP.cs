using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Apportionment2.UWP;
using Apportionment2.Extensions;
using Apportionment2.UWP.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;
[assembly: ExportRenderer(typeof(LabelWithLongClick), typeof(LabelWithLongClickRendererUWP))]
namespace Apportionment2.UWP
{
    public class LabelWithLongClickRendererUWP : LabelRenderer
    {
        LabelWithLongClick label;
        private bool isHolding;

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Label> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                label = e.NewElement as LabelWithLongClick;

                isHolding = false;

                Control.IsHoldingEnabled = true;

                if (Device.Idiom == TargetIdiom.Desktop)
                {
                    Control.RightTapped += RightClickOverride;
                    Control.Tapped += LeftClickOverride;
                }
                else if (Device.Idiom == TargetIdiom.Phone || Device.Idiom == TargetIdiom.Tablet)
                {
                    Control.Holding += HoldingOverride;
                }
            }
        }

        private void RightClickOverride(object sender, RightTappedRoutedEventArgs e)
        {
            label.OnLongClicked(sender, null);
        }

        private void LeftClickOverride(object sender, TappedRoutedEventArgs e)
        {
            label.OnClicked(sender, null);
        }

        private void HoldingOverride(object sender, HoldingRoutedEventArgs e)
        {
            if (!isHolding)
            {
                isHolding = true;
                label.OnLongClicked(sender, null);
            }
            else
            {
                isHolding = false;
            }
        }
    }
}
