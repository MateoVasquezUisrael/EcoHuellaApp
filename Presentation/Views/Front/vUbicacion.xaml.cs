namespace EcoHuellaApp.Presentation.Views.Front;

using EcoHuellaApp.Presentation.Services;

public partial class vUbicacion : ContentPage
{
    public vUbicacion()
    {
        InitializeComponent();
        NavegacionInferior.Conectar(this);
    }
}
