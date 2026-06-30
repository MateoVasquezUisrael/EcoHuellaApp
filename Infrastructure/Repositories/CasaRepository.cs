using EcoHuellaApp.Data;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;


namespace EcoHuellaApp.Infrastructure.Repositories
{
    public class CasaRepository : IRepositoryGeneric<Casa>
    {
        //esto es la conexión que debe guardarse localmente
        private readonly AppDatabase _database;
        private string Status { get; set; }
        public CasaRepository(AppDatabase database)
        {
            _database = database;
        }

        public async Task ActualizarAsync(Casa entity)
        {
            try
            {
                await _database.Database.UpdateAsync(entity);

                Status = string.Format("Dato ingresado: ", entity);
            }
            catch (Exception ex)
            {

                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }

        public async Task BorrarRegistroAsync(Casa entity)
        {
            try
            {
                //temporal, LUEGO MANDAR A SERVICE
                entity.Estado = false;

                await _database.Database.UpdateAsync(entity);
            }
            catch (Exception ex)
            {

                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }

        public async Task GuardarRegistroAsync(Casa entity)
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

        public async Task<Casa> ObtenerPorId(int id)
        {
            try
            {
                return await _database.Database
                    .Table<Casa>()
                    .Where(c => c.Id == id)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Status = string.Format("Error: {0}", ex.Message);
                throw;
            }
        }

        public async Task<List<Casa>> ObtenerTodosAsync()
        {
            try
            {
                return await _database.Database
                .Table<Casa>()
                .Where(c => c.Estado)
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
