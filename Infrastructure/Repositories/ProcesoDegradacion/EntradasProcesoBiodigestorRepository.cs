using EcoHuellaApp.Data;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.ProcesoDegradacion;
using SQLiteNetExtensionsAsync.Extensions;

namespace EcoHuellaApp.Infrastructure.Repositories.ProcesoDegradacion
{
    public class EntradasProcesoBiodigestorRepository : IRepositoryGeneric<EntradasProcesoBiodigestor>
    {
        private readonly AppDatabase _database;
        private readonly IUserSessionService _session;
        private string Status { get; set; }

        public EntradasProcesoBiodigestorRepository(AppDatabase database, IUserSessionService session)
        {
            _database = database;
            _session = session;
        }
        private string UsuarioUid => _session.AuthUser?.Uid ?? throw new InvalidOperationException("No hay una sesión activa.");

        public async Task ActualizarAsync(EntradasProcesoBiodigestor entity)
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

        public async Task BorrarRegistroAsync(EntradasProcesoBiodigestor entity)
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

        public async Task GuardarRegistroAsync(EntradasProcesoBiodigestor entity)
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

        public async Task<EntradasProcesoBiodigestor> ObtenerPorId(int id)
        {
            try
            {
                var entrada = await _database.Database
                    .Table<EntradasProcesoBiodigestor>()
                    .Where(e => e.Id == id && e.Estado && e.UsuarioUid == UsuarioUid)
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
                    .GetAllWithChildrenAsync<EntradasProcesoBiodigestor>(e => e.Estado && e.UsuarioUid == UsuarioUid);

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
