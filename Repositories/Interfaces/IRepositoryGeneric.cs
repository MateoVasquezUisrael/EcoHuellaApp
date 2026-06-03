using System;
using System.Collections.Generic;
using System.Text;

namespace EcoHuellaApp.Repositories.Interfaces
{
    public interface IRepositoryGeneric<T>
    {
        public Task<List<T>> ObtenerTodosAsync(); //hacerlo asyn en la implementación

        public Task<T> ObtenerPorId(int id); //con el id debería ser suficiente, recuerda que sea async

        public Task ActualizarAsync(T entity); //hacer async

        public Task GuardarRegistroAsync(T entity); //siempre se devuelve un int en sql

        public Task BorrarRegistroAsync(T entity); //este podría cambiar para que solo sea borrado lógico
    }
}
