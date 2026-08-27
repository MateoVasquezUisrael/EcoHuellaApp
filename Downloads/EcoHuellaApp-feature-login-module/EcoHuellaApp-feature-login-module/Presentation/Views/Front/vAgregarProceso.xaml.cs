namespace EcoHuellaApp.Presentation.Views.Front;

using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.ProcesoDegradacion;
using Microsoft.Extensions.DependencyInjection;

public partial class vAgregarProceso : ContentPage
{
    private readonly IRepositoryGeneric<Biodigestor>? _biodigestorRepository;
    private readonly IRepositoryGeneric<ProcesoBiodigestor>? _procesoRepository;
    private List<Biodigestor> _biodigestores = [];

    public vAgregarProceso()
    {
        InitializeComponent();
        _biodigestorRepository = Application.Current?.Handler?.MauiContext?.Services.GetService<IRepositoryGeneric<Biodigestor>>();
        _procesoRepository = Application.Current?.Handler?.MauiContext?.Services.GetService<IRepositoryGeneric<ProcesoBiodigestor>>();
        dpFechaInicio.Date = DateTime.Today;
        _ = CargarBiodigestoresAsync();
    }

    private async Task CargarBiodigestoresAsync()
    {
        _biodigestores = _biodigestorRepository is null
            ? []
            : await _biodigestorRepository.ObtenerTodosAsync();

        pkBiodigestor.ItemsSource = _biodigestores;
    }

    private async void GuardarProceso_Clicked(object? sender, EventArgs e)
    {
        if (_procesoRepository is null)
        {
            await DisplayAlert("Error", "No se pudo acceder al registro de procesos.", "Aceptar");
            return;
        }

        if (pkBiodigestor.SelectedItem is not Biodigestor biodigestor)
        {
            await DisplayAlert("Aviso", "Selecciona un biodigestor.", "Aceptar");
            return;
        }

        var fechaInicio = dpFechaInicio.Date.GetValueOrDefault(DateTime.Today);

        await _procesoRepository.GuardarRegistroAsync(new ProcesoBiodigestor
        {
            FechaInicio = fechaInicio,
            FechaEstimadaFinProceso = fechaInicio.AddDays(42),
            BiodigestorId = biodigestor.Id,
            EstadoLlenado = false,
            EstadoFinalizado = false,
            MetanoEvitado = 0,
            CarbonoEvitado = 0
        });

        await DisplayAlert("Listo", "Proceso registrado correctamente.", "Aceptar");
        await Navigation.PopAsync();
    }
}
