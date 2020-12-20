using System.ComponentModel;
using Android.Content;
using Apportionment2.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(WebView), typeof(ZoomableWebViewRenderer))]
namespace Apportionment2.Droid
{
    public class ZoomableWebViewRenderer : WebViewRenderer
    {
        public ZoomableWebViewRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (Control != null)
            {
                Control.Settings.SetSupportZoom(true);
                Control.Settings.BuiltInZoomControls = true;
                Control.Settings.DisplayZoomControls = false;
                Control.Settings.UseWideViewPort = true;
                Control.Settings.LoadWithOverviewMode = true;
                Control.Settings.UseWideViewPort = true;
            }

            base.OnElementPropertyChanged(sender, e);
        }
    }
}