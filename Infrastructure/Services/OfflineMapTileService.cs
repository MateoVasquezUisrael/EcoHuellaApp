using BruTile.MbTiles;
using Mapsui.Layers;
using Mapsui.Tiling.Layers;
using SQLite;

namespace EcoHuellaApp.Infrastructure.Services
{
    public class OfflineMapTileService
    {
        private TileLayer? _tileLayer;

        public async Task<TileLayer> GetTileLayerAsync()
        {
            if (_tileLayer != null)
                return _tileLayer;

            string targetPath = Path.Combine(
                FileSystem.AppDataDirectory,
                "offline_map.mbtiles");

            await CopyAssetIfNeededAsync(targetPath);

            var connectionString = new SQLiteConnectionString(targetPath, true);
            var tileSource = new MbTilesTileSource(connectionString);
            _tileLayer = new TileLayer(tileSource)
            {
                Name = "OfflineMap"
            };

            return _tileLayer;
        }

        private async Task CopyAssetIfNeededAsync(string targetPath)
        {
            if (File.Exists(targetPath))
                return;

            using var sourceStream = await FileSystem
                .OpenAppPackageFileAsync("offline_map.mbtiles");

            using var targetStream = File.Create(targetPath);

            await sourceStream.CopyToAsync(targetStream);
        }
    }
}
