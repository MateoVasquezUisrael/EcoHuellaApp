using EcoHuellaApp.Domain.Models;

namespace EcoHuellaApp.Domain.Interfaces
{
    /// <summary>Gestiona la autenticación.</summary>
    public interface IAuthService
    {
        AppUser? CurrentUser { get; }

        /// <summary>Devuelve la sesión almacenada.</summary>
        bool HasActiveSession();

        Task<AuthResult> SignInWithEmailPasswordAsync(string email, string password);
        Task<AuthResult> RegisterWithEmailPasswordAsync(string email, string password);
        Task<AuthResult> SignOutAsync();

        /// <summary>Actualiza la contraseña.</summary>
        Task<AuthResult> UpdatePasswordAsync(string newPassword);

        Task<AuthResult> SendPasswordResetEmailAsync(string email);
        Task<string?> GetFreshTokenAsync();
    }
}
