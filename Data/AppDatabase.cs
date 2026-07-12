using EcoHuellaApp.Domain.Models;
using SQLite;
using EcoHuellaApp.Domain.Models.Recoleccion;
using EcoHuellaApp.Domain.Models.ProcesoDegradacion;
using EcoHuellaApp.Domain.Models.ProcesoComposteraArtesanal;
using EcoHuellaApp.Domain.Models.Ventas;

namespace EcoHuellaApp.Data
{
    public class AppDatabase
    {
        private readonly SQLiteAsyncConnection _connection;

        public AppDatabase(string _dbPath)
        {
            try
            {
                _connection = new SQLiteAsyncConnection(_dbPath);

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

        //get
        public SQLiteAsyncConnection Database
        {
            get
            {
                return _connection;
            }
        }

    }
}
