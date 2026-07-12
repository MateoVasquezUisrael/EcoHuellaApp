using EcoHuellaApp.Data;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.ProcesoDegradacion;
using SQLiteNetExtensionsAsync.Extensions;

namespace EcoHuellaApp.Infrastructure.Repositories.ProcesoDegradacion
{
    public class EntradasProcesoBiodigestorRepository : IRepositoryGeneric<EntradasProcesoBiodigestor>
    {
        private readonly AppDatabase _database;
        private string Status { get; set; }

        public EntradasProcesoBiodigestorRepository(AppDatabase database)
        {
            _database = database;
        }

        public async Task ActualizarAsync(EntradasProcesoBiodigestor entity)
        {
            try
            {
                await _database.Database.UpdateAsync(entity);
                Status = string.Format("Dato actualizado: {0}", entity);
            }
            catch (Exception ex)
            {
                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }

        public async Task BorrarRegistroAsync(EntradasProcesoBiodigestor entity)
        {
            try
            {
                entity.Estado = false;
                await _database.Database.UpdateAsync(entity);
            }
            catch (Exception ex)
            {
                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }

        public async Task GuardarRegistroAsync(EntradasProcesoBiodigestor entity)
        {
            try
            {
                await _database.Database.InsertAsync(entity);
                Status = string.Format("Dato ingresado: {0}", entity);
            }
            catch (Exception ex)
            {
                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }

        public async Task<EntradasProcesoBiodigestor> ObtenerPorId(int id)
        {
            try
            {
                var entrada = await _database.Database
                    .Table<EntradasProcesoBiodigestor>()
                    .Where(e => e.Id == id && e.Estado)
                    .FirstOrDefaultAsync();

                if (entrada != null)
                {
                    await _database.Database.GetChildrenAsync(entrada);
                }

                return entrada;
            }
            catch (Exception ex)
            {
                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }

        public async Task<List<EntradasProcesoBiodigestor>> ObtenerTodosAsync()
        {
            try
            {
                var entradas = await _database.Database
                    .GetAllWithChildrenAsync<EntradasProcesoBiodigestor>(e => e.Estado);

                return entradas
                    .OrderByDescending(e => e.FechaIngreso)
                    .ToList();
            }
            catch (Exception ex)
            {
                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }
    }
}
