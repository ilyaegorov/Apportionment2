using Android.Content;
using Android.Content.Res;
using Android.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Apportionment2.CustomElements;
using Apportionment2.Droid;
using Android.Widget;
using Android.Runtime;

[assembly: ExportRenderer(typeof(CustomEntry), typeof(CustomEntryRenderer))]
namespace Apportionment2.Droid
{
    public class CustomEntryRenderer : EntryRenderer
    {
        public CustomEntryRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                // Remove entry underlining.
                Control.SetBackgroundColor(global::Android.Graphics.Color.Transparent);
                Control.SetRawInputType(InputTypes.TextFlagNoSuggestions);
                Control.SetHintTextColor(ColorStateList.ValueOf(global::Android.Graphics.Color.White));

                System.IntPtr IntPtrtextViewClass = Android.Runtime.JNIEnv.FindClass(typeof(TextView));
                System.IntPtr mCursorDrawableResProperty = JNIEnv.GetFieldID(IntPtrtextViewClass, "mCursorDrawableRes", "I");
                JNIEnv.SetField(Control.Handle, mCursorDrawableResProperty, 0);
            }
        }
    }
}