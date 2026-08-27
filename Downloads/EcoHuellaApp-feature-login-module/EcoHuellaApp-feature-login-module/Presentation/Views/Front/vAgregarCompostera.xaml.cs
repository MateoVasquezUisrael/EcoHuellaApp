namespace EcoHuellaApp.Presentation.Views.Front;

using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.ProcesoComposteraArtesanal;
using Microsoft.Extensions.DependencyInjection;

public partial class vAgregarCompostera : ContentPage
{
    private readonly IRepositoryGeneric<ComposteraArtesanal>? _repository;

    public vAgregarCompostera()
    {
        InitializeComponent();
        _repository = Application.Current?.Handler?.MauiContext?.Services.GetService<IRepositoryGeneric<ComposteraArtesanal>>();
    }

    private async void GuardarCompostera_Clicked(object? sender, EventArgs e)
    {
        if (_repository is null)
        {
            await DisplayAlert("Error", "No se pudo acceder al registro de composteras.", "Aceptar");
            return;
        }

        await _repository.GuardarRegistroAsync(new ComposteraArtesanal
        {
            PesoMaximo = double.TryParse(txtPesoMaximo.Text, out var peso) ? peso : 0,
            Estado = true
        });

        await DisplayAlert("Listo", "Compostera registrada correctamente.", "Aceptar");
        await Navigation.PopAsync();
    }
}
