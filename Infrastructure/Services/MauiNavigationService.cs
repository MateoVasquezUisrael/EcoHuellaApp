using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Presentation.Views;

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

        public void GoToMainApp()
        {
            if (Application.Current?.Windows is { Count: > 0 } windows)
                windows[0].Page = new AppShell();
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