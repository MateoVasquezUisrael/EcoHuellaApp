namespace EcoHuellaApp.Presentation.Views.Front;

using EcoHuellaApp.Presentation.Services;

public partial class vCambiarContrasena : ContentPage
{
    public vCambiarContrasena()
    {
        InitializeComponent();
        NavegacionInferior.Conectar(this);
    }

    private async void Cancelar_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
