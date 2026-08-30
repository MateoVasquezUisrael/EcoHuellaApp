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

        swEstado.IsToggled =
            _biodigestorSeleccionado.Estado;
    }

    private async void btnGuardarBiodigestor_Clicked(
        object sender,
        EventArgs e)
    {
        var capacidad = double.TryParse(txtCapacidadMaxima.Text, out var valor) ? valor : 0;
        if (capacidad <= 0)
        {
            await DisplayAlert(
                "Capacidad inválida",
                "La capacidad máxima debe ser mayor que 0 kg.",
                "Aceptar");
            return;
        }

        try
        {
            await _repository.GuardarRegistroAsync(
                new Biodigestor { CapacidadMaxima = capacidad, Estado = swEstado.IsToggled });

            await CargarBiodigestores();
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo guardar el biodigestor: {ex.Message}", "Aceptar");
        }
    }

    private async void btnActualizarBiodigestor_Clicked(
        object sender,
        EventArgs e)
    {
        if (_biodigestorSeleccionado == null)
        {
            await DisplayAlert("Aviso", "Seleccione un biodigestor.", "Aceptar");
            return;
        }

        var capacidad = double.TryParse(txtCapacidadMaxima.Text, out var valor) ? valor : 0;
        if (capacidad <= 0)
        {
            await DisplayAlert(
                "Capacidad inválida",
                "La capacidad máxima debe ser mayor que 0 kg.",
                "Aceptar");
            return;
        }

        try
        {
            _biodigestorSeleccionado.CapacidadMaxima = capacidad;
            _biodigestorSeleccionado.Estado = swEstado.IsToggled;

            await _repository.ActualizarAsync(_biodigestorSeleccionado);

            await CargarBiodigestores();
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo actualizar el biodigestor: {ex.Message}", "Aceptar");
        }
    }

    private async void btnEliminarBiodigestor_Clicked(
        object sender,
        EventArgs e)
    {
        if (_biodigestorSeleccionado == null)
        {
            await DisplayAlert("Aviso", "Seleccione un biodigestor.", "Aceptar");
            return;
        }

        bool respuesta = await DisplayAlert(
            "Confirmar",
            "¿Desea eliminar el biodigestor?",
            "Sí",
            "No");

        if (!respuesta)
            return;

        try
        {
            await _repository.BorrarRegistroAsync(_biodigestorSeleccionado);

            await CargarBiodigestores();
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo eliminar el biodigestor: {ex.Message}", "Aceptar");
        }
    }

    private void LimpiarFormulario()
    {
        txtCapacidadMaxima.Text = string.Empty;
        swEstado.IsToggled = true;

        _biodigestorSeleccionado = null;
        cvBiodigestores.SelectedItem = null;
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
}
