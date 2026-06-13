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

            // Solicita permiso de notificaciones al iniciar
            Task.Run(async () =>
            {
                await LocalNotificationCenter
                    .Current
                    .RequestNotificationPermission();
            });
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Siempre se inicia en LoginPage.
            //
            // La sesión de Firebase Auth se limpia en Preferences/Keychain
            // pero la autorización de Firestore (IUserSessionService) es en memoria.
            // Para garantizar que cada inicio de app valide permisos en Firestore,
            // el usuario debe autenticarse siempre que abre la aplicación.
            //
            // Esto protege contra escenarios como:
            //   - Un usuario fue desactivado mientras la app estaba en segundo plano
            //   - El rol del usuario cambió en Firestore
            var loginPage = _services.GetRequiredService<LoginPage>();
            return new Window(new NavigationPage(loginPage)
            {
                BarBackgroundColor = Colors.Transparent,
                BarTextColor = Colors.Transparent
            });
        }
    }
}