using EcoHuellaApp.Data;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.ProcesoComposteraArtesanal;
using SQLiteNetExtensionsAsync.Extensions;

namespace EcoHuellaApp.Infrastructure.Repositories.ProcesoComposteraArtesanal
{
    public class ComposteraArtesanalRepository : IRepositoryGeneric<ComposteraArtesanal>
    {
        private readonly AppDatabase _database;
        private readonly IUserSessionService _session;
        private string Status { get; set; }

        public ComposteraArtesanalRepository(AppDatabase database, IUserSessionService session)
        {
            _database = database;
            _session = session;
        }
        private string UsuarioUid => _session.AuthUser?.Uid ?? throw new InvalidOperationException("No hay una sesión activa.");

        public async Task ActualizarAsync(ComposteraArtesanal entity)
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

        public async Task BorrarRegistroAsync(ComposteraArtesanal entity)
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

        public async Task GuardarRegistroAsync(ComposteraArtesanal entity)
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

        public async Task<ComposteraArtesanal> ObtenerPorId(int id)
        {
            try
            {
                var compostera = await _database.Database
                    .Table<ComposteraArtesanal>()
                    .Where(c => c.Id == id && c.Estado && c.UsuarioUid == UsuarioUid)
                    .FirstOrDefaultAsync();

                if (compostera != null)
                {
                    await _database.Database.GetChildrenAsync(compostera);
                }

                return compostera;
            }
            catch (Exception ex)
            {
                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }

        public async Task<List<ComposteraArtesanal>> ObtenerTodosAsync()
        {
            try
            {
                var composteras = await _database.Database
                    .GetAllWithChildrenAsync<ComposteraArtesanal>(c => c.Estado && c.UsuarioUid == UsuarioUid);

                return composteras
                    .OrderByDescending(c => c.Id)
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
