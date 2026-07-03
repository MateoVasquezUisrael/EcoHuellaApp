using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models;
using EcoHuellaApp.Infrastructure.Services;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;



namespace EcoHuellaApp.Presentation.Views;

public partial class CasaView : ContentPage
{

    private readonly IRepositoryGeneric<Casa> _repository;
    private readonly OfflineMapTileService _tileService;
    private Casa _casaSeleccionada;

    public CasaView(
        IRepositoryGeneric<Casa> repository,
        OfflineMapTileService tileService)
	{
        InitializeComponent();
        _repository = repository;
        _tileService = tileService;

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

        txtLatitud.Text =
            _casaSeleccionada.Latitud.ToString();

        txtLongitud.Text =
            _casaSeleccionada.Longitud.ToString();

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
                Estado = swEstado.IsToggled,
                Latitud = double.TryParse(txtLatitud.Text, out var lat) ? lat : 0,
                Longitud = double.TryParse(txtLongitud.Text, out var lon) ? lon : 0
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
        txtLatitud.Text = string.Empty;
        txtLongitud.Text = string.Empty;

        swEstado.IsToggled = true;

        _casaSeleccionada = null;

        cvCasas.SelectedItem = null;
    }

    private async void btnSeleccionarUbicacion_Clicked(
        object sender,
        EventArgs e)
    {
        var picker = new LocationPickerPage(_tileService);

        picker.Disappearing += (s, args) =>
        {
            if (picker.Latitud.HasValue && picker.Longitud.HasValue)
            {
                txtLatitud.Text = picker.Latitud.Value.ToString();
                txtLongitud.Text = picker.Longitud.Value.ToString();
            }
        };

        await Navigation.PushModalAsync(picker);
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

        _casaSeleccionada.Latitud =
            double.TryParse(txtLatitud.Text, out var latUpdate) ? latUpdate : 0;

        _casaSeleccionada.Longitud =
            double.TryParse(txtLongitud.Text, out var lonUpdate) ? lonUpdate : 0;

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