using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.Recoleccion;
using EcoHuellaApp.Infrastructure.Services;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Microsoft.Maui.Controls;

namespace EcoHuellaApp.Presentation.Views
{
    public partial class MapPage : ContentPage
    {
        private readonly OfflineMapTileService _tileService;
        private readonly IRepositoryGeneric<Casa> _casaRepository;
        private readonly IRepositoryGeneric<PuntoRecoleccion> _puntoRepository;

        public MapPage(
            OfflineMapTileService tileService,
            IRepositoryGeneric<Casa> casaRepository,
            IRepositoryGeneric<PuntoRecoleccion> puntoRepository)
        {
            InitializeComponent();
            _tileService = tileService;
            _casaRepository = casaRepository;
            _puntoRepository = puntoRepository;

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

                var casas = await _casaRepository.ObtenerTodosAsync();
                var casaLayer = CreateCasaLayer(casas);
                map.Layers.Add(casaLayer);

                var puntos = await _puntoRepository.ObtenerTodosAsync();
                var puntoLayer = CreatePuntoRecoleccionLayer(puntos);
                map.Layers.Add(puntoLayer);

                mapControl.Map = map;

                // Centro de Quito: longitud -78.5, latitud -0.18
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
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", ex.Message, "Aceptar");
            }
        }

        private static MemoryLayer CreateCasaLayer(List<Casa> casas)
        {
            var features = casas
                .Where(c => c.Latitud != 0 && c.Longitud != 0)
                .Select(c =>
                {
                    var (x, y) = SphericalMercator.FromLonLat(c.Longitud, c.Latitud);
                    var feature = new PointFeature(x, y);
                    feature["Nombre"] = c.NombreResponsable;
                    feature["Direccion"] = c.Direccion;
                    feature.Styles.Add(new SymbolStyle
                    {
                        SymbolType = SymbolType.Ellipse,
                        Fill = new Mapsui.Styles.Brush(Mapsui.Styles.Color.Red),
                        Outline = new Pen(Mapsui.Styles.Color.Black, 2),
                        SymbolScale = 1.2
                    });
                    return feature;
                })
                .ToList();

            return new MemoryLayer
            {
                Name = "Casas",
                Features = features
            };
        }

        private static MemoryLayer CreatePuntoRecoleccionLayer(List<PuntoRecoleccion> puntos)
        {
            var features = puntos
                .Where(p => p.Latitud != 0 && p.Longitud != 0)
                .Select(p =>
                {
                    var (x, y) = SphericalMercator.FromLonLat(p.Longitud, p.Latitud);
                    var feature = new PointFeature(x, y);
                    feature["Direccion"] = p.Direccion;
                    feature.Styles.Add(new SymbolStyle
                    {
                        SymbolType = SymbolType.Ellipse,
                        Fill = new Mapsui.Styles.Brush(Mapsui.Styles.Color.Blue),
                        Outline = new Pen(Mapsui.Styles.Color.Black, 2),
                        SymbolScale = 1.2
                    });
                    return feature;
                })
                .ToList();

            return new MemoryLayer
            {
                Name = "Puntos de Recolección",
                Features = features
            };
        }
    }
}
