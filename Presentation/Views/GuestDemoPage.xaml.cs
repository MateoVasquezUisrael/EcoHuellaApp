using EcoHuellaApp.Domain.Interfaces;

namespace EcoHuellaApp.Presentation.Views;

public partial class GuestDemoPage : ContentPage
{
    private readonly INavigationService _navigation;
    private readonly IUserSessionService _session;

    public GuestDemoPage(INavigationService navigation, IUserSessionService session)
    {
        InitializeComponent();
        _navigation = navigation;
        _session = session;
    }

    private async void BackToLogin_Clicked(object? sender, EventArgs e)
    {
        _session.ClearSession();
        await _navigation.GoToLoginAsync();
    }
}
