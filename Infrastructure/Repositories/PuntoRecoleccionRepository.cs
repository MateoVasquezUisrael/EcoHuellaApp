using EcoHuellaApp.Data;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.Recoleccion;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcoHuellaApp.Infrastructure.Repositories
{
    public class PuntoRecoleccionRepository : IRepositoryGeneric<PuntoRecoleccion>
    {
        private readonly AppDatabase _database;
        private readonly IUserSessionService _session;
        private string Status { get; set; }

        public PuntoRecoleccionRepository(AppDatabase database, IUserSessionService session)
        {
            _database = database;
            _session = session;
        }
        private string UsuarioUid => _session.AuthUser?.Uid ?? throw new InvalidOperationException("No hay una sesión activa.");

        public async Task ActualizarAsync(PuntoRecoleccion entity)
        {
            try
            {
                entity.UsuarioUid = UsuarioUid;
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
                entity.UsuarioUid = UsuarioUid;

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
                entity.UsuarioUid = UsuarioUid;
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
                    .Where(p => p.Id == id && p.UsuarioUid == UsuarioUid)
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
                .Where(p => p.Estado && p.UsuarioUid == UsuarioUid)
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
