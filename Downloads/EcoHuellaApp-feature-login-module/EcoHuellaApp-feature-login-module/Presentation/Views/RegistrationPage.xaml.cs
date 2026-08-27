using EcoHuellaApp.Presentation.ViewModels;

namespace EcoHuellaApp.Presentation.Views;

public partial class RegistrationPage : ContentPage
{
    public RegistrationPage(RegistrationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
