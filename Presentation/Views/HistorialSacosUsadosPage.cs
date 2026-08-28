using EcoHuellaApp.Infrastructure.Repositories.Ventas;

namespace EcoHuellaApp.Presentation.Views;

public sealed class HistorialSacosUsadosPage : ContentPage
{
    public HistorialSacosUsadosPage(SacosCompostRepository repository)
    {
        Title = "Historial de sacos usados";
        BackgroundColor = Color.FromArgb("#F4F8F6");
        var lista = new CollectionView
        {
            ItemTemplate = new DataTemplate(() =>
            {
                var titulo = new Label { FontAttributes = FontAttributes.Bold, FontSize = 17 };
                titulo.SetBinding(Label.TextProperty, "Id", stringFormat: "Saco #{0}");
                var motivo = new Label { TextColor = Colors.DimGray }; motivo.SetBinding(Label.TextProperty, "Motivo", stringFormat: "Estado: {0}");
                var cliente = new Label { TextColor = Colors.DimGray }; cliente.SetBinding(Label.TextProperty, "ClienteVenta", stringFormat: "Cliente: {0}");
                return new Border { Margin = new Thickness(0, 6), Padding = 14, BackgroundColor = Colors.White, Content = new VerticalStackLayout { Children = { titulo, motivo, cliente } } };
            })
        };
        var cerrar = new Button { Text = "Cerrar" }; cerrar.Clicked += async (_, _) => await Navigation.PopModalAsync();
        Content = new Grid { Padding = 20, RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) }, Children = { new Label { Text = "Sacos usados o vendidos", FontSize = 26, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#075E54") }, lista, cerrar } };
        Grid.SetRow(lista, 1); Grid.SetRow(cerrar, 2);
        Loaded += async (_, _) => lista.ItemsSource = await repository.ObtenerUsadosOVendidosAsync();
    }
}
