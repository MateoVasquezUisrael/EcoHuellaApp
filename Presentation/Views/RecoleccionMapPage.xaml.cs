using EcoHuellaApp.Domain.Models.Recoleccion;
using EcoHuellaApp.Infrastructure.Services;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;

namespace EcoHuellaApp.Presentation.Views
{
    public partial class RecoleccionMapPage : ContentPage
    {
        private readonly OfflineMapTileService _tileService;
        private readonly Casa _casa;
        private readonly PuntoRecoleccion _punto;

        public RecoleccionMapPage(
            OfflineMapTileService tileService,
            Casa casa,
            PuntoRecoleccion punto)
        {
            InitializeComponent();
            _tileService = tileService;
            _casa = casa;
            _punto = punto;

            _ = InitializeMapAsync();
        }

        private async Task InitializeMapAsync()
        {
            try
            {
                var map = new Mapsui.Map();
                var tileLayer = await _tileService.GetTileLayerAsync();
                map.Layers.Add(tileLayer);

                var casaFeature = CreateFeature(
                    _casa.Longitud,
                    _casa.Latitud,
                    Mapsui.Styles.Color.Red,
                    "Casa");

                var puntoFeature = CreateFeature(
                    _punto.Longitud,
                    _punto.Latitud,
                    Mapsui.Styles.Color.Blue,
                    "Punto de Recolección");

                var recoleccionLayer = new MemoryLayer
                {
                    Name = "Recolección",
                    Features = new List<PointFeature> { casaFeature, puntoFeature }
                };

                map.Layers.Add(recoleccionLayer);
                mapControl.Map = map;

                var (casaX, casaY) = SphericalMercator.FromLonLat(_casa.Longitud, _casa.Latitud);
                var (puntoX, puntoY) = SphericalMercator.FromLonLat(_punto.Longitud, _punto.Latitud);

                var minX = Math.Min(casaX, puntoX);
                var minY = Math.Min(casaY, puntoY);
                var maxX = Math.Max(casaX, puntoX);
                var maxY = Math.Max(casaY, puntoY);

                const double padding = 1000;
                var bbox = new MRect(
                    minX - padding,
                    minY - padding,
                    maxX + padding,
                    maxY + padding);

                map.Navigator.ZoomToBox(bbox, Mapsui.MBoxFit.Fit);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", ex.Message, "Aceptar");
            }
        }

        private static PointFeature CreateFeature(
            double longitud,
            double latitud,
            Mapsui.Styles.Color color,
            string label)
        {
            var (x, y) = SphericalMercator.FromLonLat(longitud, latitud);
            var feature = new PointFeature(x, y);
            feature["Nombre"] = label;
            feature.Styles.Add(new SymbolStyle
            {
                SymbolType = SymbolType.Ellipse,
                Fill = new Mapsui.Styles.Brush(color),
                Outline = new Pen(Mapsui.Styles.Color.Black, 2),
                SymbolScale = 1.5
            });
            return feature;
        }

        private async void OnCerrarClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
