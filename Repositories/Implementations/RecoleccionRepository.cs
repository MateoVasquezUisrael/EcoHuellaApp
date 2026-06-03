using EcoHuellaApp.Data;
using EcoHuellaApp.Domain.Models;
using EcoHuellaApp.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;


namespace EcoHuellaApp.Repositories.Implementations
{
    //TODO: meter los try y catchs
    public class RecoleccionRepository : IRepositoryGeneric<Recoleccion>
    {
        private readonly AppDatabase _database;
        private string Status { get; set; }

        public RecoleccionRepository(AppDatabase database)
        {
            _database = database;
        }

        public async Task<List<Recoleccion>> ObtenerTodosAsync()
        {
            return await _database.Database
                .Table<Recoleccion>()
                .Where(r => r.Estado)
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();
        }

        public async Task<Recoleccion> ObtenerPorId(int id)
        {
            return await _database.Database
                .Table<Recoleccion>()
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task ActualizarAsync(Recoleccion entity)
        {
            await _database.Database
                .UpdateAsync(entity);
        }

        public async Task GuardarRegistroAsync(Recoleccion entity)
        {
            await _database.Database
                .InsertAsync(entity);
        }

        public async Task BorrarRegistroAsync(Recoleccion entity)
        {
            entity.Estado = false;
            await _database.Database.UpdateAsync(entity);
        }
    }
}
