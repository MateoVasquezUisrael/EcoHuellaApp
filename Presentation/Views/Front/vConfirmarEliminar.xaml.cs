namespace EcoHuellaApp.Presentation.Views.Front;

public partial class vConfirmarEliminar : ContentPage
{
    public vConfirmarEliminar()
    {
        InitializeComponent();
    }

    private async void Cancelar_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
