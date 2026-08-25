using EcoHuellaApp.Presentation.ViewModels;

namespace EcoHuellaApp.Presentation.Views;

public partial class PuntoRecoleccionView : ContentPage
{
    public PuntoRecoleccionView(PuntoRecoleccionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
