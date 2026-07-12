using EcoHuellaApp.Data;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.Ventas;
using SQLiteNetExtensionsAsync.Extensions;

namespace EcoHuellaApp.Infrastructure.Repositories.Ventas
{
    public class SacosCompostRepository : IRepositoryGeneric<SacosCompost>
    {
        private readonly AppDatabase _database;
        private string Status { get; set; }

        public SacosCompostRepository(AppDatabase database)
        {
            _database = database;
        }

        public async Task ActualizarAsync(SacosCompost entity)
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

        public async Task BorrarRegistroAsync(SacosCompost entity)
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

        public async Task GuardarRegistroAsync(SacosCompost entity)
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

        public async Task<SacosCompost> ObtenerPorId(int id)
        {
            try
            {
                var saco = await _database.Database
                    .Table<SacosCompost>()
                    .Where(s => s.Id == id)
                    .FirstOrDefaultAsync();

                return saco;
            }
            catch (Exception ex)
            {
                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }

        public async Task<List<SacosCompost>> ObtenerTodosAsync()
        {
            try
            {
                var sacos = await _database.Database
                    .Table<SacosCompost>()
                    .ToListAsync();

                return sacos
                    .OrderByDescending(s => s.Id)
                    .ToList();
            }
            catch (Exception ex)
            {
                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }

        public async Task<List<SacosCompost>> ObtenerDisponiblesAsync()
        {
            try
            {
                var sacos = await _database.Database
                    .Table<SacosCompost>()
                    .Where(s => s.Estado)
                    .ToListAsync();

                return sacos
                    .OrderByDescending(s => s.Id)
                    .ToList();
            }
            catch (Exception ex)
            {
                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }

        public async Task<List<SacosCompost>> ObtenerUsadosOVendidosAsync()
        {
            try
            {
                var sacos = await _database.Database
                    .Table<SacosCompost>()
                    .Where(s => !s.Estado)
                    .ToListAsync();

                return sacos
                    .OrderByDescending(s => s.Fecha)
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
