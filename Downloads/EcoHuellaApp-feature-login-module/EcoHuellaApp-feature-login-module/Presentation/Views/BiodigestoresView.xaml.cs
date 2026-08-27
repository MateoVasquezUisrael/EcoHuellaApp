using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.ProcesoDegradacion;

namespace EcoHuellaApp.Presentation.Views;

public partial class BiodigestoresView : ContentPage
{
    private readonly IRepositoryGeneric<Biodigestor> _repository;
    private Biodigestor _biodigestorSeleccionado;

    public BiodigestoresView(IRepositoryGeneric<Biodigestor> repository)
    {
        InitializeComponent();
        _repository = repository;

        _ = CargarBiodigestores();
    }

    private async Task CargarBiodigestores()
    {
        cvBiodigestores.ItemsSource =
            await _repository.ObtenerTodosAsync();
    }

    private void cvBiodigestores_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0)
            return;

        _biodigestorSeleccionado =
            (Biodigestor)e.CurrentSelection.First();

        txtCapacidadMaxima.Text =
            _biodigestorSeleccionado.CapacidadMaxima.ToString();
    }

    private async void btnGuardar_Clicked(
        object sender,
        EventArgs e)
    {
        if (_biodigestorSeleccionado != null)
        {
            await ActualizarBiodigestorAsync();
            return;
        }

        var nuevoBiodigestor = new Biodigestor
        {
            CapacidadMaxima = double.TryParse(
                txtCapacidadMaxima.Text, out var cap) ? cap : 0,
            Estado = true
        };

        await _repository.GuardarRegistroAsync(nuevoBiodigestor);

        await CargarBiodigestores();
        LimpiarFormulario();
    }

    private async void btnActualizar_Clicked(
        object sender,
        EventArgs e)
    {
        await ActualizarBiodigestorAsync();
    }

    private async Task ActualizarBiodigestorAsync()
    {
        if (_biodigestorSeleccionado == null)
        {
            await DisplayAlert(
                "Aviso",
                "Seleccione un biodigestor.",
                "Aceptar");

            return;
        }

        _biodigestorSeleccionado.CapacidadMaxima =
            double.TryParse(txtCapacidadMaxima.Text, out var cap) ? cap : 0;

        await _repository.ActualizarAsync(_biodigestorSeleccionado);

        await CargarBiodigestores();
        LimpiarFormulario();
    }

    private async void btnEliminar_Clicked(
        object sender,
        EventArgs e)
    {
        if (_biodigestorSeleccionado == null)
        {
            await DisplayAlert(
                "Aviso",
                "Seleccione un biodigestor.",
                "Aceptar");

            return;
        }

        bool respuesta = await DisplayAlert(
            "Confirmar",
            "¿Desea eliminar el biodigestor?",
            "Sí",
            "No");

        if (!respuesta)
            return;

        await _repository.BorrarRegistroAsync(_biodigestorSeleccionado);

        await CargarBiodigestores();
        LimpiarFormulario();
    }

    private async void btnVerProcesos_Clicked(
        object sender,
        EventArgs e)
    {
        if (_biodigestorSeleccionado == null)
        {
            await DisplayAlert(
                "Aviso",
                "Seleccione un biodigestor.",
                "Aceptar");

            return;
        }

        var procesosView = new ProcesosBiodigestorView(
            _biodigestorSeleccionado.Id);

        await Navigation.PushAsync(procesosView);
    }

    private async void btnVerFinalizados_Clicked(
        object sender,
        EventArgs e)
    {
        if (_biodigestorSeleccionado == null)
        {
            await DisplayAlert(
                "Aviso",
                "Seleccione un biodigestor.",
                "Aceptar");

            return;
        }

        var finalizadosView = new ProcesosFinalizadosView(
            _biodigestorSeleccionado.Id);

        await Navigation.PushAsync(finalizadosView);
    }

    private void LimpiarFormulario()
    {
        txtCapacidadMaxima.Text = string.Empty;
        _biodigestorSeleccionado = null;
        cvBiodigestores.SelectedItem = null;
    }
}
