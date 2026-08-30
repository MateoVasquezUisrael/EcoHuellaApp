using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.Recoleccion;
using Microsoft.Extensions.DependencyInjection;

namespace EcoHuellaApp.Presentation.Views;

public sealed class EntregaHistorialPage : ContentPage
{
    public EntregaHistorialPage(Recoleccion entrega)
    {
        BackgroundColor = Color.FromArgb("#F4F8F6");
        var fecha = new DatePicker { Date = entrega.Fecha ?? DateTime.Today, Format = "dd/MM/yyyy" };
        var cubetas = new Entry { Text = entrega.CantidadCubetas.ToString(), Keyboard = Keyboard.Numeric, Placeholder = "Cantidad de cubetas" };
        var guardar = new Button { Text = "Guardar cambios", BackgroundColor = Color.FromArgb("#2FC477"), TextColor = Colors.White };
        var eliminar = new Button { Text = "Eliminar entrega", BackgroundColor = Color.FromArgb("#FFF0F0"), TextColor = Colors.Red };
        var cerrar = new Button { Text = "Cerrar" };
        var repository = Application.Current?.Handler?.MauiContext?.Services.GetService<IRepositoryGeneric<Recoleccion>>();

        guardar.Clicked += async (_, _) =>
        {
            if (repository is null || !int.TryParse(cubetas.Text, out var cantidad) || cantidad <= 0) { await DisplayAlertAsync("Aviso", "Ingresa una cantidad válida.", "Aceptar"); return; }
            try
            {
                entrega.Fecha = fecha.Date;
                entrega.CantidadCubetas = cantidad;
                entrega.LitrosEstimados = cantidad * 20;
                entrega.MasaEstimada = entrega.LitrosEstimados * 0.6;
                await repository.ActualizarAsync(entrega);
                await Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"No se pudo actualizar la entrega: {ex.Message}", "Aceptar");
            }
        };
        eliminar.Clicked += async (_, _) =>
        {
            if (repository is null || !await DisplayAlertAsync("Eliminar entrega", "¿Deseas eliminar este registro?", "Eliminar", "Cancelar")) return;
            try
            {
                await repository.BorrarRegistroAsync(entrega);
                await Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"No se pudo eliminar la entrega: {ex.Message}", "Aceptar");
            }
        };
        cerrar.Clicked += async (_, _) => await Navigation.PopModalAsync();

        Content = new Grid { Padding = 24, Children = { new Border { Padding = 24, BackgroundColor = Colors.White, VerticalOptions = LayoutOptions.Center, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 24 }, Content = new VerticalStackLayout { Spacing = 14, Children = { new Label { Text = $"Entrega #{entrega.Id}", FontSize = 25, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#075E54") }, new Label { Text = $"Casa: {entrega.Casa?.Direccion ?? entrega.CasaId.ToString()}" }, new Label { Text = $"Punto: {entrega.PuntoRecoleccion?.Direccion ?? entrega.PuntoRecoleccionId.ToString()}" }, fecha, cubetas, guardar, eliminar, cerrar } } } } };
    }
}
