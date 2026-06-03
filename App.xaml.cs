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

            Task.Run(async () =>
            {
                await LocalNotificationCenter
                    .Current
                    .RequestNotificationPermission();
            });
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var view = _services.GetRequiredService<CasaView>();
            var view2 = _services.GetRequiredService<RecoleccionView>();

            return new Window(new AppShell());
        }
    }
}