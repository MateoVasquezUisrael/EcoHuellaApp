using EcoHuellaApp.Data;
using EcoHuellaApp.Domain.Models;
using EcoHuellaApp.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcoHuellaApp.Repositories.Implementations
{
    public class CompostajeRepository : IRepositoryGeneric<Compostaje>
    {
        //instancia de la datbase que debe existir aquí, esto se manda al mauiapp porque... Injección de Dependencias.
        private AppDatabase _database;

        public Task ActualizarAsync(Compostaje entity)
        {
            throw new NotImplementedException();
        }

        public Task BorrarRegistroAsync(Compostaje entity)
        {
            throw new NotImplementedException();
        }

        public Task GuardarRegistroAsync(Compostaje entity)
        {
            throw new NotImplementedException();
        }

        public Task<Compostaje> ObtenerPorId(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Compostaje>> ObtenerTodosAsync()
        {
            throw new NotImplementedException();
        }
    }
}
