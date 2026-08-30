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

    private async Task<(bool EsValido, double Latitud, double Longitud)> ValidarFormularioAsync()
    {
        if (string.IsNullOrWhiteSpace(txtDireccion.Text))
        {
            await DisplayAlertAsync("Aviso", "Ingrese la dirección del punto.", "Aceptar");
            return (false, 0, 0);
        }

        if (!double.TryParse(txtLatitud.Text, out var latitud) || latitud < -90 || latitud > 90)
        {
            await DisplayAlertAsync("Aviso", "Seleccione una ubicación válida en el mapa.", "Aceptar");
            return (false, 0, 0);
        }

        if (!double.TryParse(txtLongitud.Text, out var longitud) || longitud < -180 || longitud > 180)
        {
            await DisplayAlertAsync("Aviso", "Seleccione una ubicación válida en el mapa.", "Aceptar");
            return (false, 0, 0);
        }

        return (true, latitud, longitud);
    }

    private async void btnGuardar_Clicked(
        object sender,
        EventArgs e)
    {
        var (esValido, lat, lon) = await ValidarFormularioAsync();
        if (!esValido) return;

        try
        {
            if (_puntoSeleccionado == null)
            {
                var nuevoPunto = new PuntoRecoleccion
                {
                    Direccion = txtDireccion.Text,
                    Estado = swEstado.IsToggled,
                    Latitud = lat,
                    Longitud = lon
                };

                await _repository.GuardarRegistroAsync(nuevoPunto);
            }
            else
            {
                _puntoSeleccionado.Direccion =
                    txtDireccion.Text;

                _puntoSeleccionado.Estado =
                    swEstado.IsToggled;

                _puntoSeleccionado.Latitud = lat;
                _puntoSeleccionado.Longitud = lon;

                await _repository.ActualizarAsync(
                    _puntoSeleccionado);
            }

            await CargarPuntos();

            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo guardar el punto de recolección: {ex.Message}", "Aceptar");
        }
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

        var (esValido, lat, lon) = await ValidarFormularioAsync();
        if (!esValido) return;

        try
        {
            _puntoSeleccionado.Direccion =
                txtDireccion.Text;

            _puntoSeleccionado.Latitud = lat;

            _puntoSeleccionado.Longitud = lon;

            _puntoSeleccionado.Estado =
                swEstado.IsToggled;

            await _repository.ActualizarAsync(
                _puntoSeleccionado);

            await CargarPuntos();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo actualizar el punto de recolección: {ex.Message}", "Aceptar");
        }
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

        try
        {
            await _repository.BorrarRegistroAsync(
                _puntoSeleccionado);

            await CargarPuntos();

            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo eliminar el punto de recolección: {ex.Message}", "Aceptar");
        }
    }
}
