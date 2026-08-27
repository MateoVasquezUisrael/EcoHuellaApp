namespace EcoHuellaApp.Presentation.Views.Front;

using EcoHuellaApp.Presentation.Services;
using EcoHuellaApp.Presentation.Views;

public partial class vGestionResiduos : ContentPage
{
    public vGestionResiduos()
    {
        InitializeComponent();
        NavegacionInferior.Conectar(this);
    }

    private async void NuevaEntrega_Clicked(object? sender, EventArgs e)
    {
        await BackendNavigation.PushAsync<RecoleccionView>(this);
    }

    private async void Casas_Clicked(object? sender, EventArgs e)
    {
        await BackendNavigation.PushAsync<CasaView>(this);
    }

    private async void Puntos_Clicked(object? sender, EventArgs e)
    {
        await BackendNavigation.PushAsync<PuntoRecoleccionView>(this);
    }
}
