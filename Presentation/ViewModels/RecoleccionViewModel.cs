using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.Recoleccion;
using EcoHuellaApp.Helpers;
using EcoHuellaApp.Infrastructure.Services;
using EcoHuellaApp.Presentation.Views;

namespace EcoHuellaApp.Presentation.ViewModels
{
    public sealed partial class RecoleccionViewModel : BaseViewModel
    {
        private readonly IRepositoryGeneric<Recoleccion>      _repository;
        private readonly IRepositoryGeneric<Casa>              _casaRepository;
        private readonly IRepositoryGeneric<PuntoRecoleccion>  _puntoRepository;
        private readonly OfflineMapTileService                 _tileService;
        private readonly MatematicaVerde                       _matematicaVerde = new();

        public RecoleccionViewModel(
            IRepositoryGeneric<Recoleccion>     repository,
            IRepositoryGeneric<Casa>            casaRepository,
            IRepositoryGeneric<PuntoRecoleccion> puntoRepository,
            OfflineMapTileService                tileService)
        {
            _repository      = repository;
            _casaRepository  = casaRepository;
            _puntoRepository = puntoRepository;
            _tileService     = tileService;
        }

        /// <summary>Se llama desde OnAppearing de la vista — los catálogos (casas/puntos) pueden
        /// haber cambiado en otro tab desde la última vez que se mostró esta página.</summary>
        public async Task CargarDatosInicialesAsync() => await ExecuteAsync(async () =>
        {
            var casas  = await _casaRepository.ObtenerTodosAsync();
            var puntos = await _puntoRepository.ObtenerTodosAsync();

            Casas  = new ObservableCollection<Casa>(casas);
            Puntos = new ObservableCollection<PuntoRecoleccion>(puntos);

            await CargarRecoleccionesAsync();
        });

        // ── Catálogos ─────────────────────────────────────────────────────────

        [ObservableProperty] private ObservableCollection<Casa> _casas = new();
        [ObservableProperty] private ObservableCollection<PuntoRecoleccion> _puntos = new();

        [ObservableProperty] private Casa? _casaSeleccionada;
        [ObservableProperty] private PuntoRecoleccion? _puntoSeleccionado;

        // ── Lista y selección de recolecciones ──────────────────────────────────

        [ObservableProperty] private ObservableCollection<Recoleccion> _recolecciones = new();
        [ObservableProperty] private Recoleccion? _recoleccionSeleccionada;

        partial void OnRecoleccionSeleccionadaChanged(Recoleccion? value)
        {
            if (value is null) return;

            Fecha              = value.Fecha ?? DateTime.Today;
            CasaSeleccionada   = Casas.FirstOrDefault(c => c.Id == value.CasaId);
            PuntoSeleccionado  = Puntos.FirstOrDefault(p => p.Id == value.PuntoRecoleccionId);
            CantidadCubetas    = value.CantidadCubetas.ToString();

            // El recálculo automático de CantidadCubetas puede diferir del valor
            // guardado si las constantes cambiaron — se sobreescribe con el valor real.
            LitrosEstimados = value.LitrosEstimados.ToString();
            MasaEstimada    = value.MasaEstimada.ToString("F2");
        }

        // ── Formulario ────────────────────────────────────────────────────────

        [ObservableProperty] private DateTime _fecha = DateTime.Today;
        [ObservableProperty] private string _cantidadCubetas = string.Empty;
        [ObservableProperty] private string _litrosEstimados = string.Empty;
        [ObservableProperty] private string _masaEstimada = string.Empty;

        partial void OnCantidadCubetasChanged(string value)
        {
            if (!int.TryParse(value, out var cantidad) || cantidad < 0)
            {
                LitrosEstimados = string.Empty;
                MasaEstimada    = string.Empty;
                return;
            }

            var litros = cantidad * ConstantesMatematicaVerde.VolumenBaldes;
            var masa   = _matematicaVerde.CalcularMasa(cantidad);

            LitrosEstimados = litros.ToString();
            MasaEstimada    = masa.ToString("F2");
        }

        // ── Comandos ──────────────────────────────────────────────────────────

        [RelayCommand]
        private async Task VerMapaAsync()
        {
            if (CasaSeleccionada is null || PuntoSeleccionado is null)
            {
                await MostrarAvisoAsync("Seleccione una casa y un punto de recolección.");
                return;
            }

            var mapPage = new RecoleccionMapPage(_tileService, CasaSeleccionada, PuntoSeleccionado);
            var navegacion = Shell.Current?.Navigation ?? PaginaActual()?.Navigation;
            if (navegacion is not null)
                await navegacion.PushModalAsync(mapPage);
        }

        [RelayCommand]
        private async Task GuardarAsync() => await ExecuteAsync(GuardarInternoAsync);

        [RelayCommand]
        private async Task ActualizarAsync() => await ExecuteAsync(async () =>
        {
            if (RecoleccionSeleccionada is null)
            {
                await MostrarAvisoAsync("Seleccione una recolección.");
                return;
            }

            await GuardarInternoAsync();
        });

        private async Task GuardarInternoAsync()
        {
            if (CasaSeleccionada is null || PuntoSeleccionado is null)
            {
                await MostrarAvisoAsync("Seleccione una casa y un punto de recolección.");
                return;
            }

            if (!int.TryParse(CantidadCubetas, out var cantidadCubetas))
            {
                await MostrarAvisoAsync("Ingrese una cantidad válida de cubetas.");
                return;
            }

            if (RecoleccionSeleccionada is null)
            {
                var recoleccion = new Recoleccion
                {
                    Fecha               = Fecha,
                    CasaId              = CasaSeleccionada.Id,
                    PuntoRecoleccionId  = PuntoSeleccionado.Id,
                    CantidadCubetas     = cantidadCubetas,
                    LitrosEstimados     = double.TryParse(LitrosEstimados, out var litros) ? litros : 0,
                    MasaEstimada        = double.TryParse(MasaEstimada, out var masa) ? masa : 0,
                    Estado              = true
                };

                await _repository.GuardarRegistroAsync(recoleccion);
            }
            else
            {
                RecoleccionSeleccionada.Fecha              = Fecha;
                RecoleccionSeleccionada.CasaId              = CasaSeleccionada.Id;
                RecoleccionSeleccionada.PuntoRecoleccionId  = PuntoSeleccionado.Id;
                RecoleccionSeleccionada.CantidadCubetas     = cantidadCubetas;
                RecoleccionSeleccionada.LitrosEstimados     = double.TryParse(LitrosEstimados, out var litrosUpdate) ? litrosUpdate : 0;
                RecoleccionSeleccionada.MasaEstimada        = double.TryParse(MasaEstimada, out var masaUpdate) ? masaUpdate : 0;

                await _repository.ActualizarAsync(RecoleccionSeleccionada);
            }

            await CargarRecoleccionesAsync();
            LimpiarFormulario();
        }

        [RelayCommand]
        private async Task EliminarAsync() => await ExecuteAsync(async () =>
        {
            if (RecoleccionSeleccionada is null)
            {
                await MostrarAvisoAsync("Seleccione una recolección.");
                return;
            }

            if (!await ConfirmarAsync("¿Desea eliminar la recolección?")) return;

            await _repository.BorrarRegistroAsync(RecoleccionSeleccionada);
            await CargarRecoleccionesAsync();
            LimpiarFormulario();
        });

        // ── Helpers privados ─────────────────────────────────────────────────

        private async Task CargarRecoleccionesAsync()
        {
            var lista = await _repository.ObtenerTodosAsync();
            Recolecciones = new ObservableCollection<Recoleccion>(lista);
        }

        private void LimpiarFormulario()
        {
            Fecha                    = DateTime.Today;
            CasaSeleccionada         = null;
            PuntoSeleccionado        = null;
            CantidadCubetas          = string.Empty;
            LitrosEstimados          = string.Empty;
            MasaEstimada             = string.Empty;
            RecoleccionSeleccionada  = null;
        }
    }
}
