using Microsoft.UI.Xaml;

namespace EcoHuellaApp.WinUI
{
    /// <summary>
    /// App Windows. Google Sign-In usa HttpListener en localhost — no requiere
    /// manejo de activación por protocolo URI.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        public App() => InitializeComponent();

        protected override MauiApp CreateMauiApp() =>
            MauiProgram.CreateMauiApp();
    }
}
