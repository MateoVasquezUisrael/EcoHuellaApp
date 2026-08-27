using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.Ventas;
using EcoHuellaApp.Infrastructure.Repositories.Ventas;

namespace EcoHuellaApp.Presentation.Views;

public partial class SacosCompostView : ContentPage
{
    private readonly IRepositoryGeneric<SacosCompost> _repository;
    private readonly SacosCompostRepository _repositoryEspecifico;

    public SacosCompostView(IRepositoryGeneric<SacosCompost> repository, SacosCompostRepository repositoryEspecifico)
    {
        InitializeComponent();
        _repository = repository;
        _repositoryEspecifico = repositoryEspecifico;
        dpFechaRegistro.Date = DateTime.Today;
        _ = CargarSacosAsync();
    }

    protected override void OnAppearing() { base.OnAppearing(); _ = CargarSacosAsync(); }
    private async Task CargarSacosAsync() => cvSacosDisponibles.ItemsSource = await _repositoryEspecifico.ObtenerDisponiblesAsync();

    private async void btnGuardarSaco_Clicked(object sender, EventArgs e)
    {
        await _repository.GuardarRegistroAsync(new SacosCompost { Fecha = dpFechaRegistro.Date, Estado = true });
        dpFechaRegistro.Date = DateTime.Today;
        await CargarSacosAsync();
    }

    private async void btnAccionesSaco_Clicked(object sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: SacosCompost saco })
            await Navigation.PushModalAsync(new SacoAccionPage(saco, _repository));
    }

    private async void btnHistorialSacos_Clicked(object sender, EventArgs e) =>
        await Navigation.PushModalAsync(new HistorialSacosUsadosPage(_repositoryEspecifico));
}
