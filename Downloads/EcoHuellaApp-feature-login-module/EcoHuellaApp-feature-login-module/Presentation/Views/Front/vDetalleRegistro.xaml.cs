namespace EcoHuellaApp.Presentation.Views.Front;

using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Presentation.Services;
using Microsoft.Extensions.DependencyInjection;

public partial class vDetalleRegistro : ContentPage
{
    public string ResponsableTexto { get; private set; } = "Usuario: sesión actual";

    public vDetalleRegistro()
    {
        InitializeComponent();
        BindingContext = this;
        NavegacionInferior.Conectar(this);
        CargarResponsable();
    }

    private void CargarResponsable()
    {
        var session = Application.Current?.Handler?.MauiContext?.Services.GetService<IUserSessionService>();
        var usuario = session?.SistemaUser;
        var auth = session?.AuthUser;

        var nombre = usuario?.Nombre ?? auth?.DisplayName ?? auth?.Email ?? "Usuario";
        ResponsableTexto = $"Usuario: {nombre}";
        OnPropertyChanged(nameof(ResponsableTexto));
    }

    private async void Editar_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new vEditarEntrega());
    }

    private async void Eliminar_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new vConfirmarEliminar());
    }

    private async void Volver_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
