using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.Recoleccion;
using EcoHuellaApp.Infrastructure.Services;
using EcoHuellaApp.Presentation.Views;

namespace EcoHuellaApp.Presentation.ViewModels
{
    public sealed partial class PuntoRecoleccionViewModel : BaseViewModel
    {
        private readonly IRepositoryGeneric<PuntoRecoleccion> _repository;
        private readonly OfflineMapTileService                _tileService;

        public PuntoRecoleccionViewModel(IRepositoryGeneric<PuntoRecoleccion> repository, OfflineMapTileService tileService)
        {
            _repository  = repository;
            _tileService = tileService;

            _ = CargarPuntosAsync();
        }

        // ── Lista y selección ────────────────────────────────────────────────

        [ObservableProperty]
        private ObservableCollection<PuntoRecoleccion> _puntos = new();

        [ObservableProperty]
        private PuntoRecoleccion? _puntoSeleccionado;

        partial void OnPuntoSeleccionadoChanged(PuntoRecoleccion? value)
        {
            if (value is null) return;

            Direccion = value.Direccion;
            Latitud   = value.Latitud.ToString();
            Longitud  = value.Longitud.ToString();
            Estado    = value.Estado;
        }

        // ── Formulario ────────────────────────────────────────────────────────

        [ObservableProperty] private string _direccion = string.Empty;
        [ObservableProperty] private string _latitud = string.Empty;
        [ObservableProperty] private string _longitud = string.Empty;
        [ObservableProperty] private bool _estado = true;

        // ── Comandos ──────────────────────────────────────────────────────────

        [RelayCommand]
        private async Task GuardarAsync() => await ExecuteAsync(async () =>
        {
            if (PuntoSeleccionado is null)
            {
                var nuevoPunto = new PuntoRecoleccion
                {
                    Direccion = Direccion,
                    Estado    = Estado,
                    Latitud   = double.TryParse(Latitud, out var lat) ? lat : 0,
                    Longitud  = double.TryParse(Longitud, out var lon) ? lon : 0
                };

                await _repository.GuardarRegistroAsync(nuevoPunto);
            }
            else
            {
                PuntoSeleccionado.Direccion = Direccion;
                PuntoSeleccionado.Estado    = Estado;

                await _repository.ActualizarAsync(PuntoSeleccionado);
            }

            await CargarPuntosAsync();
            LimpiarFormulario();
        });

        [RelayCommand]
        private async Task ActualizarAsync() => await ExecuteAsync(async () =>
        {
            if (PuntoSeleccionado is null)
            {
                await MostrarAvisoAsync("Seleccione un punto de recolección.");
                return;
            }

            PuntoSeleccionado.Direccion = Direccion;
            PuntoSeleccionado.Latitud   = double.TryParse(Latitud, out var lat) ? lat : 0;
            PuntoSeleccionado.Longitud  = double.TryParse(Longitud, out var lon) ? lon : 0;
            PuntoSeleccionado.Estado    = Estado;

            await _repository.ActualizarAsync(PuntoSeleccionado);
            await CargarPuntosAsync();
        });

        [RelayCommand]
        private async Task EliminarAsync() => await ExecuteAsync(async () =>
        {
            if (PuntoSeleccionado is null)
            {
                await MostrarAvisoAsync("Seleccione un punto de recolección.");
                return;
            }

            if (!await ConfirmarAsync("¿Desea eliminar el punto de recolección?")) return;

            await _repository.BorrarRegistroAsync(PuntoSeleccionado);
            await CargarPuntosAsync();
            LimpiarFormulario();
        });

        [RelayCommand]
        private async Task SeleccionarUbicacionAsync()
        {
            var picker = new LocationPickerPage(_tileService);

            picker.Disappearing += (s, args) =>
            {
                if (picker.Latitud.HasValue && picker.Longitud.HasValue)
                {
                    Latitud  = picker.Latitud.Value.ToString();
                    Longitud = picker.Longitud.Value.ToString();
                }
            };

            var navegacion = Shell.Current?.Navigation ?? PaginaActual()?.Navigation;
            if (navegacion is not null)
                await navegacion.PushModalAsync(picker);
        }

        // ── Helpers privados ─────────────────────────────────────────────────

        private async Task CargarPuntosAsync()
        {
            var lista = await _repository.ObtenerTodosAsync();
            Puntos = new ObservableCollection<PuntoRecoleccion>(lista);
        }

        private void LimpiarFormulario()
        {
            Direccion          = string.Empty;
            Latitud            = string.Empty;
            Longitud           = string.Empty;
            Estado             = true;
            PuntoSeleccionado  = null;
        }
    }
}
