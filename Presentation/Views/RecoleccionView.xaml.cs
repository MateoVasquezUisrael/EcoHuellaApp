using EcoHuellaApp.Presentation.ViewModels;

namespace EcoHuellaApp.Presentation.Views;

public partial class RecoleccionView : ContentPage
{
    private readonly RecoleccionViewModel _viewModel;

    public RecoleccionView(RecoleccionViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.CargarDatosInicialesAsync();
    }
}
