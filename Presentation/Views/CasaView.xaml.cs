using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.Recoleccion;
using EcoHuellaApp.Infrastructure.Services;
using Microsoft.Maui.Controls;


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

    private async Task<(bool EsValido, double Latitud, double Longitud)> ValidarFormularioAsync()
    {
        if (string.IsNullOrWhiteSpace(txtNombreResponsable.Text) ||
            string.IsNullOrWhiteSpace(txtDireccion.Text) ||
            string.IsNullOrWhiteSpace(txtSector.Text))
        {
            await DisplayAlertAsync("Aviso", "Complete responsable, dirección y sector.", "Aceptar");
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
            if (_casaSeleccionada == null)
            {
                Casa nuevaCasa = new Casa
                {
                    NombreResponsable = txtNombreResponsable.Text,
                    Direccion = txtDireccion.Text,
                    Sector = txtSector.Text,
                    Estado = swEstado.IsToggled,
                    Latitud = lat,
                    Longitud = lon
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

                _casaSeleccionada.Latitud = lat;
                _casaSeleccionada.Longitud = lon;

                await _repository.ActualizarAsync(
                    _casaSeleccionada);
            }

            await CargarCasas();

            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo guardar la casa: {ex.Message}", "Aceptar");
        }
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

        var (esValido, lat, lon) = await ValidarFormularioAsync();
        if (!esValido) return;

        try
        {
            _casaSeleccionada.NombreResponsable =
                txtNombreResponsable.Text;

            _casaSeleccionada.Direccion =
                txtDireccion.Text;

            _casaSeleccionada.Sector =
                txtSector.Text;

            _casaSeleccionada.Latitud = lat;

            _casaSeleccionada.Longitud = lon;

            _casaSeleccionada.Estado =
                swEstado.IsToggled;

            await _repository.ActualizarAsync(
                _casaSeleccionada);

            await CargarCasas();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo actualizar la casa: {ex.Message}", "Aceptar");
        }
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

        try
        {
            await _repository.BorrarRegistroAsync(
                _casaSeleccionada);

            await CargarCasas();

            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo eliminar la casa: {ex.Message}", "Aceptar");
        }
    }


}
