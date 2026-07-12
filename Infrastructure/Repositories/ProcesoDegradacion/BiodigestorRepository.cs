using EcoHuellaApp.Data;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.ProcesoDegradacion;
using SQLiteNetExtensionsAsync.Extensions;

namespace EcoHuellaApp.Infrastructure.Repositories.ProcesoDegradacion
{
    public class BiodigestorRepository : IRepositoryGeneric<Biodigestor>
    {
        private readonly AppDatabase _database;
        private string Status { get; set; }

        public BiodigestorRepository(AppDatabase database)
        {
            _database = database;
        }

        public async Task ActualizarAsync(Biodigestor entity)
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

        public async Task BorrarRegistroAsync(Biodigestor entity)
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

        public async Task GuardarRegistroAsync(Biodigestor entity)
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

        public async Task<Biodigestor> ObtenerPorId(int id)
        {
            try
            {
                var biodigestor = await _database.Database
                    .Table<Biodigestor>()
                    .Where(b => b.Id == id && b.Estado)
                    .FirstOrDefaultAsync();

                if (biodigestor != null)
                {
                    await _database.Database.GetChildrenAsync(biodigestor);
                }

                return biodigestor;
            }
            catch (Exception ex)
            {
                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }

        public async Task<List<Biodigestor>> ObtenerTodosAsync()
        {
            try
            {
                var biodigestores = await _database.Database
                    .GetAllWithChildrenAsync<Biodigestor>(b => b.Estado);

                return biodigestores
                    .OrderByDescending(b => b.Id)
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
