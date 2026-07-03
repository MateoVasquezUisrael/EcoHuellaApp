using EcoHuellaApp.Data;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcoHuellaApp.Infrastructure.Repositories
{
    public class PuntoRecoleccionRepository : IRepositoryGeneric<PuntoRecoleccion>
    {
        private readonly AppDatabase _database;
        private string Status { get; set; }

        public PuntoRecoleccionRepository(AppDatabase database)
        {
            _database = database;
        }

        public async Task ActualizarAsync(PuntoRecoleccion entity)
        {
            try
            {
                await _database.Database.UpdateAsync(entity);

                Status = string.Format("Dato actualizado: ", entity);
            }
            catch (Exception ex)
            {
                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }

        public async Task BorrarRegistroAsync(PuntoRecoleccion entity)
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

        public async Task GuardarRegistroAsync(PuntoRecoleccion entity)
        {
            try
            {
                await _database.Database.InsertAsync(entity);

                Status = string.Format("Dato ingresado: ", entity);
            }
            catch (Exception ex)
            {
                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }

        public async Task<PuntoRecoleccion> ObtenerPorId(int id)
        {
            try
            {
                return await _database.Database
                    .Table<PuntoRecoleccion>()
                    .Where(p => p.Id == id)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }

        public async Task<List<PuntoRecoleccion>> ObtenerTodosAsync()
        {
            try
            {
                return await _database.Database
                    .Table<PuntoRecoleccion>()
                    .Where(p => p.Estado)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }
    }
}
