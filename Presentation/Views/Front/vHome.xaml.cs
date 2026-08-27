namespace EcoHuellaApp.Presentation.Views.Front;

using System.Collections.ObjectModel;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.ProcesoDegradacion;
using EcoHuellaApp.Domain.Models.Recoleccion;
using EcoHuellaApp.Domain.Models.Ventas;
using EcoHuellaApp.Infrastructure.Repositories.ProcesoDegradacion;
using EcoHuellaApp.Presentation.Services;
using Microsoft.Extensions.DependencyInjection;

public partial class vHome : ContentPage
{
    private readonly IRepositoryGeneric<Recoleccion>? _recoleccionRepository;
    private readonly IRepositoryGeneric<ProcesoBiodigestor>? _procesoRepository;
    private readonly ProcesoBiodigestorRepository? _procesoRepositoryEspecifico;
    private readonly IRepositoryGeneric<SacosCompost>? _sacosRepository;

    public ObservableCollection<BarraMensual> EvolucionMensual { get; } = [];

    public string TotalKilosTexto { get; private set; } = "0 kg";
    public string Co2EvitadoTexto { get; private set; } = "0 kg";
    public string MetanoEvitadoTexto { get; private set; } = "0 kg";
    public string TierraRegeneradaTexto { get; private set; } = "Sin datos";
    public string EficienciaTexto { get; private set; } = "0%";
    public double EficienciaProgreso { get; private set; }
    public string ResumenImpacto { get; private set; } = "Cargando métricas reales...";

    public vHome()
    {
        InitializeComponent();
        BindingContext = this;
        NavegacionInferior.Conectar(this);

        var services = Application.Current?.Handler?.MauiContext?.Services;
        _recoleccionRepository = services?.GetService<IRepositoryGeneric<Recoleccion>>();
        _procesoRepository = services?.GetService<IRepositoryGeneric<ProcesoBiodigestor>>();
        _procesoRepositoryEspecifico = services?.GetService<ProcesoBiodigestorRepository>();
        _sacosRepository = services?.GetService<IRepositoryGeneric<SacosCompost>>();

        dpHasta.Date = DateTime.Today;
        dpDesde.Date = DateTime.Today.AddMonths(-5);

        _ = CargarDashboardAsync();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = CargarDashboardAsync();
    }

    private async Task CargarDashboardAsync()
    {
        var desde = dpDesde.Date.GetValueOrDefault(DateTime.Today.AddMonths(-5)).Date;
        var hasta = dpHasta.Date.GetValueOrDefault(DateTime.Today).Date.AddDays(1).AddTicks(-1);

        List<Recoleccion> recolecciones = [];
        List<ProcesoBiodigestor> procesosActivos = [];
        List<ProcesoBiodigestor> procesosFinalizados = [];
        List<SacosCompost> sacos = [];

        if (_recoleccionRepository is not null)
        {
            recolecciones = (await _recoleccionRepository.ObtenerTodosAsync())
                .Where(r => r.Fecha is not null && r.Fecha.Value >= desde && r.Fecha.Value <= hasta)
                .ToList();
        }

        if (_procesoRepository is not null)
        {
            procesosActivos = await _procesoRepository.ObtenerTodosAsync();
        }

        if (_procesoRepositoryEspecifico is not null)
        {
            procesosFinalizados = (await _procesoRepositoryEspecifico.ObtenerFinalizadosAsync())
                .Where(p => p.FechaCierre is not null && p.FechaCierre.Value >= desde && p.FechaCierre.Value <= hasta)
                .ToList();
        }

        if (_sacosRepository is not null)
        {
            sacos = await _sacosRepository.ObtenerTodosAsync();
        }

        var totalKilos = recolecciones.Sum(r => r.MasaEstimada);
        var co2Evitado = procesosFinalizados.Sum(p => p.CarbonoEvitado);
        var metanoEvitado = procesosFinalizados.Sum(p => p.MetanoEvitado);
        var procesosTotales = procesosActivos.Count + procesosFinalizados.Count;
        var eficiencia = procesosTotales == 0 ? 0 : (double)procesosFinalizados.Count / procesosTotales;
        var sacosDisponibles = sacos.Count(s => s.Estado);

        TotalKilosTexto = FormatearKg(totalKilos);
        Co2EvitadoTexto = FormatearKg(co2Evitado);
        MetanoEvitadoTexto = FormatearKg(metanoEvitado);
        TierraRegeneradaTexto = sacosDisponibles > 0 ? $"{sacosDisponibles} sacos" : "Sin datos";
        EficienciaProgreso = eficiencia;
        EficienciaTexto = $"{eficiencia:P0}";
        ResumenImpacto = $"{recolecciones.Count} entregas y {procesosFinalizados.Count} procesos finalizados en el rango.";

        CargarEvolucionMensual(recolecciones);
        RefrescarBindings();
    }

    private void CargarEvolucionMensual(List<Recoleccion> recolecciones)
    {
        EvolucionMensual.Clear();

        var meses = Enumerable.Range(0, 6)
            .Select(offset => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(offset - 5))
            .ToList();

        var valores = meses
            .Select(mes => new
            {
                Mes = mes,
                Kilos = recolecciones
                    .Where(r => r.Fecha is not null &&
                                r.Fecha.Value.Year == mes.Year &&
                                r.Fecha.Value.Month == mes.Month)
                    .Sum(r => r.MasaEstimada)
            })
            .ToList();

        var maximo = Math.Max(1, valores.Max(v => v.Kilos));
        var colores = new[] { "#B8EEE0", "#37D6D6", "#42B883", "#8CDDB9", "#0E6B55", "#42B883" };

        for (var i = 0; i < valores.Count; i++)
        {
            EvolucionMensual.Add(new BarraMensual
            {
                Mes = valores[i].Mes.ToString("MMM")[..3],
                Altura = Math.Max(18, 104 * valores[i].Kilos / maximo),
                Color = Color.FromArgb(colores[i])
            });
        }
    }

    private void RefrescarBindings()
    {
        OnPropertyChanged(nameof(TotalKilosTexto));
        OnPropertyChanged(nameof(Co2EvitadoTexto));
        OnPropertyChanged(nameof(MetanoEvitadoTexto));
        OnPropertyChanged(nameof(TierraRegeneradaTexto));
        OnPropertyChanged(nameof(EficienciaTexto));
        OnPropertyChanged(nameof(EficienciaProgreso));
        OnPropertyChanged(nameof(ResumenImpacto));
    }

    private static string FormatearKg(double valor)
    {
        return valor >= 1000
            ? $"{valor / 1000:N1} ton"
            : $"{valor:N1} kg";
    }

    private async void FiltroFecha_DateSelected(object? sender, DateChangedEventArgs e)
    {
        if (dpDesde.Date <= dpHasta.Date)
        {
            await CargarDashboardAsync();
        }
    }

    private async void DescargarReporte_Clicked(object? sender, EventArgs e)
    {
        await DisplayAlert("Reporte", "La descarga visual está lista para conectarse a la exportación cuando exista.", "Aceptar");
    }

    public sealed class BarraMensual
    {
        public string Mes { get; init; } = string.Empty;
        public double Altura { get; init; }
        public Color Color { get; init; } = Colors.Green;
    }
}
