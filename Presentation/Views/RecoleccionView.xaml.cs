using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.Recoleccion;
using EcoHuellaApp.Helpers;
using EcoHuellaApp.Infrastructure.Services;
using System.Collections.ObjectModel;

namespace EcoHuellaApp.Presentation.Views;

public partial class RecoleccionView : ContentPage
{
    private readonly IRepositoryGeneric<Recoleccion> _repository;
    private readonly IRepositoryGeneric<Casa> _casaRepository;
    private readonly IRepositoryGeneric<PuntoRecoleccion> _puntoRepository;
    private readonly OfflineMapTileService _tileService;

    private ObservableCollection<Casa> _casas;
    private ObservableCollection<PuntoRecoleccion> _puntos;
    private Recoleccion _recoleccionSeleccionada;
    private readonly MatematicaVerde _matematicaVerde;

    public RecoleccionView(
        IRepositoryGeneric<Recoleccion> repository,
        IRepositoryGeneric<Casa> casaRepository,
        IRepositoryGeneric<PuntoRecoleccion> puntoRepository,
        OfflineMapTileService tileService)
    {
        InitializeComponent();

        _repository = repository;
        _casaRepository = casaRepository;
        _puntoRepository = puntoRepository;
        _tileService = tileService;
        _matematicaVerde = new MatematicaVerde();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = CargarDatosIniciales();
    }

    private async Task CargarDatosIniciales()
    {
        try
        {
            var casas = await _casaRepository.ObtenerTodosAsync();
            var puntos = await _puntoRepository.ObtenerTodosAsync();

            _casas = new ObservableCollection<Casa>(casas);
            _puntos = new ObservableCollection<PuntoRecoleccion>(puntos);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                pkCasa.ItemDisplayBinding = new Binding("Direccion");
                pkCasa.ItemsSource = _casas;

                pkPuntoRecoleccion.ItemDisplayBinding = new Binding("Direccion");
                pkPuntoRecoleccion.ItemsSource = _puntos;
            });

            await CargarRecolecciones();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron cargar los datos: {ex.Message}", "Aceptar");
        }
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

        dpFecha.Date = _recoleccionSeleccionada.Fecha ?? DateTime.Today;

        pkCasa.SelectedItem = _casas
            .FirstOrDefault(c => c.Id == _recoleccionSeleccionada.CasaId);

        pkPuntoRecoleccion.SelectedItem = _puntos
            .FirstOrDefault(p => p.Id == _recoleccionSeleccionada.PuntoRecoleccionId);

        txtCantidadCubetas.Text =
            _recoleccionSeleccionada.CantidadCubetas.ToString();

        txtLitrosEstimados.Text =
            _recoleccionSeleccionada.LitrosEstimados.ToString();

        txtMasaEstimada.Text =
            _recoleccionSeleccionada.MasaEstimada.ToString();
    }

    private void txtCantidadCubetas_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        CalcularLitrosYMasa();
    }

    private void CalcularLitrosYMasa()
    {
        if (!int.TryParse(txtCantidadCubetas.Text, out var cantidad) || cantidad < 0)
        {
            txtLitrosEstimados.Text = string.Empty;
            txtMasaEstimada.Text = string.Empty;
            return;
        }

        var litros = cantidad * ConstantesMatematicaVerde.VolumenBaldes;
        var masa = _matematicaVerde.CalcularMasa(cantidad);

        txtLitrosEstimados.Text = litros.ToString();
        txtMasaEstimada.Text = masa.ToString("F2");
    }

    private async void btnVerMapa_Clicked(
        object sender,
        EventArgs e)
    {
        var casa = pkCasa.SelectedItem as Casa;
        var punto = pkPuntoRecoleccion.SelectedItem as PuntoRecoleccion;

        if (casa == null || punto == null)
        {
            await DisplayAlertAsync(
                "Aviso",
                "Seleccione una casa y un punto de recolección.",
                "Aceptar");
            return;
        }

        var mapPage = new RecoleccionMapPage(_tileService, casa, punto);
        await Navigation.PushModalAsync(mapPage);
    }

    private async void btnGuardar_Clicked(
        object sender,
        EventArgs e)
    {
        var casa = pkCasa.SelectedItem as Casa;
        var punto = pkPuntoRecoleccion.SelectedItem as PuntoRecoleccion;

        if (casa == null || punto == null)
        {
            await DisplayAlertAsync(
                "Aviso",
                "Seleccione una casa y un punto de recolección.",
                "Aceptar");
            return;
        }

        if (!int.TryParse(txtCantidadCubetas.Text, out var cantidadCubetas))
        {
            await DisplayAlertAsync(
                "Aviso",
                "Ingrese una cantidad válida de cubetas.",
                "Aceptar");
            return;
        }

        if (_recoleccionSeleccionada == null)
        {
            var recoleccion = new Recoleccion
            {
                Fecha = dpFecha.Date,
                CasaId = casa.Id,
                PuntoRecoleccionId = punto.Id,
                CantidadCubetas = cantidadCubetas,
                LitrosEstimados = double.TryParse(txtLitrosEstimados.Text, out var litros) ? litros : 0,
                MasaEstimada = double.TryParse(txtMasaEstimada.Text, out var masa) ? masa : 0,
                Estado = true
            };

            await _repository.GuardarRegistroAsync(recoleccion);
        }
        else
        {
            _recoleccionSeleccionada.Fecha = dpFecha.Date;
            _recoleccionSeleccionada.CasaId = casa.Id;
            _recoleccionSeleccionada.PuntoRecoleccionId = punto.Id;
            _recoleccionSeleccionada.CantidadCubetas = cantidadCubetas;
            _recoleccionSeleccionada.LitrosEstimados = double.TryParse(txtLitrosEstimados.Text, out var litrosUpdate) ? litrosUpdate : 0;
            _recoleccionSeleccionada.MasaEstimada = double.TryParse(txtMasaEstimada.Text, out var masaUpdate) ? masaUpdate : 0;

            await _repository.ActualizarAsync(_recoleccionSeleccionada);
        }

        await CargarRecolecciones();
        LimpiarFormulario();
    }

    private async void btnActualizar_Clicked(
        object sender,
        EventArgs e)
    {
        if (_recoleccionSeleccionada == null)
        {
            await DisplayAlertAsync(
                "Aviso",
                "Seleccione una recolección.",
                "Aceptar");
            return;
        }

        btnGuardar_Clicked(sender, e);
    }

    private async void btnEliminar_Clicked(
        object sender,
        EventArgs e)
    {
        if (_recoleccionSeleccionada == null)
        {
            await DisplayAlertAsync(
                "Aviso",
                "Seleccione una recolección.",
                "Aceptar");
            return;
        }

        bool respuesta = await DisplayAlertAsync(
            "Confirmar",
            "¿Desea eliminar la recolección?",
            "Sí",
            "No");

        if (!respuesta)
            return;

        await _repository.BorrarRegistroAsync(_recoleccionSeleccionada);

        await CargarRecolecciones();
        LimpiarFormulario();
    }

    private void LimpiarFormulario()
    {
        dpFecha.Date = DateTime.Today;
        pkCasa.SelectedItem = null;
        pkPuntoRecoleccion.SelectedItem = null;
        txtCantidadCubetas.Text = string.Empty;
        txtLitrosEstimados.Text = string.Empty;
        txtMasaEstimada.Text = string.Empty;

        _recoleccionSeleccionada = null;
        cvRecolecciones.SelectedItem = null;
    }
}
