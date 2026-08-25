using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.Recoleccion;
using EcoHuellaApp.Infrastructure.Services;
using EcoHuellaApp.Presentation.Views;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;

namespace EcoHuellaApp.Presentation.ViewModels
{
    public sealed partial class CasaViewModel : BaseViewModel
    {
        private readonly IRepositoryGeneric<Casa> _repository;
        private readonly OfflineMapTileService    _tileService;

        public CasaViewModel(IRepositoryGeneric<Casa> repository, OfflineMapTileService tileService)
        {
            _repository  = repository;
            _tileService = tileService;

            _ = CargarCasasAsync();
        }

        // ── Lista y selección ────────────────────────────────────────────────

        [ObservableProperty]
        private ObservableCollection<Casa> _casas = new();

        [ObservableProperty]
        private Casa? _casaSeleccionada;

        partial void OnCasaSeleccionadaChanged(Casa? value)
        {
            if (value is null) return;

            NombreResponsable = value.NombreResponsable;
            Direccion         = value.Direccion;
            Sector            = value.Sector ?? string.Empty;
            Latitud           = value.Latitud.ToString();
            Longitud          = value.Longitud.ToString();
            Estado            = value.Estado;
        }

        // ── Formulario ────────────────────────────────────────────────────────

        [ObservableProperty] private string _nombreResponsable = string.Empty;
        [ObservableProperty] private string _direccion = string.Empty;
        [ObservableProperty] private string _sector = string.Empty;
        [ObservableProperty] private string _latitud = string.Empty;
        [ObservableProperty] private string _longitud = string.Empty;
        [ObservableProperty] private bool _estado = true;

        // ── Comandos ──────────────────────────────────────────────────────────

        [RelayCommand]
        private async Task GuardarAsync() => await ExecuteAsync(async () =>
        {
            if (CasaSeleccionada is null)
            {
                var nuevaCasa = new Casa
                {
                    NombreResponsable = NombreResponsable,
                    Direccion         = Direccion,
                    Sector            = Sector,
                    Estado            = Estado,
                    Latitud           = double.TryParse(Latitud, out var lat) ? lat : 0,
                    Longitud          = double.TryParse(Longitud, out var lon) ? lon : 0
                };

                await _repository.GuardarRegistroAsync(nuevaCasa);
            }
            else
            {
                CasaSeleccionada.NombreResponsable = NombreResponsable;
                CasaSeleccionada.Direccion         = Direccion;
                CasaSeleccionada.Sector            = Sector;
                CasaSeleccionada.Estado            = Estado;

                await _repository.ActualizarAsync(CasaSeleccionada);
            }

            await CargarCasasAsync();
            LimpiarFormulario();
        });

        [RelayCommand]
        private async Task ActualizarAsync() => await ExecuteAsync(async () =>
        {
            if (CasaSeleccionada is null)
            {
                await MostrarAvisoAsync("Seleccione una casa.");
                return;
            }

            CasaSeleccionada.NombreResponsable = NombreResponsable;
            CasaSeleccionada.Direccion         = Direccion;
            CasaSeleccionada.Sector            = Sector;
            CasaSeleccionada.Latitud           = double.TryParse(Latitud, out var lat) ? lat : 0;
            CasaSeleccionada.Longitud          = double.TryParse(Longitud, out var lon) ? lon : 0;
            CasaSeleccionada.Estado            = Estado;

            await _repository.ActualizarAsync(CasaSeleccionada);
            await CargarCasasAsync();
        });

        [RelayCommand]
        private async Task EliminarAsync() => await ExecuteAsync(async () =>
        {
            if (CasaSeleccionada is null)
            {
                await MostrarAvisoAsync("Seleccione una casa.");
                return;
            }

            if (!await ConfirmarAsync("¿Desea eliminar la casa?")) return;

            await _repository.BorrarRegistroAsync(CasaSeleccionada);
            await CargarCasasAsync();
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

        [RelayCommand]
        private async Task ProbarNotificacionAsync()
        {
            var request = new NotificationRequest
            {
                NotificationId = 100,
                Title = "EcoHuella",
                Description = "Prueba de notificación"
            };

            await LocalNotificationCenter.Current.Show(request);
        }

        // ── Helpers privados ─────────────────────────────────────────────────

        private async Task CargarCasasAsync()
        {
            var lista = await _repository.ObtenerTodosAsync();
            Casas = new ObservableCollection<Casa>(lista);
        }

        private void LimpiarFormulario()
        {
            NombreResponsable = string.Empty;
            Direccion         = string.Empty;
            Sector            = string.Empty;
            Latitud           = string.Empty;
            Longitud          = string.Empty;
            Estado            = true;
            CasaSeleccionada  = null;
        }
    }
}
