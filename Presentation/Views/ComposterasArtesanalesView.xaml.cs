using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.ProcesoComposteraArtesanal;
using EcoHuellaApp.Infrastructure.Repositories.ProcesoComposteraArtesanal;

namespace EcoHuellaApp.Presentation.Views;

public partial class ComposterasArtesanalesView : ContentPage
{
    private readonly IRepositoryGeneric<ComposteraArtesanal> _composteras;
    private readonly IRepositoryGeneric<AccionCompostera> _acciones;
    private readonly AccionComposteraRepository _accionesEspecifico;

    public ComposterasArtesanalesView(IRepositoryGeneric<ComposteraArtesanal> composteras,
        ComposteraArtesanalRepository composteraRepository, IRepositoryGeneric<AccionCompostera> acciones,
        AccionComposteraRepository accionesEspecifico)
    {
        InitializeComponent();
        _composteras = composteras;
        _acciones = acciones;
        _accionesEspecifico = accionesEspecifico;
        _ = CargarAsync();
    }

    protected override void OnAppearing() { base.OnAppearing(); _ = CargarAsync(); }
    private async Task CargarAsync() => cvComposteras.ItemsSource = await _composteras.ObtenerTodosAsync();

    private async void btnGuardarCompostera_Clicked(object sender, EventArgs e)
    {
        var peso = double.TryParse(txtPesoMaximo.Text, out var valor) ? valor : 0;
        if (peso <= 0)
        {
            await DisplayAlertAsync("Peso inválido", "El peso máximo debe ser mayor que 0 kg.", "Aceptar");
            return;
        }
        await _composteras.GuardarRegistroAsync(new ComposteraArtesanal { PesoMaximo = peso, Estado = true });
        txtPesoMaximo.Text = string.Empty;
        await CargarAsync();
    }

    private async void btnAccionesCompostera_Clicked(object sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: ComposteraArtesanal compostera })
            await Navigation.PushModalAsync(new ComposteraAccionPage(compostera, _acciones));
    }

    private async void btnHistorialAcciones_Clicked(object sender, EventArgs e) =>
        await Navigation.PushModalAsync(new HistorialAccionesComposteraPage(_accionesEspecifico));

    private void cvComposteras_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
}
