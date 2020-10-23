using System;
using Xamarin.Forms.Platform.Android.AppCompat;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using ElegantTabs.Abstraction;
using TabbedPage = Xamarin.Forms.TabbedPage;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using View = Android.Views.View;
using System.Linq;
using Android.Support.Design.Internal;
using Xamarin.Forms;
using Android.Support.Design.BottomNavigation;
using Android.Graphics;
using Android.Graphics.Drawables;
using System.Runtime.InteropServices.WindowsRuntime;

namespace ElegantTabs.Droid
{
    public class ElegantTabRenderer : TabbedPageRenderer, BottomNavigationView.IOnNavigationItemSelectedListener
    {
        private ViewGroup _bottomTabStrip;
        private BottomNavigationView _bottomNavigationView;

        Context context;
        private int tabCount;

        public ElegantTabRenderer(Context context) : base(context)
        {
            this.context = context;
        }

        private int InitLayout()
        {
            switch (this.Element.OnThisPlatform().GetToolbarPlacement())
            {
                case ToolbarPlacement.Default:

                case ToolbarPlacement.Bottom:
                    _bottomTabStrip = ViewGroup.FindChildOfType<BottomNavigationView>()?.GetChildAt(0) as ViewGroup;
                    _bottomNavigationView = ViewGroup.FindChildOfType<BottomNavigationView>();
                    if (_bottomNavigationView != null)
                    {
                        _bottomNavigationView.SetOnNavigationItemSelectedListener(this);
                        _bottomNavigationView.ItemIconTintList = null;
                        _bottomNavigationView.LabelVisibilityMode = LabelVisibilityMode.LabelVisibilityLabeled;
                    }
                    if (_bottomTabStrip == null)
                    {
                        Console.WriteLine("Plugin.Badge: No bottom tab layout found. Badge not added.");
                        return 0;
                    }
                    return _bottomTabStrip.ChildCount;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private void UpdateIcons(int selectedIndex)
        {
            var menu = _bottomNavigationView.Menu;
            for (int i = 0; i < Element.Children.Count; i++)
            {
                //Setting Icon
                var page = Element.GetChildPageWithTransform(i);
                if (i == selectedIndex)
                {
                    if (!Transforms.GetKeepIconColourIntact(page))
                    {
                        var icon = Transforms.GetSelectedIcon(page);
                        if (string.IsNullOrWhiteSpace(icon))
                        {

                            //Draw Label Icon
                            if (page.IconImageSource is FontImageSource)
                                menu.GetItem(i).SetIcon(DrawableFromFont(page.IconImageSource, BarSelectedItemColor));
                            else if (page.IconImageSource is FileImageSource)
                                menu.GetItem(i).SetIcon(IdFromTitle(page.IconImageSource ?? page.Icon, ResourceManager.DrawableClass));
                        }
                        else
                        {
                            //Draw Label Icon
                            if (page.IconImageSource is FontImageSource)
                                menu.GetItem(i).SetIcon(DrawableFromFont(page.IconImageSource, BarSelectedItemColor));
                            else if (page.IconImageSource is FileImageSource)
                                menu.GetItem(i).SetIcon(IdFromTitle(page.IconImageSource ?? page.Icon, ResourceManager.DrawableClass));
                        }
                    }
                    else
                    {
                        if (page.IconImageSource is FontImageSource)
                            menu.GetItem(i).SetIcon(DrawableFromFont(page.IconImageSource, BarSelectedItemColor));
                        else if (page.IconImageSource is FileImageSource)
                            menu.GetItem(i).SetIcon(IdFromTitle(page.IconImageSource ?? page.Icon, ResourceManager.DrawableClass));
                    }
                }
                else
                {
                    //Draw Label Icon
                    if (page.IconImageSource is FontImageSource)
                        menu.GetItem(i).SetIcon(DrawableFromFont(page.IconImageSource, Xamarin.Forms.Color.Transparent));
                    else if(page.IconImageSource is FileImageSource)
                        menu.GetItem(i).SetIcon(IdFromTitle(page.IconImageSource ?? page.Icon, ResourceManager.DrawableClass));
                }
            }
            UpdateTabs();
        }

        private int IdFromTitle(ImageSource imageSource, Type type)
        {
            string name = System.IO.Path.GetFileNameWithoutExtension((FileImageSource)imageSource);
            int id = GetId(type, name);
            return id;
            
        }

        private Drawable DrawableFromFont(ImageSource imageSource, Xamarin.Forms.Color selectedItemColor)
        {
            var fontImage = imageSource as FontImageSource;
            Typeface typeFace = Typeface.CreateFromAsset(context.Assets, fontImage.FontFamily.Split('#')[0]);
            FontDrawable drawable = null;
            if(selectedItemColor != Xamarin.Forms.Color.Transparent)
                drawable = new FontDrawable(context, fontImage.Glyph, typeFace, Android.Graphics.Color.ParseColor(selectedItemColor.ToHex()), fontImage.Size);
            else
                drawable = new FontDrawable(context, fontImage.Glyph, typeFace, Android.Graphics.Color.ParseColor(fontImage.Color.ToHex()), fontImage.Size);
            if (fontImage.Size != 30)
                drawable.sizePx(Convert.ToInt32(fontImage.Size));
            return drawable;
        }

        public new bool OnNavigationItemSelected(IMenuItem item)
        {
            if (Element == null)
                return false;

            int selectedIndex = item.Order;
            Transforms.TabIconClickedFunction(selectedIndex, this);
            var page = Element.GetChildPageWithTransform(selectedIndex);
            if (!Transforms.GetDisableLoad(page))
            {
                if (_bottomNavigationView != null)
                {
                    if (_bottomNavigationView.SelectedItemId != item.ItemId && Element.Children.Count > selectedIndex && selectedIndex >= 0)
                    {
                        Element.CurrentPage = Element.Children[selectedIndex];
                        UpdateIcons(selectedIndex);
                    }
                    else if (_bottomNavigationView.SelectedItemId == item.ItemId)
                    {
                        UpdateIcons(selectedIndex);
                    }
                    else
                    {
                        UpdateIcons(-1);
                    }
                }
                return true;
            }
            return false;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<TabbedPage> e)
        {
            base.OnElementChanged(e);

            // make sure we cleanup old event registrations
            Cleanup(e.OldElement);
            Cleanup(Element);

            tabCount = InitLayout();

            var selectedIndex = Element.Children.IndexOf(Element.CurrentPage);
            UpdateIcons(selectedIndex);
        }

        private void UpdateTabs()
        {
            for (var i = 0; i < tabCount; i++)
            {
                UpdateTab(i);
            }
        }

        int IdFromTitle(string title, Type type)
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(title);
            int id = GetId(type, name);
            return id;
        }

        int GetId(Type type, string memberName)
        {
            object value = type.GetFields().FirstOrDefault(p => p.Name == memberName)?.GetValue(type)
                ?? type.GetProperties().FirstOrDefault(p => p.Name == memberName)?.GetValue(type);
            if (value is int)
                return (int)value;
            return 0;
        }

        private void UpdateTab(int tabIndex)
        {
            var page = Element.GetChildPageWithTransform(tabIndex);
            var menu = (BottomNavigationMenuView)_bottomNavigationView.GetChildAt(0);
            //Setting Title Property and Centering
            if (Transforms.GetHideTitle(page))
            {
                var view = menu.GetChildAt(tabIndex);
                if (view == null) return;
                if (view is BottomNavigationItemView)
                {
                    var viewgroup = (ViewGroup)view;
                    for (int j = 0; j < viewgroup.ChildCount; j++)
                    {
                        View v = viewgroup.GetChildAt(j);
                        if (v is ViewGroup)
                        {
                            v.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            ImageView icon = (ImageView)v;
                            FrameLayout.LayoutParams parames = (FrameLayout.LayoutParams)icon.LayoutParameters;
                            parames.Gravity = GravityFlags.Center;
                        }
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            Cleanup(Element);
            base.Dispose(disposing);
        }

        private void Cleanup(TabbedPage page)
        {
            if (page == null)
            {
                return;
            }
            _bottomTabStrip = null;
            _bottomNavigationView = null;

        }
    }
}