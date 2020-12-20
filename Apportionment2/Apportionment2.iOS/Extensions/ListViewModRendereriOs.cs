using Apportionment2.Extensions;
using Apportionment2.iOS.Extensions;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ListViewMod), typeof(ListViewModRendereriOs))]
namespace Apportionment2.iOS.Extensions
{
    public class ListViewModRendereriOs: ListViewRenderer
    {
        UILongPressGestureRecognizer _longPressGestureRecognizer;
        ListViewMod _list = new ListViewMod();

        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);

            _list = e.NewElement as ListViewMod;

            if (_list == null | Control == null) return;

            _longPressGestureRecognizer = new UILongPressGestureRecognizer(LongPressMethod)
            {
                MinimumPressDuration = 2.0f
            };

            if (e.NewElement == null)
            {
                if (_longPressGestureRecognizer != null)
                {
                    RemoveGestureRecognizer(_longPressGestureRecognizer);
                }
            }

            if (e.OldElement == null)
            {
                AddGestureRecognizer(_longPressGestureRecognizer);
            }
        }
        private void LongPressMethod(UILongPressGestureRecognizer gestureRecognizer)
        {
            if (gestureRecognizer.State == UIGestureRecognizerState.Began)
            {

                var p = gestureRecognizer.LocationInView(Control);
                var longPressOnTheList = Control.IndexPathForRowAtPoint(p);

                if (longPressOnTheList == null)
                    return;

                // if a the long press is on a row on the listView
                var counter = 0;

                foreach (var item in _list.ItemsSource)
                {
                    if (counter == longPressOnTheList.Row)
                    {
                        _list.OnLongClicked(_list, item);
                        break;
                    }

                    counter += 1;
                }
            }
        }

    }
}