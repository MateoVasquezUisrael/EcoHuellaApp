namespace EcoHuellaApp.Presentation.Views.Front;

using System.Collections.ObjectModel;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.ProcesoDegradacion;
using EcoHuellaApp.Infrastructure.Repositories.ProcesoDegradacion;
using Microsoft.Extensions.DependencyInjection;

public partial class vHistorialProcesos : ContentPage
{
    private readonly IRepositoryGeneric<ProcesoBiodigestor>? _activosRepository;
    private readonly ProcesoBiodigestorRepository? _finalizadosRepository;

    public ObservableCollection<ProcesoBiodigestor> Procesos { get; } = [];
    public string Resumen { get; private set; } = "Datos reales registrados";

    public vHistorialProcesos()
    {
        InitializeComponent();
        BindingContext = this;
        var services = Application.Current?.Handler?.MauiContext?.Services;
        _activosRepository = services?.GetService<IRepositoryGeneric<ProcesoBiodigestor>>();
        _finalizadosRepository = services?.GetService<ProcesoBiodigestorRepository>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = CargarAsync();
    }

    private async Task CargarAsync()
    {
        Procesos.Clear();
        var activos = _activosRepository is null ? [] : await _activosRepository.ObtenerTodosAsync();
        var finalizados = _finalizadosRepository is null ? [] : await _finalizadosRepository.ObtenerFinalizadosAsync();

        foreach (var proceso in activos.Concat(finalizados).OrderByDescending(p => p.FechaInicio))
            Procesos.Add(proceso);

        Resumen = $"{Procesos.Count} procesos registrados";
        OnPropertyChanged(nameof(Resumen));
    }
}
