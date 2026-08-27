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
        private readonly IUserSessionService _session;
        private string Status { get; set; }

        public RecoleccionRepository(AppDatabase database, IUserSessionService session)
        {
            _database = database;
            _session = session;
        }
        private string UsuarioUid => _session.AuthUser?.Uid ?? throw new InvalidOperationException("No hay una sesión activa.");

        public async Task<List<Recoleccion>> ObtenerTodosAsync()
        {
            var recolecciones = await _database.Database
                .GetAllWithChildrenAsync<Recoleccion>(r => r.Estado && r.UsuarioUid == UsuarioUid);

            return recolecciones
                .OrderByDescending(r => r.Fecha)
                .ToList();
        }

        public async Task<Recoleccion> ObtenerPorId(int id)
        {
                var recoleccion = await _database.Database
                .Table<Recoleccion>()
                .Where(r => r.Id == id && r.UsuarioUid == UsuarioUid)
                .FirstOrDefaultAsync();


                if (recoleccion != null)
                {
                    await _database.Database.GetChildrenAsync(recoleccion);
                }

                return recoleccion;

        }

        public async Task ActualizarAsync(Recoleccion entity)
        {
            entity.UsuarioUid = UsuarioUid;
            await _database.Database
                .UpdateAsync(entity);
        }

        public async Task GuardarRegistroAsync(Recoleccion entity)
        {
            entity.UsuarioUid = UsuarioUid;
            await _database.Database
                .InsertAsync(entity);
        }

        public async Task BorrarRegistroAsync(Recoleccion entity)
        {
            entity.Estado = false;
            entity.UsuarioUid = UsuarioUid;
            await _database.Database.UpdateAsync(entity);
        }
    }
}
