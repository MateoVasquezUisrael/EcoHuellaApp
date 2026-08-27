using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.ProcesoComposteraArtesanal;
using EcoHuellaApp.Infrastructure.Repositories.ProcesoComposteraArtesanal;

namespace EcoHuellaApp.Presentation.Views;

public partial class ComposterasArtesanalesView : ContentPage
{
    private readonly IRepositoryGeneric<ComposteraArtesanal> _composteraRepository;
    private readonly ComposteraArtesanalRepository _composteraRepositoryEspecifico;
    private readonly IRepositoryGeneric<AccionCompostera> _accionRepository;
    private readonly AccionComposteraRepository _accionRepositoryEspecifico;

    private ComposteraArtesanal _composteraSeleccionada;

    public ComposterasArtesanalesView(
        IRepositoryGeneric<ComposteraArtesanal> composteraRepository,
        ComposteraArtesanalRepository composteraRepositoryEspecifico,
        IRepositoryGeneric<AccionCompostera> accionRepository,
        AccionComposteraRepository accionRepositoryEspecifico)
    {
        InitializeComponent();

        _composteraRepository = composteraRepository;
        _composteraRepositoryEspecifico = composteraRepositoryEspecifico;
        _accionRepository = accionRepository;
        _accionRepositoryEspecifico = accionRepositoryEspecifico;

        dpFechaAccion.Date = DateTime.Today;

        _ = CargarComposterasAsync();
    }

    private async Task CargarComposterasAsync()
    {
        cvComposteras.ItemsSource = await _composteraRepository.ObtenerTodosAsync();
    }

    private void cvComposteras_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0)
        {
            _composteraSeleccionada = null;
            OcultarFormularioAccion();
            cvAcciones.ItemsSource = null;
            return;
        }

        _composteraSeleccionada = (ComposteraArtesanal)e.CurrentSelection.First();
        MostrarFormularioAccion();
        _ = CargarAccionesAsync();
    }

    private async Task CargarAccionesAsync()
    {
        if (_composteraSeleccionada == null)
            return;

        var acciones = await _accionRepositoryEspecifico.ObtenerPorComposteraAsync(_composteraSeleccionada.Id);
        cvAcciones.ItemsSource = acciones;
    }

    private async void btnGuardarCompostera_Clicked(
        object sender,
        EventArgs e)
    {
        try
        {
            var nuevaCompostera = new ComposteraArtesanal
            {
                PesoMaximo = double.TryParse(txtPesoMaximo.Text, out var peso) ? peso : 0,
                Estado = true
            };

            await _composteraRepository.GuardarRegistroAsync(nuevaCompostera);

            txtPesoMaximo.Text = string.Empty;
            await CargarComposterasAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[ComposterasArtesanalesView] Error al guardar compostera: {ex}");
            await DisplayAlert(
                "Error",
                "No se pudo guardar la compostera. Intenta de nuevo.",
                "Aceptar");
        }
    }

    private async void btnGuardarAccion_Clicked(
        object sender,
        EventArgs e)
    {
        try
        {
            if (_composteraSeleccionada == null)
            {
                await DisplayAlert(
                    "Aviso",
                    "Seleccione una compostera primero.",
                    "Aceptar");

                return;
            }

            if (pickerTipoAccion.SelectedIndex < 0)
            {
                await DisplayAlert(
                    "Aviso",
                    "Seleccione el tipo de acción.",
                    "Aceptar");

                return;
            }

            if (pickerTipoElemento.SelectedIndex < 0)
            {
                await DisplayAlert(
                    "Aviso",
                    "Seleccione el tipo de elemento.",
                    "Aceptar");

                return;
            }

            var nuevaAccion = new AccionCompostera
            {
                TipoAccion = pickerTipoAccion.SelectedItem?.ToString(),
                FechaAccion = dpFechaAccion.Date,
                TipoElemento = pickerTipoElemento.SelectedItem?.ToString(),
                ComposteraArtesanalId = _composteraSeleccionada.Id
            };

            await _accionRepository.GuardarRegistroAsync(nuevaAccion);

            pickerTipoAccion.SelectedIndex = -1;
            pickerTipoElemento.SelectedIndex = -1;
            dpFechaAccion.Date = DateTime.Today;

            await CargarAccionesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[ComposterasArtesanalesView] Error al guardar acción: {ex}");
            await DisplayAlert(
                "Error",
                "No se pudo guardar la acción. Intenta de nuevo.",
                "Aceptar");
        }
    }

    private void MostrarFormularioAccion()
    {
        lblAccionTitulo.IsVisible = true;
        pickerTipoAccion.IsVisible = true;
        dpFechaAccion.IsVisible = true;
        pickerTipoElemento.IsVisible = true;
        btnGuardarAccion.IsVisible = true;
        cvAcciones.IsVisible = true;
    }

    private void OcultarFormularioAccion()
    {
        lblAccionTitulo.IsVisible = false;
        pickerTipoAccion.IsVisible = false;
        dpFechaAccion.IsVisible = false;
        pickerTipoElemento.IsVisible = false;
        btnGuardarAccion.IsVisible = false;
        cvAcciones.IsVisible = false;
    }
}
