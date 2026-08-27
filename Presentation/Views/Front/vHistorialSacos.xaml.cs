namespace EcoHuellaApp.Presentation.Views.Front;

using System.Collections.ObjectModel;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.Ventas;
using Microsoft.Extensions.DependencyInjection;

public partial class vHistorialSacos : ContentPage
{
    private readonly IRepositoryGeneric<SacosCompost>? _repository;

    public ObservableCollection<SacosCompost> Sacos { get; } = [];
    public string Resumen { get; private set; } = "Datos reales registrados";

    public vHistorialSacos()
    {
        InitializeComponent();
        BindingContext = this;
        _repository = Application.Current?.Handler?.MauiContext?.Services.GetService<IRepositoryGeneric<SacosCompost>>();
        _ = CargarAsync();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = CargarAsync();
    }

    private async Task CargarAsync()
    {
        Sacos.Clear();
        var datos = _repository is null ? [] : await _repository.ObtenerTodosAsync();
        foreach (var saco in datos)
            Sacos.Add(saco);
        Resumen = $"{Sacos.Count} sacos registrados";
        OnPropertyChanged(nameof(Resumen));
    }
}
