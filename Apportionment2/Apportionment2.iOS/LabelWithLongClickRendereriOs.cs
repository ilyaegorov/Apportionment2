using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apportionment2.iOS;
using Apportionment2.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Foundation;
using UIKit;

[assembly: ExportRenderer(typeof(LabelWithLongClick), typeof(LabelWithLongClickRendereriOs))]
namespace Apportionment2.iOS
{
    public class LabelWithLongClickRendereriOs : LabelRenderer
    {
        UILongPressGestureRecognizer _longPressGestureRecognizer;
        UITapGestureRecognizer _tapGestureRecognizer;
        LabelWithLongClick _label = new LabelWithLongClick();

        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            _label = e.NewElement as LabelWithLongClick;

            if (_label == null | Control == null) return;

            _longPressGestureRecognizer = new UILongPressGestureRecognizer(LongPressMethod)
            {
                MinimumPressDuration = 2.0f
            };

            _tapGestureRecognizer = new UITapGestureRecognizer(PressMethod);

            if (_longPressGestureRecognizer != null)
            {
                if (e.NewElement == null)
                {
                    this.RemoveGestureRecognizer(_longPressGestureRecognizer);
                    this.RemoveGestureRecognizer(_tapGestureRecognizer);
                }
                else if (e.OldElement == null)
                {
                    this.AddGestureRecognizer(_longPressGestureRecognizer);
                    this.AddGestureRecognizer(_tapGestureRecognizer);
                }
            }
        }
        private void LongPressMethod(UILongPressGestureRecognizer gestureRecognizer)
        {
            if (gestureRecognizer.State == UIGestureRecognizerState.Began)
            {
                _label.OnLongClicked(_label, null);
                
            }
        }

        private void PressMethod(UITapGestureRecognizer gestureRecognizer)
        {
            if (gestureRecognizer.State == UIGestureRecognizerState.Began)
            {
                _label.OnClicked(_label, null);

            }
        }
    }
}