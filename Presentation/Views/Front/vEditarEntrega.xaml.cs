namespace EcoHuellaApp.Presentation.Views.Front;

using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Presentation.Services;
using Microsoft.Extensions.DependencyInjection;

public partial class vEditarEntrega : ContentPage
{
    public string NombreResponsable { get; private set; } = "Usuario";

    public vEditarEntrega()
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

        NombreResponsable = usuario?.Nombre ?? auth?.DisplayName ?? auth?.Email ?? "Usuario";
        OnPropertyChanged(nameof(NombreResponsable));
    }

    private async void Cancelar_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
