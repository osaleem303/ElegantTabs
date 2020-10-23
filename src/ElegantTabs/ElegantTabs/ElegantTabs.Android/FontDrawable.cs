using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Renderscripts;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace ElegantTabs.Droid
{
    class FontDrawable : Drawable
    {
        private Context context;
        private String icon;
        TextPaint paint;
        int size = -1;
        int alpha = 255;

        public FontDrawable(Context context, String icon, Typeface typeface, Color color, double fontSize)
        {
            this.context = context;
            this.icon = icon;
            paint = new TextPaint();
            paint.SetTypeface(typeface);
            paint.SetStyle(Paint.Style.Fill);
            paint.TextAlign = Paint.Align.Center;
            paint.UnderlineText = false;
            paint.TextSize = (float)fontSize;
            if (color != null)
                paint.Color = color;
            paint.AntiAlias = true;

        }

        public FontDrawable sizeRes(int dimnRes)
        {
            return sizePx(context.Resources.GetDimensionPixelSize(dimnRes));
        }

        public FontDrawable sizeDp(int size)
        {
            return sizePx(dpToPx(context.Resources, size));
        }


        public static int dpToPx(Resources res, int dp)
        {
            return (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, dp, res.DisplayMetrics);
        }

        public FontDrawable sizePx(int size)
        {
            this.size = size;
            SetBounds(0, 0, size, size);
            InvalidateSelf();
            return this;
        }

        public FontDrawable colorRes(Android.Graphics.Color colorRes)
        {
            paint.Color = colorRes;
            InvalidateSelf();
            return this;
        }


        public FontDrawable Alpha(int alpha)
        {
            SetAlpha(alpha);
            InvalidateSelf();
            return this;
        }

        public override int IntrinsicHeight => size;
        public override int IntrinsicWidth => size;

        public override int Opacity => -1;

        public override void Draw(Canvas canvas)
        {
            paint.TextSize = Bounds.Height();
            Rect textBounds = new Rect();
            String textValue = icon;
            paint.GetTextBounds(textValue, 0, 1, textBounds);
            float textBottom = (Bounds.Height() - textBounds.Height()) / 2f + textBounds.Height() - textBounds.Bottom;
            canvas.DrawText(textValue, Bounds.Width() / 2f, textBottom, paint);
        }

        public override bool IsStateful => true;

        public override bool SetState(int[] stateSet)
        {
            int oldValue = paint.Alpha;
            int newValue = isEnabled(stateSet) ? alpha : (alpha / 2);
            paint.Alpha = newValue;
            return oldValue != newValue;
        }

        public static bool isEnabled(int[] stateSet)
        {
            foreach (var item in stateSet)
            {
                if (item == Android.Resource.Attribute.StateEnabled)
                    return true;
            }
            return false;
        }

        public override void SetAlpha(int alpha)
        {
            SetAlpha(alpha);
            InvalidateSelf();
        }

        public override void SetColorFilter(ColorFilter colorFilter)
        {
            paint.SetColorFilter(colorFilter);
        }

        public override void ClearColorFilter()
        {
            paint.SetColorFilter(null);
        }

        public void setStyle(Paint.Style style)
        {
            paint.SetStyle(style);
        }


    }
}