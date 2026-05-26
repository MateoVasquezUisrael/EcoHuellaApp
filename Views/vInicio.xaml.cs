namespace EcoHuellaFront.Views;

public partial class vInicio : ContentPage
{
	public vInicio()
	{
		InitializeComponent();
	}

	private async void OnIniciarSesionClicked(object? sender, EventArgs e)
	{
		await Navigation.PushAsync(new vLogin());
	}

	private async void OnEntrarInvitadoClicked(object? sender, EventArgs e)
	{
		await Navigation.PushAsync(new MainPage());
	}
}
