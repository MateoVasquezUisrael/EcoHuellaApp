using EcoHuellaApp.Domain.Models;
using EcoHuellaApp.Repositories.Interfaces;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;

namespace EcoHuellaApp.Presentation.Views;

public partial class RecoleccionView : ContentPage
{
    private readonly IRepositoryGeneric<Recoleccion> _repository;

    private Recoleccion _recoleccionSeleccionada;
    public RecoleccionView(IRepositoryGeneric<Recoleccion> repository)
	{
        InitializeComponent();

        _repository = repository;

        _ = CargarRecolecciones();
    }
    private async Task CargarRecolecciones()
    {
        cvRecolecciones.ItemsSource =
            await _repository.ObtenerTodosAsync();
    }

    private void cvRecolecciones_SelectionChanged(
    object sender,
    SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0)
            return;

        _recoleccionSeleccionada =
            (Recoleccion)e.CurrentSelection.First();

        dpFecha.Date =
            _recoleccionSeleccionada.Fecha;

        txtLatitud.Text =
            _recoleccionSeleccionada.Latitud.ToString();

        txtLongitud.Text =
            _recoleccionSeleccionada.Longitud.ToString();
    }
    private async void btnUbicacion_Clicked(
    object sender,
    EventArgs e)
    {
        var status =
            await Permissions.CheckStatusAsync
            <Permissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted)
        {
            status =
                await Permissions.RequestAsync
                <Permissions.LocationWhenInUse>();
        }

        if (status != PermissionStatus.Granted)
            return;

        var location =
            await Geolocation.Default.GetLocationAsync();

        if (location == null)
            return;

        txtLatitud.Text =
            location.Latitude.ToString();

        txtLongitud.Text =
            location.Longitude.ToString();
    }

    private async void btnGuardar_Clicked(
    object sender,
    EventArgs e)
    {
        var recoleccion = new Recoleccion
        {
            Fecha = dpFecha.Date,

            Latitud =
                double.Parse(txtLatitud.Text),

            Longitud =
                double.Parse(txtLongitud.Text),

            Estado = true
        };

        await _repository.GuardarRegistroAsync(
            recoleccion);

        await CargarRecolecciones();

        LimpiarFormulario();
    }

    private async void btnActualizar_Clicked(
    object sender,
    EventArgs e)
    {
        if (_recoleccionSeleccionada == null)
            return;

        _recoleccionSeleccionada.Fecha =
            dpFecha.Date;

        _recoleccionSeleccionada.Latitud =
            double.Parse(txtLatitud.Text);

        _recoleccionSeleccionada.Longitud =
            double.Parse(txtLongitud.Text);

        await _repository.ActualizarAsync(
            _recoleccionSeleccionada);

        await CargarRecolecciones();
    }

    private async void btnEliminar_Clicked(
    object sender,
    EventArgs e)
    {
        if (_recoleccionSeleccionada == null)
            return;

        await _repository.BorrarRegistroAsync(
            _recoleccionSeleccionada);

        await CargarRecolecciones();

        LimpiarFormulario();
    }

    private void LimpiarFormulario()
    {
        dpFecha.Date = DateTime.Today;

        txtLatitud.Text = string.Empty;

        txtLongitud.Text = string.Empty;

        _recoleccionSeleccionada = null;

        cvRecolecciones.SelectedItem = null;
    }
}