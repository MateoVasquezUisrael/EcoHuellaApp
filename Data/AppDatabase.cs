using EcoHuellaApp.Domain.Models.ProcesoComposteraArtesanal;
using EcoHuellaApp.Domain.Models.ProcesoDegradacion;
using EcoHuellaApp.Domain.Models.Recoleccion;
using EcoHuellaApp.Domain.Models.Ventas;
using SQLite;

namespace EcoHuellaApp.Data
{
    public class AppDatabase
    {
        private readonly SQLiteAsyncConnection _connection;

        public AppDatabase(string dbPath)
        {
            try
            {
                _connection = new SQLiteAsyncConnection(dbPath);

                Task.WhenAll(
                    _connection.CreateTableAsync<Casa>(),
                    _connection.CreateTableAsync<PuntoRecoleccion>(),
                    _connection.CreateTableAsync<Recoleccion>(),
                    _connection.CreateTableAsync<Biodigestor>(),
                    _connection.CreateTableAsync<ProcesoBiodigestor>(),
                    _connection.CreateTableAsync<EntradasProcesoBiodigestor>(),
                    _connection.CreateTableAsync<ComposteraArtesanal>(),
                    _connection.CreateTableAsync<AccionCompostera>(),
                    _connection.CreateTableAsync<SacosCompost>()
                ).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo inicializar la base de datos local.", ex);
            }
        }

        public SQLiteAsyncConnection Database => _connection;
    }
}
