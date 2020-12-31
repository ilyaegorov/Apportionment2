using Android.Content;
using Apportionment2.Droid.Extensions;
using Apportionment2.Extensions;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Apportionment2.Droid;

[assembly: ExportRenderer(typeof(LabelWithLongClick), typeof(LabelWithLongClickRendererDroid))]
namespace Apportionment2.Droid
{
    public class LabelWithLongClickRendererDroid : LabelRenderer
    {
        public LabelWithLongClickRendererDroid(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Label> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                LabelWithLongClick view = (LabelWithLongClick)e.NewElement;

                Control.LongClick += (s, args) =>
                {
                    view.OnLongClicked(s, args);
                };

                Control.Click += (s, args) =>
                {
                    view.OnClicked(s, args);
                };
            }
        }
    }
}