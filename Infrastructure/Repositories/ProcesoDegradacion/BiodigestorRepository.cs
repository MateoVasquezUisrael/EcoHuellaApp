using EcoHuellaApp.Data;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.ProcesoDegradacion;
using SQLiteNetExtensionsAsync.Extensions;

namespace EcoHuellaApp.Infrastructure.Repositories.ProcesoDegradacion
{
    public class BiodigestorRepository : IRepositoryGeneric<Biodigestor>
    {
        private readonly AppDatabase _database;
        private readonly IUserSessionService _session;
        private string Status { get; set; }

        public BiodigestorRepository(AppDatabase database, IUserSessionService session)
        {
            _database = database;
            _session = session;
        }
        private string UsuarioUid => _session.AuthUser?.Uid ?? throw new InvalidOperationException("No hay una sesión activa.");

        public async Task ActualizarAsync(Biodigestor entity)
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

        public async Task BorrarRegistroAsync(Biodigestor entity)
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

        public async Task GuardarRegistroAsync(Biodigestor entity)
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

        public async Task<Biodigestor> ObtenerPorId(int id)
        {
            try
            {
                var biodigestor = await _database.Database
                    .Table<Biodigestor>()
                    .Where(b => b.Id == id && b.Estado && b.UsuarioUid == UsuarioUid)
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
                    .GetAllWithChildrenAsync<Biodigestor>(b => b.Estado && b.UsuarioUid == UsuarioUid);

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
