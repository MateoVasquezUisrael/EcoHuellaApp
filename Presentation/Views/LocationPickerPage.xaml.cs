using EcoHuellaApp.Infrastructure.Services;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;

namespace EcoHuellaApp.Presentation.Views
{
    public partial class LocationPickerPage : ContentPage
    {
        private readonly OfflineMapTileService _tileService;
        private PointFeature? _selectedFeature;
        private MemoryLayer? _pinLayer;

        public double? Latitud { get; private set; }
        public double? Longitud { get; private set; }

        public LocationPickerPage(OfflineMapTileService tileService)
        {
            InitializeComponent();
            _tileService = tileService;
            _ = InitializeMapAsync();
        }

        private async Task InitializeMapAsync()
        {
            try
            {
                Mapsui.Widgets.InfoWidgets.LoggingWidget.ShowLoggingInMap = Mapsui.Widgets.ActiveMode.No;

                var map = new Mapsui.Map();
                var tileLayer = await _tileService.GetTileLayerAsync();
                map.Layers.Add(tileLayer);

                _pinLayer = new MemoryLayer
                {
                    Name = "SelectedPin",
                    Features = new List<PointFeature>()
                };
                map.Layers.Add(_pinLayer);

                mapControl.Map = map;

                // Centro inicial en Quito
                var (x, y) = SphericalMercator.FromLonLat(-78.5, -0.18);
                var center = new MPoint(x, y);

                if (map.Navigator.Resolutions.Count > 0)
                {
                    var index = Math.Min(10, map.Navigator.Resolutions.Count - 1);
                    map.Navigator.CenterOnAndZoomTo(center, map.Navigator.Resolutions[index]);
                }
                else
                {
                    map.Navigator.CenterOn(center);
                }

                mapControl.Info += OnMapTapped;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", ex.Message, "Aceptar");
            }
        }

        private void OnMapTapped(object? sender, MapInfoEventArgs e)
        {
            if (e.WorldPosition == null)
                return;

            var worldPosition = e.WorldPosition;
            var lonLat = SphericalMercator.ToLonLat(worldPosition.X, worldPosition.Y);

            Longitud = lonLat.lon;
            Latitud = lonLat.lat;

            _selectedFeature = new PointFeature(worldPosition.X, worldPosition.Y);
            _selectedFeature.Styles.Add(new SymbolStyle
            {
                SymbolType = SymbolType.Ellipse,
                Fill = new Mapsui.Styles.Brush(Mapsui.Styles.Color.Blue),
                Outline = new Pen(Mapsui.Styles.Color.White, 2),
                SymbolScale = 1.5
            });

            if (_pinLayer != null)
            {
                _pinLayer.Features = new List<PointFeature> { _selectedFeature };
                mapControl?.Map?.RefreshGraphics();
            }
        }

        private async void OnConfirmarClicked(object sender, EventArgs e)
        {
            if (Latitud == null || Longitud == null)
            {
                await DisplayAlertAsync(
                    "Aviso",
                    "Toca el mapa para seleccionar una ubicación.",
                    "Aceptar");
                return;
            }

            await Navigation.PopModalAsync();
        }
    }
}
