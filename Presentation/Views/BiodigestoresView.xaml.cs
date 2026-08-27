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
