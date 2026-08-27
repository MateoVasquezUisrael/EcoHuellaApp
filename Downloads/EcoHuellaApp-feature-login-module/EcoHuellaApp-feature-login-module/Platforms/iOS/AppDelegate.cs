// Platforms/iOS/AppDelegate.cs
using Foundation;
using UIKit;

namespace EcoHuellaApp
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override bool FinishedLaunching(
            UIApplication application,
            NSDictionary launchOptions)
        {
            Firebase.Core.App.Configure();
            return base.FinishedLaunching(application, launchOptions);
        }
    }
}