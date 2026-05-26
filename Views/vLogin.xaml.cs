namespace EcoHuellaFront.Views;

public partial class vLogin : ContentPage
{
	public vLogin()
	{
		InitializeComponent();
	}

	private async void OnIngresarClicked(object? sender, EventArgs e)
	{
		await Navigation.PushAsync(new MainPage());
	}

	private async void OnGoogleClicked(object? sender, EventArgs e)
	{
		await DisplayAlertAsync("Google", "Inicio de sesión con Google pendiente de configurar.", "Aceptar");
	}

	private async void OnRegistrarseClicked(object? sender, EventArgs e)
	{
		await Navigation.PushAsync(new vRegisUsuario());
	}
}
