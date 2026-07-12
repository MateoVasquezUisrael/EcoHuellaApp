using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using EcoHuellaApp.Platforms.Android;

namespace EcoHuellaApp
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.Multiple,
        ConfigurationChanges =
            ConfigChanges.ScreenSize | ConfigChanges.Orientation |
            ConfigChanges.UiMode | ConfigChanges.ScreenLayout |
            ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[MainActivity] OnCreate called. IsFinishing={IsFinishing}, savedInstanceState is null={savedInstanceState is null}");
            base.OnCreate(savedInstanceState);
            GoogleSignInService.Initialize(this);
        }

        protected override void OnActivityResult(
            int requestCode, Result resultCode, Intent? data)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[MainActivity] OnActivityResult fired: requestCode={requestCode}, resultCode={resultCode}");
            base.OnActivityResult(requestCode, resultCode, data);
            GoogleSignInService.HandleActivityResult(requestCode, resultCode, data);
        }

        protected override void OnDestroy()
        {
            System.Diagnostics.Debug.WriteLine("[MainActivity] OnDestroy called.");
            base.OnDestroy();
        }

        protected override void OnNewIntent(Intent? intent)
        {
            System.Diagnostics.Debug.WriteLine("[MainActivity] OnNewIntent called.");
            base.OnNewIntent(intent);
        }
    }
}