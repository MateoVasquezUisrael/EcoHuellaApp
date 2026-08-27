using EcoHuellaApp.Presentation.ViewModels;

namespace EcoHuellaApp.Presentation.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}