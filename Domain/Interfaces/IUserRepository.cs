using EcoHuellaApp.Domain.Models;

namespace EcoHuellaApp.Domain.Interfaces
{
    /// <summary>Consulta y administra usuarios.</summary>
    public interface IUserRepository
    {
        /// <summary>Busca un usuario por UID.</summary>
        Task<UsuarioSistema?> GetByUidAsync(string uid, string idToken);
        Task<bool> CreateAsync(UsuarioSistema usuario, string idToken);
    }
}
