using EcoHuellaApp.Domain.Models.ProcesoDegradacion;
using EcoHuellaApp.Infrastructure.Repositories.ProcesoDegradacion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace EcoHuellaApp.Presentation.Views;

public partial class ProcesosFinalizadosView : ContentPage
{
    private readonly int? _biodigestorId;
    private readonly ProcesoBiodigestorRepository _procesoRepository;

    public ProcesosFinalizadosView(int? biodigestorId = null)
    {
        InitializeComponent();
        _biodigestorId = biodigestorId;

        var services = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services;

        _procesoRepository = services?.GetRequiredService<ProcesoBiodigestorRepository>()
            ?? throw new InvalidOperationException("No se pudo resolver el repositorio de procesos.");

        lblFiltro.Text = biodigestorId.HasValue
            ? $"Filtrado por biodigestor #{biodigestorId.Value}"
            : "Mostrando todos los biodigestores";

        _ = CargarProcesosFinalizadosAsync();
    }

    private async Task CargarProcesosFinalizadosAsync()
    {
        var finalizados = await _procesoRepository.ObtenerFinalizadosAsync(_biodigestorId);

        cvProcesosFinalizados.ItemsSource = finalizados
            .Where(p => !_biodigestorId.HasValue || p.BiodigestorId == _biodigestorId.Value)
            .OrderByDescending(p => p.FechaCierre)
            .ToList();
    }
}
