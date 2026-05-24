using EcoHuellaApp.Data;
using EcoHuellaApp.Models;

using System;
using System.Collections.Generic;
using System.Text;


namespace EcoHuellaApp.Repositories
{
    public class RecoleccionRepositoriy
    {
        private readonly AppDatabase _database;

        public RecoleccionRepositoriy(AppDatabase database)
        {
            _database = database;
        }

        public async Task<List<Recoleccion>> ObtenerTodosAsync()
        {
            return await _database.Database.Table<Recoleccion>().ToListAsync();
        }

        public async Task<int> GuardarRegistroAsync(Recoleccion recoleccion)
        {
            return await _database.Database.InsertAsync(recoleccion);
        }
    }
}
