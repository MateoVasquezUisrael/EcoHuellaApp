using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.ProcesoDegradacion;
using EcoHuellaApp.Helpers;
using EcoHuellaApp.Infrastructure.Repositories.ProcesoDegradacion;

namespace EcoHuellaApp.Presentation.Views;

public partial class ProcesosBiodigestorView : ContentPage
{
    private readonly int _biodigestorId;
    private readonly IRepositoryGeneric<Biodigestor> _biodigestorRepository;
    private readonly IRepositoryGeneric<ProcesoBiodigestor> _procesoRepository;
    private readonly ProcesoBiodigestorRepository _procesoRepositoryEspecifico;
    private readonly IRepositoryGeneric<EntradasProcesoBiodigestor> _entradasRepository;

    private ProcesoBiodigestor _procesoSeleccionado;

    public ProcesosBiodigestorView(int biodigestorId)
    {
        InitializeComponent();
        _biodigestorId = biodigestorId;

        var services = Application.Current?.Handler?.MauiContext?.Services;

        _biodigestorRepository = services?.GetRequiredService<IRepositoryGeneric<Biodigestor>>()
            ?? throw new InvalidOperationException("No se pudo resolver el repositorio de biodigestores.");

        _procesoRepository = services?.GetRequiredService<IRepositoryGeneric<ProcesoBiodigestor>>()
            ?? throw new InvalidOperationException("No se pudo resolver el repositorio de procesos.");

        _procesoRepositoryEspecifico = services?.GetRequiredService<ProcesoBiodigestorRepository>()
            ?? throw new InvalidOperationException("No se pudo resolver el repositorio específico de procesos.");

        _entradasRepository = services?.GetRequiredService<IRepositoryGeneric<EntradasProcesoBiodigestor>>()
            ?? throw new InvalidOperationException("No se pudo resolver el repositorio de entradas.");

        dpFechaIngreso.Date = DateTime.Today;
        btnGuardarEntrada.IsEnabled = false;
        lblProcesoSeleccionado.Text = "Ningún proceso seleccionado";

        _ = CargarDatosAsync();
    }

    private async Task CargarDatosAsync()
    {
        var biodigestor = await _biodigestorRepository.ObtenerPorId(_biodigestorId);

        lblInfoBiodigestor.Text = biodigestor is null
            ? $"Biodigestor #{_biodigestorId}"
            : $"Biodigestor #{biodigestor.Id} - Capacidad máxima: {biodigestor.CapacidadMaxima} kg";

        await CargarProcesosAsync();
    }

    private async Task CargarProcesosAsync()
    {
        var todos = await _procesoRepository.ObtenerTodosAsync();

        cvProcesos.ItemsSource = todos
            .Where(p => p.BiodigestorId == _biodigestorId)
            .OrderByDescending(p => p.FechaInicio)
            .ToList();
    }

    private async void cvProcesos_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0)
        {
            _procesoSeleccionado = null;
            cvEntradas.ItemsSource = null;
            lblProcesoSeleccionado.Text = "Ningún proceso seleccionado";
            btnGuardarEntrada.IsEnabled = false;
            return;
        }

        _procesoSeleccionado = (ProcesoBiodigestor)e.CurrentSelection.First();

        lblProcesoSeleccionado.Text =
            $"Proceso #{_procesoSeleccionado.Id} registrado el día " +
            $"{_procesoSeleccionado.FechaInicio:dd/MM/yyyy}";

        btnGuardarEntrada.IsEnabled = true;

        await CargarEntradasAsync();
    }

    private async Task CargarEntradasAsync()
    {
        if (_procesoSeleccionado == null)
            return;

        var todas = await _entradasRepository.ObtenerTodosAsync();

        cvEntradas.ItemsSource = todas
            .Where(en => en.ProcesoBiodigestorId == _procesoSeleccionado.Id)
            .OrderByDescending(en => en.FechaIngreso)
            .ToList();
    }

    private async void btnIniciarProceso_Clicked(
        object sender,
        EventArgs e)
    {
        var nuevoProceso = new ProcesoBiodigestor
        {
            FechaInicio = DateTime.Now,
            BiodigestorId = _biodigestorId,
            Estado = true,
            MetanoEvitado = 0,
            CarbonoEvitado = 0
        };

        await _procesoRepository.GuardarRegistroAsync(nuevoProceso);

        await CargarProcesosAsync();
    }

    private async void btnGuardarEntrada_Clicked(
        object sender,
        EventArgs e)
    {
        if (_procesoSeleccionado == null)
        {
            await DisplayAlert(
                "Proceso no seleccionado",
                "Debe elegir un proceso registrado de la lista para poder agregar una entrada.",
                "Aceptar");

            return;
        }

        int baldes = int.TryParse(txtBaldesIngresados.Text, out var b) ? b : 0;
        var matematicaVerde = new MatematicaVerde();
        double masa = matematicaVerde.CalcularMasa(baldes);

        var nuevaEntrada = new EntradasProcesoBiodigestor
        {
            FechaIngreso = dpFechaIngreso.Date ?? DateTime.Today,
            BaldesIngresados = baldes,
            MasaBaldes = masa,
            ProcesoBiodigestorId = _procesoSeleccionado.Id,
            Estado = true
        };

        await _entradasRepository.GuardarRegistroAsync(nuevaEntrada);

        txtBaldesIngresados.Text = string.Empty;
        dpFechaIngreso.Date = DateTime.Today;

        await CargarEntradasAsync();
    }

    private async void btnFinalizarProceso_Clicked(
        object sender,
        EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ProcesoBiodigestor proceso)
        {
            bool respuesta = await DisplayAlert(
                "Confirmar",
                "¿Desea finalizar este proceso? Se calcularán el metano y carbono evitado.",
                "Sí",
                "No");

            if (!respuesta)
                return;

            try
            {
                await _procesoRepositoryEspecifico.FinalizarProcesoAsync(proceso.Id);

                await CargarProcesosAsync();
                cvEntradas.ItemsSource = null;
                _procesoSeleccionado = null;
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Error",
                    ex.Message,
                    "Aceptar");
            }
        }
    }
}
