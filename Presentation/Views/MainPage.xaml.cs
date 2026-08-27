using EcoHuellaApp.Presentation.ViewModels;

namespace EcoHuellaApp.Presentation.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}