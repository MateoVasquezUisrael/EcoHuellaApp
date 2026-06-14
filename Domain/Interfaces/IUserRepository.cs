using EcoHuellaApp.Domain.Models;

namespace EcoHuellaApp.Domain.Interfaces
{
    /// <summary>
    /// Contrato para consultar usuarios del sistema en Firestore.
    /// Responsabilidad única: ¿tiene acceso este UID?
    ///
    /// Separado de IAuthService a propósito:
    ///   IAuthService  → autenticación (¿quién eres?)
    ///   IUserRepository → autorización (¿puedes entrar?)
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Busca el usuario en Firestore por su UID de Firebase Auth.
        /// Retorna null si el documento no existe (usuario no autorizado).
        /// El idToken se usa para autenticar la petición a Firestore.
        /// </summary>
        Task<UsuarioSistema?> GetByUidAsync(string uid, string idToken);
    }
}
