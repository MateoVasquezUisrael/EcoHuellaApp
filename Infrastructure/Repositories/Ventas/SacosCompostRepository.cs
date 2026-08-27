using EcoHuellaApp.Data;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.Ventas;
using SQLiteNetExtensionsAsync.Extensions;

namespace EcoHuellaApp.Infrastructure.Repositories.Ventas
{
    public class SacosCompostRepository : IRepositoryGeneric<SacosCompost>
    {
        private readonly AppDatabase _database;
        private readonly IUserSessionService _session;
        private string Status { get; set; }

        public SacosCompostRepository(AppDatabase database, IUserSessionService session)
        {
            _database = database;
            _session = session;
        }
        private string UsuarioUid => _session.AuthUser?.Uid ?? throw new InvalidOperationException("No hay una sesión activa.");

        public async Task ActualizarAsync(SacosCompost entity)
        {
            try
            {
                entity.UsuarioUid = UsuarioUid;
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
                entity.UsuarioUid = UsuarioUid;
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
                entity.UsuarioUid = UsuarioUid;
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
                    .Where(s => s.Id == id && s.UsuarioUid == UsuarioUid)
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
                    .Where(s => s.UsuarioUid == UsuarioUid)
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
                    .Where(s => s.Estado && s.UsuarioUid == UsuarioUid)
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
                    .Where(s => !s.Estado && s.UsuarioUid == UsuarioUid)
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
