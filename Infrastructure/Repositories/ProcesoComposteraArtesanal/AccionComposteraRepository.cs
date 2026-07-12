using EcoHuellaApp.Data;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.ProcesoComposteraArtesanal;
using SQLiteNetExtensionsAsync.Extensions;

namespace EcoHuellaApp.Infrastructure.Repositories.ProcesoComposteraArtesanal
{
    public class AccionComposteraRepository : IRepositoryGeneric<AccionCompostera>
    {
        private readonly AppDatabase _database;
        private string Status { get; set; }

        public AccionComposteraRepository(AppDatabase database)
        {
            _database = database;
        }

        public async Task ActualizarAsync(AccionCompostera entity)
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

        public async Task BorrarRegistroAsync(AccionCompostera entity)
        {
            try
            {
                entity.TipoAccion = "ELIMINADO";
                await _database.Database.UpdateAsync(entity);
            }
            catch (Exception ex)
            {
                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }

        public async Task GuardarRegistroAsync(AccionCompostera entity)
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

        public async Task<AccionCompostera> ObtenerPorId(int id)
        {
            try
            {
                var accion = await _database.Database
                    .Table<AccionCompostera>()
                    .Where(a => a.Id == id)
                    .FirstOrDefaultAsync();

                if (accion != null)
                {
                    await _database.Database.GetChildrenAsync(accion);
                }

                return accion;
            }
            catch (Exception ex)
            {
                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }

        public async Task<List<AccionCompostera>> ObtenerTodosAsync()
        {
            try
            {
                var acciones = await _database.Database
                    .GetAllWithChildrenAsync<AccionCompostera>();

                return acciones
                    .Where(a => a.TipoAccion != "ELIMINADO")
                    .OrderByDescending(a => a.FechaAccion)
                    .ToList();
            }
            catch (Exception ex)
            {
                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }

        public async Task<List<AccionCompostera>> ObtenerPorComposteraAsync(int composteraId)
        {
            try
            {
                var todas = await ObtenerTodosAsync();
                return todas
                    .Where(a => a.ComposteraArtesanalId == composteraId)
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
