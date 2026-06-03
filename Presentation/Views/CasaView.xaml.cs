using EcoHuellaApp.Domain.Models;
using EcoHuellaApp.Repositories.Interfaces;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;



namespace EcoHuellaApp.Presentation.Views;

public partial class CasaView : ContentPage
{

    private readonly IRepositoryGeneric<Casa> _repository;
    private Casa _casaSeleccionada;
    public CasaView(IRepositoryGeneric<Casa> repository)
	{
        InitializeComponent();
        _repository = repository;

        _ = CargarCasas();
    }

    private async Task CargarCasas()
    {
        cvCasas.ItemsSource =
            await _repository.ObtenerTodosAsync();
    }

    private void cvCasas_SelectionChanged(
      object sender,
      SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0)
            return;

        _casaSeleccionada =
            (Casa)e.CurrentSelection.First();

        txtNombreResponsable.Text =
            _casaSeleccionada.NombreResponsable;

        txtDireccion.Text =
            _casaSeleccionada.Direccion;

        txtSector.Text =
            _casaSeleccionada.Sector;

        swEstado.IsToggled =
            _casaSeleccionada.Estado;
    }

    private async void btnGuardar_Clicked(
    object sender,
    EventArgs e)
    {
        if (_casaSeleccionada == null)
        {
            Casa nuevaCasa = new Casa
            {
                NombreResponsable = txtNombreResponsable.Text,
                Direccion = txtDireccion.Text,
                Sector = txtSector.Text,
                Estado = swEstado.IsToggled
            };

            await _repository.GuardarRegistroAsync(nuevaCasa);
        }
        else
        {
            _casaSeleccionada.NombreResponsable =
                txtNombreResponsable.Text;

            _casaSeleccionada.Direccion =
                txtDireccion.Text;

            _casaSeleccionada.Sector =
                txtSector.Text;

            _casaSeleccionada.Estado =
                swEstado.IsToggled;

            await _repository.ActualizarAsync(
                _casaSeleccionada);
        }

        await CargarCasas();

        LimpiarFormulario();
    }

    private void LimpiarFormulario()
    {
        txtNombreResponsable.Text = "";
        txtDireccion.Text = "";
        txtSector.Text = "";

        swEstado.IsToggled = true;

        _casaSeleccionada = null;

        cvCasas.SelectedItem = null;
    }

    private async void btnActualizar_Clicked(
        object sender,
        EventArgs e)
    {
        if (_casaSeleccionada == null)
        {
            await DisplayAlertAsync(
                "Aviso",
                "Seleccione una casa.",
                "Aceptar");

            return;
        }

        _casaSeleccionada.NombreResponsable =
            txtNombreResponsable.Text;

        _casaSeleccionada.Direccion =
            txtDireccion.Text;

        _casaSeleccionada.Sector =
            txtSector.Text;

        _casaSeleccionada.Estado =
            swEstado.IsToggled;

        await _repository.ActualizarAsync(
            _casaSeleccionada);

        await CargarCasas();
    }
    private async void btnEliminar_Clicked(
    object sender,
    EventArgs e)
    {
        if (_casaSeleccionada == null)
        {
            await DisplayAlertAsync(
                "Aviso",
                "Seleccione una casa.",
                "Aceptar");

            return;
        }

        bool respuesta =
            await DisplayAlertAsync(
                "Confirmar",
                "¿Desea eliminar la casa?",
                "Sí",
                "No");

        if (!respuesta)
            return;

        await _repository.BorrarRegistroAsync(
            _casaSeleccionada);

        await CargarCasas();

        LimpiarFormulario();
    }


    private async void BtnNotificacion_Clicked(
    object sender,
    EventArgs e)
    {
        var request = new NotificationRequest
        {
            NotificationId = 100,
            Title = "EcoHuella",
            Description = "Prueba de notificación"
        };

        await LocalNotificationCenter.Current.Show(request);
    }
}