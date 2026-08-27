using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.Recoleccion;
using EcoHuellaApp.Infrastructure.Services;

namespace EcoHuellaApp.Presentation.Views;

public partial class PuntoRecoleccionView : ContentPage
{
    private readonly IRepositoryGeneric<PuntoRecoleccion> _repository;
    private readonly OfflineMapTileService _tileService;
    private PuntoRecoleccion _puntoSeleccionado;

    public PuntoRecoleccionView(
        IRepositoryGeneric<PuntoRecoleccion> repository,
        OfflineMapTileService tileService)
    {
        InitializeComponent();
        _repository = repository;
        _tileService = tileService;

        _ = CargarPuntos();
    }

    private async Task CargarPuntos()
    {
        cvPuntosRecoleccion.ItemsSource =
            await _repository.ObtenerTodosAsync();
    }

    private void cvPuntosRecoleccion_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0)
            return;

        _puntoSeleccionado =
            (PuntoRecoleccion)e.CurrentSelection.First();

        txtDireccion.Text =
            _puntoSeleccionado.Direccion;

        txtLatitud.Text =
            _puntoSeleccionado.Latitud.ToString();

        txtLongitud.Text =
            _puntoSeleccionado.Longitud.ToString();

        swEstado.IsToggled =
            _puntoSeleccionado.Estado;
    }

    private async void btnGuardar_Clicked(
        object sender,
        EventArgs e)
    {
        if (_puntoSeleccionado == null)
        {
            var nuevoPunto = new PuntoRecoleccion
            {
                Direccion = txtDireccion.Text,
                Estado = swEstado.IsToggled,
                Latitud = double.TryParse(txtLatitud.Text, out var lat) ? lat : 0,
                Longitud = double.TryParse(txtLongitud.Text, out var lon) ? lon : 0
            };

            await _repository.GuardarRegistroAsync(nuevoPunto);
        }
        else
        {
            _puntoSeleccionado.Direccion =
                txtDireccion.Text;

            _puntoSeleccionado.Estado =
                swEstado.IsToggled;

            await _repository.ActualizarAsync(
                _puntoSeleccionado);
        }

        await CargarPuntos();

        LimpiarFormulario();
    }

    private void LimpiarFormulario()
    {
        txtDireccion.Text = "";
        txtLatitud.Text = string.Empty;
        txtLongitud.Text = string.Empty;

        swEstado.IsToggled = true;

        _puntoSeleccionado = null;

        cvPuntosRecoleccion.SelectedItem = null;
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
        if (_puntoSeleccionado == null)
        {
            await DisplayAlertAsync(
                "Aviso",
                "Seleccione un punto de recolección.",
                "Aceptar");

            return;
        }

        _puntoSeleccionado.Direccion =
            txtDireccion.Text;

        _puntoSeleccionado.Latitud =
            double.TryParse(txtLatitud.Text, out var latUpdate) ? latUpdate : 0;

        _puntoSeleccionado.Longitud =
            double.TryParse(txtLongitud.Text, out var lonUpdate) ? lonUpdate : 0;

        _puntoSeleccionado.Estado =
            swEstado.IsToggled;

        await _repository.ActualizarAsync(
            _puntoSeleccionado);

        await CargarPuntos();
    }

    private async void btnEliminar_Clicked(
        object sender,
        EventArgs e)
    {
        if (_puntoSeleccionado == null)
        {
            await DisplayAlertAsync(
                "Aviso",
                "Seleccione un punto de recolección.",
                "Aceptar");

            return;
        }

        bool respuesta =
            await DisplayAlertAsync(
                "Confirmar",
                "¿Desea eliminar el punto de recolección?",
                "Sí",
                "No");

        if (!respuesta)
            return;

        await _repository.BorrarRegistroAsync(
            _puntoSeleccionado);

        await CargarPuntos();

        LimpiarFormulario();
    }
}
