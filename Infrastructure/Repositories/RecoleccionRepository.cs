using EcoHuellaApp.Data;
using System;
using System.Collections.Generic;
using System.Text;
using SQLitePCL;
using SQLiteNetExtensionsAsync.Extensions;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.Recoleccion;

namespace EcoHuellaApp.Infrastructure.Repositories
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
            var recolecciones = await _database.Database
                .GetAllWithChildrenAsync<Recoleccion>(r => r.Estado);

            return recolecciones
                .OrderByDescending(r => r.Fecha)
                .ToList();
        }

        public async Task<Recoleccion> ObtenerPorId(int id)
        {
                var recoleccion = await _database.Database
                .Table<Recoleccion>()
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync();


                if (recoleccion != null)
                {
                    await _database.Database.GetChildrenAsync(recoleccion);
                }

                return recoleccion;

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
