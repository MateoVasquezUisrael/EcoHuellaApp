using EcoHuellaApp.Data;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.Recoleccion;
using System;
using System.Collections.Generic;
using System.Text;


namespace EcoHuellaApp.Infrastructure.Repositories
{
    public class CasaRepository : IRepositoryGeneric<Casa>
    {
        //esto es la conexión que debe guardarse localmente
        private readonly AppDatabase _database;
        private readonly IUserSessionService _session;
        private string Status { get; set; }
        public CasaRepository(AppDatabase database, IUserSessionService session)
        {
            _database = database;
            _session = session;
        }
        private string UsuarioUid => _session.AuthUser?.Uid ?? throw new InvalidOperationException("No hay una sesión activa.");

        public async Task ActualizarAsync(Casa entity)
        {
            try
            {
                entity.UsuarioUid = UsuarioUid;
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
                entity.UsuarioUid = UsuarioUid;

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

        public async Task<Casa> ObtenerPorId(int id)
        {
            try
            {
                return await _database.Database
                    .Table<Casa>()
                    .Where(c => c.Id == id && c.UsuarioUid == UsuarioUid)
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
                .Where(c => c.Estado && c.UsuarioUid == UsuarioUid)
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
