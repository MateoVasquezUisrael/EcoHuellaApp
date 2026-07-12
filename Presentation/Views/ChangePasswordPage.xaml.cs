using EcoHuellaApp.Presentation.ViewModels;
namespace EcoHuellaApp.Presentation.Views
{
    public partial class ChangePasswordPage : ContentPage
    {
        public ChangePasswordPage(ChangePasswordViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
