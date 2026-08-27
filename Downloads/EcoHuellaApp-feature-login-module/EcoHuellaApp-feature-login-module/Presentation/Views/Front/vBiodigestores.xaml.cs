namespace EcoHuellaApp.Presentation.Views.Front;

using EcoHuellaApp.Presentation.Services;
using EcoHuellaApp.Presentation.Views;

public partial class vBiodigestores : ContentPage
{
    public vBiodigestores()
    {
        InitializeComponent();
        NavegacionInferior.Conectar(this);
    }

    private async void GestionarBiodigestores_Clicked(object? sender, EventArgs e)
    {
        await BackendNavigation.PushAsync<BiodigestoresView>(this);
    }

    private async void Procesos_Clicked(object? sender, EventArgs e)
    {
        await BackendNavigation.PushAsync<ProcesosBiodigestorView>(this);
    }
}
