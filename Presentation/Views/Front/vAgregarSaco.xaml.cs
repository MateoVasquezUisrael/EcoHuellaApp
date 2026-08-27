namespace EcoHuellaApp.Presentation.Views.Front;

using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.Ventas;
using Microsoft.Extensions.DependencyInjection;

public partial class vAgregarSaco : ContentPage
{
    private readonly IRepositoryGeneric<SacosCompost>? _repository;

    public vAgregarSaco()
    {
        InitializeComponent();
        _repository = Application.Current?.Handler?.MauiContext?.Services.GetService<IRepositoryGeneric<SacosCompost>>();
        dpFechaRegistro.Date = DateTime.Today;
    }

    private async void GuardarSaco_Clicked(object? sender, EventArgs e)
    {
        if (_repository is null)
        {
            await DisplayAlert("Error", "No se pudo acceder al registro de sacos.", "Aceptar");
            return;
        }

        await _repository.GuardarRegistroAsync(new SacosCompost
        {
            Fecha = dpFechaRegistro.Date.GetValueOrDefault(DateTime.Today),
            Estado = true,
            Motivo = null,
            ClienteVenta = null
        });

        await DisplayAlert("Listo", "Saco registrado correctamente.", "Aceptar");
        await Navigation.PopAsync();
    }
}
