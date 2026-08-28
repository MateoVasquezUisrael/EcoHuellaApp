namespace EcoHuellaApp.Presentation.Views.Front;

using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.Recoleccion;
using EcoHuellaApp.Presentation.Services;
using EcoHuellaApp.Presentation.Views;
using Microsoft.Extensions.DependencyInjection;

public partial class vGestionResiduos : ContentPage
{
    private readonly IRepositoryGeneric<Recoleccion>? _repository;

    public vGestionResiduos()
    {
        InitializeComponent();
        NavegacionInferior.Conectar(this);
        _repository = Application.Current?.Handler?.MauiContext?.Services.GetService<IRepositoryGeneric<Recoleccion>>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = CargarHistorialAsync();
    }

    private async Task CargarHistorialAsync()
    {
        cvHistorialEntregas.ItemsSource = _repository is null
            ? []
            : await _repository.ObtenerTodosAsync();
    }

    private async void NuevaEntrega_Clicked(object? sender, EventArgs e)
    {
        await BackendNavigation.PushAsync<RecoleccionView>(this);
    }

    private async void Casas_Clicked(object? sender, EventArgs e)
    {
        await BackendNavigation.PushAsync<CasaView>(this);
    }

    private async void Puntos_Clicked(object? sender, EventArgs e)
    {
        await BackendNavigation.PushAsync<PuntoRecoleccionView>(this);
    }

}
