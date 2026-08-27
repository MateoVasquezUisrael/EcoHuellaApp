using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.Ventas;
using EcoHuellaApp.Infrastructure.Repositories.Ventas;
using Microsoft.Maui.Controls;

namespace EcoHuellaApp.Presentation.Views;

public partial class SacosCompostView : ContentPage
{
    private readonly IRepositoryGeneric<SacosCompost> _sacosRepository;
    private readonly SacosCompostRepository _sacosRepositoryEspecifico;

    private SacosCompost _sacoSeleccionado;

    public SacosCompostView(
        IRepositoryGeneric<SacosCompost> sacosRepository,
        SacosCompostRepository sacosRepositoryEspecifico)
    {
        InitializeComponent();

        _sacosRepository = sacosRepository;
        _sacosRepositoryEspecifico = sacosRepositoryEspecifico;

        dpFechaRegistro.Date = DateTime.Today;

        _ = CargarSacosAsync();
    }

    private async Task CargarSacosAsync()
    {
        cvSacosDisponibles.ItemsSource = await _sacosRepositoryEspecifico.ObtenerDisponiblesAsync();
        cvSacosUsados.ItemsSource = await _sacosRepositoryEspecifico.ObtenerUsadosOVendidosAsync();
    }

    private void cvSacosDisponibles_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0)
        {
            _sacoSeleccionado = null;
            OcultarFormularioUso();
            return;
        }

        _sacoSeleccionado = (SacosCompost)e.CurrentSelection.First();
        MostrarFormularioUso();
    }

    private async void btnGuardarSaco_Clicked(
        object sender,
        EventArgs e)
    {
        try
        {
            var nuevoSaco = new SacosCompost
            {
                Fecha = dpFechaRegistro?.Date,
                Estado = true,
                Motivo = null,
                ClienteVenta = null
            };

            await _sacosRepository.GuardarRegistroAsync(nuevoSaco);

            dpFechaRegistro.Date = DateTime.Today;
            await CargarSacosAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[SacosCompostView] Error al guardar saco: {ex}");
            await DisplayAlert(
                "Error",
                "No se pudo guardar el saco. Intenta de nuevo.",
                "Aceptar");
        }
    }

    private void pickerMotivo_SelectedIndexChanged(
        object sender,
        EventArgs e)
    {
        var motivo = pickerMotivo.SelectedItem?.ToString();
        txtClienteVenta.IsVisible = motivo == "Venta";

        if (motivo != "Venta")
            txtClienteVenta.Text = string.Empty;
    }

    private async void btnConfirmarUso_Clicked(
        object sender,
        EventArgs e)
    {
        try
        {
            if (_sacoSeleccionado == null)
            {
                await DisplayAlert(
                    "Aviso",
                    "Seleccione un saco disponible primero.",
                    "Aceptar");

                return;
            }

            if (pickerMotivo.SelectedIndex < 0)
            {
                await DisplayAlert(
                    "Aviso",
                    "Seleccione el motivo.",
                    "Aceptar");

                return;
            }

            var motivo = pickerMotivo.SelectedItem?.ToString();
            var cliente = motivo == "Venta" ? txtClienteVenta.Text : null;

            _sacoSeleccionado.Estado = false;
            _sacoSeleccionado.Motivo = motivo;
            _sacoSeleccionado.ClienteVenta = cliente;

            await _sacosRepository.ActualizarAsync(_sacoSeleccionado);

            pickerMotivo.SelectedIndex = -1;
            txtClienteVenta.Text = string.Empty;
            txtClienteVenta.IsVisible = false;
            _sacoSeleccionado = null;
            cvSacosDisponibles.SelectedItem = null;
            OcultarFormularioUso();

            await CargarSacosAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[SacosCompostView] Error al confirmar uso: {ex}");
            await DisplayAlert(
                "Error",
                "No se pudo confirmar el uso/venta. Intenta de nuevo.",
                "Aceptar");
        }
    }

    private void MostrarFormularioUso()
    {
        lblUsoTitulo.IsVisible = true;
        pickerMotivo.IsVisible = true;
        txtClienteVenta.IsVisible = false;
        btnConfirmarUso.IsVisible = true;
    }

    private void OcultarFormularioUso()
    {
        lblUsoTitulo.IsVisible = false;
        pickerMotivo.IsVisible = false;
        txtClienteVenta.IsVisible = false;
        btnConfirmarUso.IsVisible = false;
    }
}
