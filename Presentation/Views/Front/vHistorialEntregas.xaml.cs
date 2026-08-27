namespace EcoHuellaApp.Presentation.Views.Front;

using System.Collections.ObjectModel;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.Recoleccion;
using EcoHuellaApp.Presentation.Services;
using EcoHuellaApp.Presentation.Views;
using Microsoft.Extensions.DependencyInjection;

public partial class vHistorialEntregas : ContentPage
{
    private readonly IRepositoryGeneric<Recoleccion>? _repository;

    public ObservableCollection<Recoleccion> Recolecciones { get; } = [];
    public string TotalRegistrosTexto { get; private set; } = "0 registros";

    public vHistorialEntregas()
    {
        InitializeComponent();
        BindingContext = this;
        NavegacionInferior.Conectar(this);

        _repository = Application.Current?.Handler?.MauiContext?.Services.GetService<IRepositoryGeneric<Recoleccion>>();
        _ = CargarRecoleccionesAsync();
    }

    private async Task CargarRecoleccionesAsync()
    {
        Recolecciones.Clear();

        var datos = _repository is null ? [] : await _repository.ObtenerTodosAsync();
        foreach (var recoleccion in datos)
            Recolecciones.Add(recoleccion);

        TotalRegistrosTexto = $"{Recolecciones.Count} registros";
        OnPropertyChanged(nameof(TotalRegistrosTexto));
    }

    private async void Detalle_Clicked(object? sender, EventArgs e)
    {
        await BackendNavigation.PushAsync<RecoleccionView>(this);
    }

    private async void Editar_Clicked(object? sender, EventArgs e)
    {
        await BackendNavigation.PushAsync<RecoleccionView>(this);
    }

    private async void Eliminar_Clicked(object? sender, EventArgs e)
    {
        await BackendNavigation.PushAsync<RecoleccionView>(this);
    }
}
