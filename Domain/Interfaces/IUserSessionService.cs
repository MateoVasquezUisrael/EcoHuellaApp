using EcoHuellaApp.Domain.Models;

namespace EcoHuellaApp.Domain.Interfaces
{
    /// <summary>Mantiene la sesión activa.</summary>
    public interface IUserSessionService
    {
        /// <summary>Usuario autenticado.</summary>
        AppUser? AuthUser { get; }

        /// <summary>Perfil y permisos.</summary>
        UsuarioSistema? SistemaUser { get; }

        /// <summary>Indica si la sesión está completa.</summary>
        bool IsAuthenticated { get; }

        void SetSession(AppUser authUser, UsuarioSistema sistemaUser);
        void ClearSession();
    }
}
