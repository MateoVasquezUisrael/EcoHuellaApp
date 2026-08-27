namespace EcoHuellaApp.Presentation.Views.Front;

using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.ProcesoComposteraArtesanal;
using EcoHuellaApp.Domain.Models.ProcesoDegradacion;
using EcoHuellaApp.Domain.Models.Ventas;
using EcoHuellaApp.Infrastructure.Repositories.ProcesoDegradacion;
using EcoHuellaApp.Presentation.Services;
using EcoHuellaApp.Presentation.Views;
using Microsoft.Extensions.DependencyInjection;

public partial class vProcesos : ContentPage
{
    private readonly IRepositoryGeneric<SacosCompost>? _sacosRepository;
    private readonly IRepositoryGeneric<ComposteraArtesanal>? _composteraRepository;
    private readonly IRepositoryGeneric<ProcesoBiodigestor>? _procesoRepository;
    private readonly ProcesoBiodigestorRepository? _procesoRepositoryEspecifico;

    public int TotalSacos { get; private set; }
    public int SacosDisponibles { get; private set; }
    public string SacosDisponiblesTexto => $"{SacosDisponibles} disponibles";
    public int TotalComposteras { get; private set; }
    public int ComposterasActivas { get; private set; }
    public string ComposterasActivasTexto => $"{ComposterasActivas} activas";
    public int TotalProcesos { get; private set; }
    public int ProcesosActivos { get; private set; }
    public int ProcesosFinalizados { get; private set; }
    public string ProcesosActivosTexto => $"{ProcesosActivos} activos";
    public string ResumenGeneral => $"{TotalSacos} sacos, {TotalComposteras} composteras y {TotalProcesos} procesos registrados.";

    public vProcesos()
    {
        InitializeComponent();
        BindingContext = this;
        NavegacionInferior.Conectar(this);

        var services = Application.Current?.Handler?.MauiContext?.Services;
        _sacosRepository = services?.GetService<IRepositoryGeneric<SacosCompost>>();
        _composteraRepository = services?.GetService<IRepositoryGeneric<ComposteraArtesanal>>();
        _procesoRepository = services?.GetService<IRepositoryGeneric<ProcesoBiodigestor>>();
        _procesoRepositoryEspecifico = services?.GetService<ProcesoBiodigestorRepository>();

        _ = CargarResumenAsync();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = CargarResumenAsync();
    }

    private async Task CargarResumenAsync()
    {
        var sacos = _sacosRepository is null ? [] : await _sacosRepository.ObtenerTodosAsync();
        var composteras = _composteraRepository is null ? [] : await _composteraRepository.ObtenerTodosAsync();
        var procesosActivos = _procesoRepository is null ? [] : await _procesoRepository.ObtenerTodosAsync();
        var procesosFinalizados = _procesoRepositoryEspecifico is null ? [] : await _procesoRepositoryEspecifico.ObtenerFinalizadosAsync();

        TotalSacos = sacos.Count;
        SacosDisponibles = sacos.Count(s => s.Estado);
        TotalComposteras = composteras.Count;
        ComposterasActivas = composteras.Count(c => c.Estado);
        ProcesosActivos = procesosActivos.Count;
        ProcesosFinalizados = procesosFinalizados.Count;
        TotalProcesos = ProcesosActivos + ProcesosFinalizados;

        OnPropertyChanged(nameof(TotalSacos));
        OnPropertyChanged(nameof(SacosDisponibles));
        OnPropertyChanged(nameof(SacosDisponiblesTexto));
        OnPropertyChanged(nameof(TotalComposteras));
        OnPropertyChanged(nameof(ComposterasActivas));
        OnPropertyChanged(nameof(ComposterasActivasTexto));
        OnPropertyChanged(nameof(TotalProcesos));
        OnPropertyChanged(nameof(ProcesosActivos));
        OnPropertyChanged(nameof(ProcesosFinalizados));
        OnPropertyChanged(nameof(ProcesosActivosTexto));
        OnPropertyChanged(nameof(ResumenGeneral));
    }

    private async void AgregarSaco_Clicked(object? sender, EventArgs e) => await Navigation.PushAsync(new vAgregarSaco());
    private async void HistorialSacos_Clicked(object? sender, EventArgs e) => await Navigation.PushAsync(new vHistorialSacos());
    private async void AgregarCompostera_Clicked(object? sender, EventArgs e) => await Navigation.PushAsync(new vAgregarCompostera());
    private async void HistorialComposteras_Clicked(object? sender, EventArgs e) => await Navigation.PushAsync(new vHistorialComposteras());
    private async void AgregarProceso_Clicked(object? sender, EventArgs e) => await Navigation.PushAsync(new vAgregarProceso());
    private async void HistorialProcesos_Clicked(object? sender, EventArgs e) => await Navigation.PushAsync(new vHistorialProcesos());
}
