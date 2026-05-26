namespace EcoHuellaFront.Views;

public partial class vRegisUsuario : ContentPage
{
	public vRegisUsuario()
	{
		InitializeComponent();
	}

	private async void OnAnadirFotoClicked(object? sender, EventArgs e)
	{
		await DisplayAlertAsync("Foto de perfil", "La selección de foto está pendiente de configurar.", "Aceptar");
	}

	private async void OnRegistrarseClicked(object? sender, EventArgs e)
	{
		await DisplayAlertAsync("Registro", "Usuario registrado correctamente.", "Aceptar");
		await Navigation.PushAsync(new vLogin());
	}

	private async void OnIniciaSesionTapped(object? sender, TappedEventArgs e)
	{
		await Navigation.PushAsync(new vLogin());
	}
}
