using Microsoft.Extensions.DependencyInjection;
using EcoHuellaApp.Presentation.Views;

namespace EcoHuellaApp
{
    public partial class App : Application
    {
        private readonly IServiceProvider _services;

        public App(IServiceProvider services)
        {
            InitializeComponent();
            _services = services;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var view = _services.GetRequiredService<CasaView>();

            return new Window(view);
        }
    }
}