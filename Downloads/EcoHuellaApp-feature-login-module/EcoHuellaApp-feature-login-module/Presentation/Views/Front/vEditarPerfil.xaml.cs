namespace EcoHuellaApp.Presentation.Views.Front;

using EcoHuellaApp.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

public partial class vEditarPerfil : ContentPage
{
    public string Nombre { get; private set; } = "Usuario";
    public string Correo { get; private set; } = string.Empty;
    public string Rol { get; private set; } = string.Empty;
    public string Iniciales { get; private set; } = "U";

    public vEditarPerfil()
    {
        InitializeComponent();
        BindingContext = this;

        var session = Application.Current?.Handler?.MauiContext?.Services.GetService<IUserSessionService>();
        var usuario = session?.SistemaUser;
        var auth = session?.AuthUser;

        Nombre = usuario?.Nombre ?? auth?.DisplayName ?? auth?.Email ?? "Usuario";
        Correo = usuario?.Email ?? auth?.Email ?? string.Empty;
        Rol = usuario?.Rol.ToString() ?? auth?.Role ?? string.Empty;
        Iniciales = ObtenerIniciales(Nombre);

        OnPropertyChanged(nameof(Nombre));
        OnPropertyChanged(nameof(Correo));
        OnPropertyChanged(nameof(Rol));
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

    private async void Cancelar_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
