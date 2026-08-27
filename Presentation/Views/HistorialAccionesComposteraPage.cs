using EcoHuellaApp.Infrastructure.Repositories.ProcesoComposteraArtesanal;

namespace EcoHuellaApp.Presentation.Views;

public sealed class HistorialAccionesComposteraPage : ContentPage
{
    private readonly AccionComposteraRepository _repository;
    private readonly CollectionView _lista;

    public HistorialAccionesComposteraPage(AccionComposteraRepository repository)
    {
        _repository = repository;
        BackgroundColor = Color.FromArgb("#F4F8F6");

        _lista = new CollectionView
        {
            SelectionMode = SelectionMode.None,
            EmptyView = new Label
            {
                Text = "No hay acciones de composteras registradas.",
                TextColor = Colors.DimGray,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 40)
            },
            ItemTemplate = new DataTemplate(CrearTarjeta)
        };

        var cerrar = new Button { Text = "Cerrar" };
        cerrar.Clicked += async (_, _) => await Navigation.PopModalAsync();

        var contenido = new Grid
        {
            Padding = 20,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            }
        };
        contenido.Add(new Label { Text = "Historial de acciones", FontSize = 26, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#075E54") });
        contenido.Add(new Label { Text = "Todas las acciones realizadas en composteras artesanales", TextColor = Colors.DimGray, Margin = new Thickness(0, 6, 0, 12) }, 0, 1);
        contenido.Add(_lista, 0, 2);
        contenido.Add(cerrar, 0, 3);
        Content = contenido;

        Loaded += async (_, _) => await CargarAsync();
    }

    private async Task CargarAsync() => _lista.ItemsSource = await _repository.ObtenerTodosAsync();

    private static View CrearTarjeta()
    {
        var titulo = new Label { FontAttributes = FontAttributes.Bold, FontSize = 17, TextColor = Color.FromArgb("#102A24") };
        titulo.SetBinding(Label.TextProperty, new Binding("ComposteraArtesanalId", stringFormat: "Compostera #{0}"));
        var accion = new Label();
        accion.SetBinding(Label.TextProperty, new Binding("TipoAccion", stringFormat: "Acción: {0}"));
        var elemento = new Label { TextColor = Colors.DimGray };
        elemento.SetBinding(Label.TextProperty, new Binding("TipoElemento", stringFormat: "Elemento: {0}"));
        var fecha = new Label { TextColor = Colors.DimGray };
        fecha.SetBinding(Label.TextProperty, new Binding("FechaAccion", stringFormat: "Fecha: {0:dd/MM/yyyy}"));
        return new Border
        {
            Margin = new Thickness(0, 6),
            Padding = 14,
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#B9E8D1"),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
            Content = new VerticalStackLayout { Spacing = 5, Children = { titulo, accion, elemento, fecha } }
        };
    }
}
