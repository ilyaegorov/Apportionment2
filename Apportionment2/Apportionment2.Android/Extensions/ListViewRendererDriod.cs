using Android.Content;
using Apportionment2.Droid.Extensions;
using Apportionment2.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ListViewMod), typeof(ListViewModRendererDroid))]
namespace Apportionment2.Droid.Extensions
{
    public class ListViewModRendererDroid : ListViewRenderer
    {
        public ListViewModRendererDroid(Context context) : base (context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.ListView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                ListViewMod view = (ListViewMod)e.NewElement;

                Control.ItemLongClick += (s, args) =>
                {
                    Control.SetItemChecked(args.Position, true);
                    Java.Lang.Object item = Control.GetItemAtPosition(args.Position);
                    view.OnLongClicked(s, item.GetType().GetProperty("Instance").GetValue(item));
                };
            }
        }
    }
}