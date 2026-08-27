using Microsoft.UI.Xaml;

namespace EcoHuellaApp.WinUI
{
    /// <summary>Aplicación Windows de EcoHuella.</summary>
    public partial class App : MauiWinUIApplication
    {
        public App() => InitializeComponent();

        protected override MauiApp CreateMauiApp() =>
            MauiProgram.CreateMauiApp();
    }
}
