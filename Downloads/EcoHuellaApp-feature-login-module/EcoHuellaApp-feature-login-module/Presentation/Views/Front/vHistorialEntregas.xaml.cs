namespace EcoHuellaApp.Presentation.Views.Front;

using System.Collections.ObjectModel;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.Recoleccion;
using EcoHuellaApp.Domain.Models.ProcesoComposteraArtesanal;
using EcoHuellaApp.Domain.Models.ProcesoDegradacion;
using EcoHuellaApp.Domain.Models.Ventas;
using EcoHuellaApp.Presentation.Services;
using EcoHuellaApp.Presentation.Views;
using Microsoft.Extensions.DependencyInjection;
using EcoHuellaApp.Infrastructure.Repositories.ProcesoComposteraArtesanal;

public partial class vHistorialEntregas : ContentPage
{
    public ObservableCollection<RegistroHistorial> Registros { get; } = [];
    public string TotalRegistrosTexto { get; private set; } = "0 registros";

    public vHistorialEntregas()
    {
        InitializeComponent();
        BindingContext = this;
        NavegacionInferior.Conectar(this);

        _ = CargarRecoleccionesAsync();
    }

    private async Task CargarRecoleccionesAsync()
    {
        Registros.Clear();
        var services = Application.Current?.Handler?.MauiContext?.Services;
        var registros = new List<RegistroHistorial>();

        foreach (var item in await ObtenerAsync<Recoleccion>(services))
            registros.Add(new("Entrega", $"Entrega #{item.Id}", item.Fecha,
                $"{item.CantidadCubetas} cubetas · {item.MasaEstimada:N1} kg · Casa: {item.Casa?.Direccion ?? item.CasaId.ToString()} · Punto: {item.PuntoRecoleccion?.Direccion ?? item.PuntoRecoleccionId.ToString()}", item));
        foreach (var item in await ObtenerAsync<Casa>(services))
            registros.Add(new("Casa", item.NombreResponsable, null, $"{item.Direccion} · Sector: {item.Sector ?? "Sin sector"}"));
        foreach (var item in await ObtenerAsync<PuntoRecoleccion>(services))
            registros.Add(new("Punto", $"Punto #{item.Id}", null, item.Direccion));
        foreach (var item in await ObtenerAsync<Biodigestor>(services))
            registros.Add(new("Biodigestor", $"Biodigestor #{item.Id}", null, $"Capacidad máxima: {item.CapacidadMaxima:N1} kg"));
        foreach (var item in await ObtenerAsync<ProcesoBiodigestor>(services))
            registros.Add(new("Proceso", $"Proceso #{item.Id}", item.FechaInicio, $"Biodigestor #{item.BiodigestorId} · {(item.EstadoFinalizado ? "Finalizado" : "Activo")}"));
        foreach (var item in await ObtenerAsync<EntradasProcesoBiodigestor>(services))
            registros.Add(new("Entrada", $"Entrada de proceso #{item.Id}", item.FechaIngreso, $"Proceso #{item.ProcesoBiodigestorId}"));
        var composteraRepository = services?.GetService<ComposteraArtesanalRepository>();
        foreach (var item in composteraRepository is null ? [] : await composteraRepository.ObtenerHistorialAsync())
            registros.Add(new("Compostera", $"Compostera #{item.Id}", null, $"Capacidad: {item.PesoMaximo:N1} kg"));
        foreach (var item in await ObtenerAsync<AccionCompostera>(services))
            registros.Add(new("Compostera", $"{item.TipoAccion} {item.TipoElemento}", item.FechaAccion, $"Compostera #{item.ComposteraArtesanalId}"));
        foreach (var item in await ObtenerAsync<SacosCompost>(services))
            registros.Add(new("Saco", $"Saco #{item.Id}", item.Fecha, item.Estado ? "Disponible" : $"{item.Motivo}: {item.ClienteVenta}"));

        foreach (var registro in registros.OrderByDescending(r => r.Fecha ?? DateTime.MinValue).ThenBy(r => r.Categoria))
            Registros.Add(registro);

        TotalRegistrosTexto = $"{Registros.Count} registros";
        OnPropertyChanged(nameof(TotalRegistrosTexto));
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = CargarRecoleccionesAsync();
    }

    private static async Task<List<T>> ObtenerAsync<T>(IServiceProvider? services) where T : class
    {
        var repository = services?.GetService<IRepositoryGeneric<T>>();
        return repository is null ? [] : await repository.ObtenerTodosAsync();
    }

    private async void Registros_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is RegistroHistorial { Entidad: Recoleccion entrega })
            await Navigation.PushModalAsync(new EntregaHistorialPage(entrega));

        if (sender is CollectionView lista)
            lista.SelectedItem = null;
    }

    public sealed record RegistroHistorial(string Categoria, string Titulo, DateTime? Fecha, string Detalle, object? Entidad = null)
    {
        public string FechaTexto => Fecha.HasValue ? Fecha.Value.ToString("dd/MM/yyyy") : "Registro activo";
    }
}
