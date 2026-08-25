using EcoHuellaApp.Presentation.ViewModels;

namespace EcoHuellaApp.Presentation.Views;

public partial class CasaView : ContentPage
{
    public CasaView(CasaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
