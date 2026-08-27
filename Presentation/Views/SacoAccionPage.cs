using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.Ventas;

namespace EcoHuellaApp.Presentation.Views;

public sealed class SacoAccionPage : ContentPage
{
    public SacoAccionPage(SacosCompost saco, IRepositoryGeneric<SacosCompost> repository)
    {
        BackgroundColor = Color.FromArgb("#F4F8F6");
        var motivo = new Picker { Title = "Seleccionar uso", ItemsSource = new[] { "Consumo", "Venta" } };
        var cliente = new Entry { Placeholder = "Motivo o cliente de la venta", IsVisible = false };
        motivo.SelectedIndexChanged += (_, _) =>
        {
            motivo.Title = motivo.SelectedIndex >= 0 ? string.Empty : "Seleccionar uso";
            cliente.IsVisible = motivo.SelectedItem?.ToString() == "Venta";
            motivo.InvalidateMeasure();
        };

        var guardar = new Button { Text = "Guardar acción", BackgroundColor = Color.FromArgb("#2FC477"), TextColor = Colors.White };
        guardar.Clicked += async (_, _) =>
        {
            if (motivo.SelectedItem is null) { await DisplayAlertAsync("Aviso", "Selecciona si el saco fue usado o vendido.", "Aceptar"); return; }
            saco.Estado = false;
            saco.Motivo = motivo.SelectedItem.ToString();
            saco.ClienteVenta = saco.Motivo == "Venta" ? cliente.Text : null;
            await repository.ActualizarAsync(saco);
            await Navigation.PopModalAsync();
        };

        var cerrar = new Button { Text = "Cancelar", BackgroundColor = Color.FromArgb("#E4F4ED"), TextColor = Color.FromArgb("#075E54") };
        cerrar.Clicked += async (_, _) => await Navigation.PopModalAsync();
        Content = new Grid
        {
            Padding = 24,
            Children =
            {
                new Border
                {
                    BackgroundColor = Colors.White, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 24 },
                    Padding = 24, VerticalOptions = LayoutOptions.Center,
                    Content = new VerticalStackLayout
                    {
                        Spacing = 14,
                        Children = { new Label { Text = $"Acciones del saco #{saco.Id}", FontSize = 24, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#075E54") }, motivo, cliente, guardar, cerrar }
                    }
                }
            }
        };
    }
}
