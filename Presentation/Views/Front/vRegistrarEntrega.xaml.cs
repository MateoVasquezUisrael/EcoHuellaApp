namespace EcoHuellaApp.Presentation.Views.Front;

using EcoHuellaApp.Presentation.Services;

public partial class vRegistrarEntrega : ContentPage
{
    public vRegistrarEntrega()
    {
        InitializeComponent();
        NavegacionInferior.Conectar(this);
    }

    private async void SeleccionarUbicacion_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new vUbicacion());
    }
}
