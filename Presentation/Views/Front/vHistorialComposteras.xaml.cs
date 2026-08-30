namespace EcoHuellaApp.Presentation.Views.Front;

using System.Collections.ObjectModel;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.ProcesoComposteraArtesanal;
using EcoHuellaApp.Infrastructure.Repositories.ProcesoComposteraArtesanal;
using Microsoft.Extensions.DependencyInjection;

public partial class vHistorialComposteras : ContentPage
{
    private readonly ComposteraArtesanalRepository? _repository;

    public ObservableCollection<ComposteraArtesanal> Composteras { get; } = [];
    public string Resumen { get; private set; } = "Datos reales registrados";

    public vHistorialComposteras()
    {
        InitializeComponent();
        BindingContext = this;
        _repository = Application.Current?.Handler?.MauiContext?.Services.GetService<ComposteraArtesanalRepository>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = CargarAsync();
    }

    private async Task CargarAsync()
    {
        Composteras.Clear();
        var datos = _repository is null ? [] : await _repository.ObtenerHistorialAsync();
        foreach (var compostera in datos)
            Composteras.Add(compostera);
        Resumen = $"{Composteras.Count} composteras registradas";
        OnPropertyChanged(nameof(Resumen));
    }
}
