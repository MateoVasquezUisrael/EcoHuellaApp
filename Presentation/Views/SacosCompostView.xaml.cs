using EcoHuellaApp.Presentation.ViewModels;

namespace EcoHuellaApp.Presentation.Views;

public partial class SacosCompostView : ContentPage
{
    public SacosCompostView(SacosCompostViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
