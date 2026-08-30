using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.ProcesoComposteraArtesanal;

namespace EcoHuellaApp.Presentation.Views;

public sealed class ComposteraAccionPage : ContentPage
{
    public ComposteraAccionPage(ComposteraArtesanal compostera, IRepositoryGeneric<AccionCompostera> repository)
    {
        BackgroundColor = Color.FromArgb("#F4F8F6");
        var accion = new Picker { Title = "Seleccionar acción", ItemsSource = new[] { "Insertar", "Extraer" } };
        var elemento = new Picker { Title = "Seleccionar elemento", ItemsSource = new[] { "Lixiviado", "Compost", "Forraje Verde" } };
        accion.SelectedIndexChanged += (_, _) =>
        {
            accion.Title = accion.SelectedIndex >= 0 ? string.Empty : "Seleccionar acción";
            accion.InvalidateMeasure();
        };
        elemento.SelectedIndexChanged += (_, _) =>
        {
            elemento.Title = elemento.SelectedIndex >= 0 ? string.Empty : "Seleccionar elemento";
            elemento.InvalidateMeasure();
        };
        var fecha = new DatePicker { Date = DateTime.Today, Format = "dd/MM/yyyy" };
        var guardar = new Button { Text = "Guardar acción", BackgroundColor = Color.FromArgb("#2FC477"), TextColor = Colors.White };
        guardar.Clicked += async (_, _) =>
        {
            if (accion.SelectedItem is null || elemento.SelectedItem is null) { await DisplayAlertAsync("Aviso", "Completa el tipo de acción y el elemento.", "Aceptar"); return; }
            try
            {
                await repository.GuardarRegistroAsync(new AccionCompostera { ComposteraArtesanalId = compostera.Id, TipoAccion = accion.SelectedItem.ToString(), TipoElemento = elemento.SelectedItem.ToString(), FechaAccion = fecha.Date });
                await Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"No se pudo guardar la acción: {ex.Message}", "Aceptar");
            }
        };
        var cerrar = new Button { Text = "Cancelar", BackgroundColor = Color.FromArgb("#E4F4ED"), TextColor = Color.FromArgb("#075E54") };
        cerrar.Clicked += async (_, _) => await Navigation.PopModalAsync();
        Content = new Grid { Padding = 24, Children = { new Border { BackgroundColor = Colors.White, Padding = 24, VerticalOptions = LayoutOptions.Center, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 24 }, Content = new VerticalStackLayout { Spacing = 14, Children = { new Label { Text = $"Acciones de compostera #{compostera.Id}", FontSize = 23, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#075E54") }, accion, elemento, fecha, guardar, cerrar } } } } };
    }
}
