using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoHuellaApp.Domain.Interfaces;

namespace EcoHuellaApp.Presentation.ViewModels
{
    public sealed partial class MainViewModel : BaseViewModel
    {
        private readonly IAuthService        _authService;
        private readonly IUserSessionService _session;
        private readonly INavigationService  _navigation;

        [ObservableProperty] private string _bienvenida  = string.Empty;
        [ObservableProperty] private string _rolUsuario  = string.Empty;
        [ObservableProperty] private string _emailUsuario = string.Empty;

        public MainViewModel(
            IAuthService        authService,
            IUserSessionService session,
            INavigationService  navigation)
        {
            _authService = authService;
            _session     = session;
            _navigation  = navigation;

            // IUserSessionService es la única fuente de verdad tras el login
            var usuario = _session.SistemaUser;
            var auth    = _session.AuthUser;

            Bienvenida   = $"Bienvenido, {usuario?.Nombre ?? auth?.Email ?? "Usuario"}";
            RolUsuario   = usuario?.Rol.ToString() ?? string.Empty;
            EmailUsuario = auth?.Email ?? string.Empty;
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            await ExecuteAsync(async () =>
            {
                _session.ClearSession();          // limpiar sesión Firestore
                await _authService.SignOutAsync(); // cerrar sesión Firebase
                await _navigation.GoToLoginAsync();
            });
        }
    }
}
