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

                _connection.CreateTableAsync<Casa>().Wait();
                _connection.CreateTableAsync<PuntoRecoleccion>().Wait();
                _connection.CreateTableAsync<Recoleccion>().Wait();
                _connection.CreateTableAsync<Biodigestor>().Wait();
                _connection.CreateTableAsync<ProcesoBiodigestor>().Wait();
                _connection.CreateTableAsync<EntradasProcesoBiodigestor>().Wait();
                _connection.CreateTableAsync<ComposteraArtesanal>().Wait();
                _connection.CreateTableAsync<AccionCompostera>().Wait();
                _connection.CreateTableAsync<SacosCompost>().Wait();
            }
            catch (Exception ex)
            {
                throw new Exception("Error:" + ex);
            }
        }

        public SQLiteAsyncConnection Database => _connection;
    }
}
