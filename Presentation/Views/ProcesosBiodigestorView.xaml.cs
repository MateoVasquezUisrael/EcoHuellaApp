using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.ProcesoDegradacion;
using EcoHuellaApp.Helpers;
using EcoHuellaApp.Infrastructure.Repositories.ProcesoDegradacion;

namespace EcoHuellaApp.Presentation.Views;

public partial class ProcesosBiodigestorView : ContentPage
{
    private int _biodigestorId;
    private readonly IRepositoryGeneric<Biodigestor> _biodigestorRepository;
    private readonly IRepositoryGeneric<ProcesoBiodigestor> _procesoRepository;
    private readonly ProcesoBiodigestorRepository _procesoRepositoryEspecifico;
    public ProcesosBiodigestorView(int biodigestorId = 0)
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

        _ = CargarDatosAsync();
    }

    private async Task CargarDatosAsync()
    {
        Biodigestor? biodigestor;
        if (_biodigestorId == 0)
        {
            biodigestor = (await _biodigestorRepository.ObtenerTodosAsync()).FirstOrDefault();
            _biodigestorId = biodigestor?.Id ?? 0;
        }
        else
        {
            biodigestor = await _biodigestorRepository.ObtenerPorId(_biodigestorId);
        }

        lblInfoBiodigestor.Text = biodigestor is null
            ? "No hay biodigestores disponibles"
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

    private async void btnIniciarProceso_Clicked(
        object sender,
        EventArgs e)
    {
        if (_biodigestorId == 0)
        {
            await DisplayAlert("Sin biodigestor", "No hay un biodigestor disponible para iniciar el proceso.", "Aceptar");
            return;
        }

        try
        {
            var nuevoProceso = new ProcesoBiodigestor
            {
                FechaInicio = DateTime.Now,
                FechaEstimadaFinProceso = DateTime.Now.AddDays(42),
                BiodigestorId = _biodigestorId,
                EstadoLlenado = false,
                EstadoFinalizado = false,
                MetanoEvitado = 0,
                CarbonoEvitado = 0
            };

            await _procesoRepository.GuardarRegistroAsync(nuevoProceso);

            await CargarProcesosAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo iniciar el proceso: {ex.Message}", "Aceptar");
        }
    }

    private async void btnMarcarLleno_Clicked(
        object sender,
        EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ProcesoBiodigestor proceso)
        {
            bool respuesta = await DisplayAlert(
                "Confirmar",
                "¿Desea marcar este proceso como lleno? No se podrán agregar más entradas.",
                "Sí",
                "No");

            if (!respuesta)
                return;

            try
            {
                proceso.EstadoLlenado = true;
                await _procesoRepository.GuardarRegistroAsync(proceso);

                await CargarProcesosAsync();

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
