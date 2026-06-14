using EcoHuellaApp.Domain.Models;

namespace EcoHuellaApp.Domain.Interfaces
{
    /// <summary>
    /// Contrato del servicio de autenticación.
    /// Desacopla completamente los ViewModels de Firebase u otro proveedor.
    /// </summary>
    public interface IAuthService
    {
        AppUser? CurrentUser { get; }

        /// <summary>Sincrono — no hace llamadas de red. Usa caché del SDK nativo.</summary>
        bool HasActiveSession();

        Task<AuthResult> SignInWithEmailPasswordAsync(string email, string password);
        Task<AuthResult> SignInWithGoogleAsync();
        Task<AuthResult> SignOutAsync();

        /// <summary>Actualiza contraseña. Requiere sesión reciente (primer login).</summary>
        Task<AuthResult> UpdatePasswordAsync(string newPassword);

        Task<AuthResult> SendPasswordResetEmailAsync(string email);
        Task<string?> GetFreshTokenAsync();
    }
}
