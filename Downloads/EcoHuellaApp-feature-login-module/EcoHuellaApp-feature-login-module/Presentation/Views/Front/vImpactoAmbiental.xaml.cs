namespace EcoHuellaApp.Presentation.Views.Front;

using EcoHuellaApp.Presentation.Services;

public partial class vImpactoAmbiental : ContentPage
{
    public vImpactoAmbiental()
    {
        InitializeComponent();
        NavegacionInferior.Conectar(this);
    }
}
