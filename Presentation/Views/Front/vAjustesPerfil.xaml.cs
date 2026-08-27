namespace EcoHuellaApp.Presentation.Views.Front;

using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Presentation.Services;
using Microsoft.Extensions.DependencyInjection;

public partial class vAjustesPerfil : ContentPage
{
    private readonly IUserSessionService? _session;
    private readonly IAuthService? _authService;
    private readonly INavigationService? _navigationService;

    public string Nombre { get; private set; } = "Usuario";
    public string Correo { get; private set; } = string.Empty;
    public string Rol { get; private set; } = string.Empty;
    public string Estado { get; private set; } = string.Empty;
    public string Iniciales { get; private set; } = "U";

    public vAjustesPerfil()
    {
        InitializeComponent();
        BindingContext = this;
        NavegacionInferior.Conectar(this);

        var services = Application.Current?.Handler?.MauiContext?.Services;
        _session = services?.GetService<IUserSessionService>();
        _authService = services?.GetService<IAuthService>();
        _navigationService = services?.GetService<INavigationService>();

        CargarUsuario();
    }

    private void CargarUsuario()
    {
        var usuario = _session?.SistemaUser;
        var auth = _session?.AuthUser;

        Nombre = usuario?.Nombre ?? auth?.DisplayName ?? auth?.Email ?? "Usuario";
        Correo = usuario?.Email ?? auth?.Email ?? string.Empty;
        Rol = usuario?.Rol.ToString() ?? auth?.Role ?? string.Empty;
        Estado = usuario?.Activo == false ? "Inactivo" : "Activo";
        Iniciales = ObtenerIniciales(Nombre);

        OnPropertyChanged(nameof(Nombre));
        OnPropertyChanged(nameof(Correo));
        OnPropertyChanged(nameof(Rol));
        OnPropertyChanged(nameof(Estado));
        OnPropertyChanged(nameof(Iniciales));
    }

    private static string ObtenerIniciales(string nombre)
    {
        var partes = nombre
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(2)
            .Select(p => p[0].ToString().ToUpperInvariant())
            .ToArray();

        return partes.Length == 0 ? "U" : string.Join(string.Empty, partes);
    }

    private async void EditarPerfil_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new vEditarPerfil());
    }

    private async void CambiarContrasena_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new vCambiarContrasena());
    }

    private async void CerrarSesion_Clicked(object? sender, EventArgs e)
    {
        _session?.ClearSession();

        if (_authService is not null)
            await _authService.SignOutAsync();

        if (_navigationService is not null)
            await _navigationService.GoToLoginAsync();
    }
}
