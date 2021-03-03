using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Acr.UserDialogs;

namespace AgroVentas.Droid
{
    [Activity(Label = "AgroVentas", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected bool FalseFlag = false;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            Window.SetStatusBarColor(Android.Graphics.Color.Rgb(57, 128, 79));

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            if (FalseFlag)
            {
                var falseJC = new Newtonsoft.Json.Converters.BinaryConverter();
            }
            UserDialogs.Init(this);

            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

        public override void OnBackPressed()
        {
            Toast.MakeText(this, "Back", ToastLength.Short);
        }
    }
}