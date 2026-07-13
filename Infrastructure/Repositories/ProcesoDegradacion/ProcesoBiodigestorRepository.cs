using EcoHuellaApp.Data;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.ProcesoDegradacion;
using EcoHuellaApp.Helpers;
using SQLiteNetExtensionsAsync.Extensions;

namespace EcoHuellaApp.Infrastructure.Repositories.ProcesoDegradacion
{
    public class ProcesoBiodigestorRepository : IRepositoryGeneric<ProcesoBiodigestor>
    {
        private readonly AppDatabase _database;
        private string Status { get; set; }

        public ProcesoBiodigestorRepository(AppDatabase database)
        {
            _database = database;
        }

        public async Task ActualizarAsync(ProcesoBiodigestor entity)
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

        public async Task BorrarRegistroAsync(ProcesoBiodigestor entity)
        {
            try
            {
                entity.EstadoLlenado = true;
                entity.EstadoFinalizado = true;
                await _database.Database.UpdateAsync(entity);
            }
            catch (Exception ex)
            {
                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }

        public async Task GuardarRegistroAsync(ProcesoBiodigestor entity)
        {
            try
            {
                if (entity.Id == 0)
                {
                    await _database.Database.InsertAsync(entity);
                    Status = string.Format("Dato ingresado: {0}", entity);
                }
                else
                {
                    await _database.Database.UpdateAsync(entity);
                    Status = string.Format("Dato actualizado: {0}", entity);
                }
            }
            catch (Exception ex)
            {
                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }

        public async Task<ProcesoBiodigestor> ObtenerPorId(int id)
        {
            try
            {
                var proceso = await _database.Database
                    .Table<ProcesoBiodigestor>()
                    .Where(p => p.Id == id && !p.EstadoFinalizado)
                    .FirstOrDefaultAsync();

                if (proceso != null)
                {
                    await _database.Database.GetChildrenAsync(proceso);
                }

                return proceso;
            }
            catch (Exception ex)
            {
                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }

        public async Task<List<ProcesoBiodigestor>> ObtenerTodosAsync()
        {
            try
            {
                var procesos = await _database.Database
                    .GetAllWithChildrenAsync<ProcesoBiodigestor>(p => !p.EstadoFinalizado);

                return procesos
                    .OrderByDescending(p => p.FechaInicio)
                    .ToList();
            }
            catch (Exception ex)
            {
                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }

        public async Task FinalizarProcesoAsync(int procesoId)
        {
            try
            {
                var proceso = await _database.Database
                    .Table<ProcesoBiodigestor>()
                    .Where(p => p.Id == procesoId && !p.EstadoFinalizado)
                    .FirstOrDefaultAsync();

                if (proceso is null)
                    throw new InvalidOperationException("El proceso no existe o ya fue finalizado.");

                await _database.Database.GetChildrenAsync(proceso);

                var matematicaVerde = new MatematicaVerde();
                double masaCarbono = proceso.Biodigestor?.CapacidadMaxima ?? 0;

                proceso.MasaRestante = matematicaVerde.PerdidaMasa(masaCarbono);
                proceso.MetanoEvitado = matematicaVerde.CalcularMetanoEvitado(masaCarbono);
                proceso.CarbonoEvitado = matematicaVerde.ConversionMetanoCarbono(masaCarbono);
                proceso.FechaCierre = DateTime.Now;
                proceso.EstadoLlenado = true;
                proceso.EstadoFinalizado = true;

                await _database.Database.UpdateAsync(proceso);
                Status = string.Format("Proceso finalizado: {0}", proceso);
            }
            catch (Exception ex)
            {
                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }

        public async Task<List<ProcesoBiodigestor>> ObtenerFinalizadosAsync(int? biodigestorId = null)
        {
            try
            {
                var query = _database.Database
                    .Table<ProcesoBiodigestor>()
                    .Where(p => p.EstadoFinalizado);

                if (biodigestorId.HasValue)
                    query = query.Where(p => p.BiodigestorId == biodigestorId.Value);

                var finalizados = await query.ToListAsync();

                foreach (var proceso in finalizados)
                {
                    await _database.Database.GetChildrenAsync(proceso);
                }

                return finalizados
                    .OrderByDescending(p => p.FechaCierre)
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
