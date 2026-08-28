using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Presentation.Views;
using EcoHuellaApp.Presentation.Views.Front;

namespace EcoHuellaApp.Infrastructure.Services
{
    public sealed class MauiNavigationService : INavigationService
    {
        private readonly IServiceProvider _services;

        public MauiNavigationService(IServiceProvider services)
        {
            _services = services;
        }

        public async Task GoToChangePasswordAsync()
        {
            var page = _services.GetRequiredService<ChangePasswordPage>();
            if (Application.Current?.Windows[0].Page is NavigationPage navPage)
                await navPage.PushAsync(page);
        }

        public async Task GoToRegistrationAsync()
        {
            var page = _services.GetRequiredService<RegistrationPage>();
            if (Application.Current?.Windows[0].Page is NavigationPage navPage)
                await navPage.PushAsync(page);
        }

        public void GoToMainApp()
        {
            if (Application.Current?.Windows is { Count: > 0 } windows)
                windows[0].Page = new NavigationPage(new vHome())
                {
                    BarBackgroundColor = Color.FromArgb("#0B3D2E"),
                    BarTextColor = Colors.White
                };
        }

        public void GoToGuestDemo()
        {
            if (Application.Current?.Windows is { Count: > 0 } windows)
                windows[0].Page = new NavigationPage(
                    _services.GetRequiredService<GuestDemoPage>())
                {
                    BarBackgroundColor = Colors.Transparent,
                    BarTextColor = Colors.Transparent
                };
        }

        public Task GoToLoginAsync()
        {
            var loginPage = _services.GetRequiredService<LoginPage>();
            if (Application.Current?.Windows is { Count: > 0 } windows)
            {
                windows[0].Page = new NavigationPage(loginPage)
                {
                    BarBackgroundColor = Colors.Transparent,
                    BarTextColor = Colors.Transparent
                };
            }
            return Task.CompletedTask;
        }

        public async Task GoBackAsync()
        {
            if (Application.Current?.Windows[0].Page is NavigationPage navPage)
                await navPage.PopAsync();
        }
    }
}
