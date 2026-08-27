using EcoHuellaApp.Presentation.Views;
using Microsoft.Extensions.DependencyInjection;
using Plugin.LocalNotification;

namespace EcoHuellaApp
{
    public partial class App : Application
    {
        private readonly IServiceProvider _services;

        public App(IServiceProvider services)
        {
            InitializeComponent();
            _services = services;

            // Permiso para notificaciones.
            Task.Run(async () =>
            {
                await LocalNotificationCenter
                    .Current
                    .RequestNotificationPermission();
            });
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Cada inicio vuelve a validar el acceso.
            var loginPage = _services.GetRequiredService<LoginPage>();
            return new Window(new NavigationPage(loginPage)
            {
                BarBackgroundColor = Colors.Transparent,
                BarTextColor = Colors.Transparent
            });
        }
    }
}
