using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.Ventas;
using EcoHuellaApp.Infrastructure.Repositories.Ventas;

namespace EcoHuellaApp.Presentation.ViewModels
{
    public sealed partial class SacosCompostViewModel : BaseViewModel
    {
        private readonly IRepositoryGeneric<SacosCompost> _sacosRepository;
        private readonly SacosCompostRepository            _sacosRepositoryEspecifico;

        public SacosCompostViewModel(
            IRepositoryGeneric<SacosCompost> sacosRepository,
            SacosCompostRepository           sacosRepositoryEspecifico)
        {
            _sacosRepository           = sacosRepository;
            _sacosRepositoryEspecifico = sacosRepositoryEspecifico;

            _ = CargarSacosAsync();
        }

        // ── Alta de saco ─────────────────────────────────────────────────────

        [ObservableProperty] private DateTime _fechaRegistro = DateTime.Today;

        [RelayCommand]
        private async Task GuardarSacoAsync() => await ExecuteAsync(async () =>
        {
            var nuevoSaco = new SacosCompost
            {
                Fecha        = FechaRegistro,
                Estado       = true,
                Motivo       = null,
                ClienteVenta = null
            };

            await _sacosRepository.GuardarRegistroAsync(nuevoSaco);

            FechaRegistro = DateTime.Today;
            await CargarSacosAsync();
        });

        // ── Listas ────────────────────────────────────────────────────────────

        [ObservableProperty] private ObservableCollection<SacosCompost> _sacosDisponibles = new();
        [ObservableProperty] private ObservableCollection<SacosCompost> _sacosUsados = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFormularioUsoVisible))]
        private SacosCompost? _sacoSeleccionado;

        public bool IsFormularioUsoVisible => SacoSeleccionado is not null;

        // ── Formulario uso / venta ───────────────────────────────────────────

        [ObservableProperty] private string? _motivoSeleccionado;

        partial void OnMotivoSeleccionadoChanged(string? value)
        {
            IsClienteVisible = value == MotivosSaco.VENTA;
            if (value != MotivosSaco.VENTA)
                ClienteVenta = string.Empty;
        }

        [ObservableProperty] private bool _isClienteVisible;
        [ObservableProperty] private string _clienteVenta = string.Empty;

        [RelayCommand]
        private async Task ConfirmarUsoAsync() => await ExecuteAsync(async () =>
        {
            if (SacoSeleccionado is null)
            {
                await MostrarAvisoAsync("Seleccione un saco disponible primero.");
                return;
            }

            if (string.IsNullOrEmpty(MotivoSeleccionado))
            {
                await MostrarAvisoAsync("Seleccione el motivo.");
                return;
            }

            SacoSeleccionado.Estado       = false;
            SacoSeleccionado.Motivo       = MotivoSeleccionado;
            SacoSeleccionado.ClienteVenta = MotivoSeleccionado == MotivosSaco.VENTA ? ClienteVenta : null;

            await _sacosRepository.ActualizarAsync(SacoSeleccionado);

            MotivoSeleccionado = null;
            ClienteVenta       = string.Empty;
            SacoSeleccionado   = null;

            await CargarSacosAsync();
        });

        // ── Helpers privados ─────────────────────────────────────────────────

        private async Task CargarSacosAsync()
        {
            SacosDisponibles = new ObservableCollection<SacosCompost>(await _sacosRepositoryEspecifico.ObtenerDisponiblesAsync());
            SacosUsados      = new ObservableCollection<SacosCompost>(await _sacosRepositoryEspecifico.ObtenerUsadosOVendidosAsync());
        }
    }
}
