using EcoHuellaApp.Domain.Models;

namespace EcoHuellaApp.Domain.Interfaces
{
    /// <summary>
    /// Mantiene el estado de la sesión combinada (Firebase Auth + Firestore).
    /// Es el único lugar de verdad sobre quién está usando la app en este momento.
    ///
    /// Ciclo de vida:
    ///   Login exitoso + autorizado → SetSession()
    ///   Logout / acceso denegado  → ClearSession()
    ///
    /// Los ViewModels solo leen de aquí; nunca escriben directamente.
    /// </summary>
    public interface IUserSessionService
    {
        /// <summary>Usuario autenticado por Firebase. Null si no hay sesión.</summary>
        AppUser? AuthUser { get; }

        /// <summary>Perfil del usuario en Firestore con rol y estado.</summary>
        UsuarioSistema? SistemaUser { get; }

        /// <summary>True solo cuando ambos (Auth + Firestore) están cargados.</summary>
        bool IsAuthenticated { get; }

        void SetSession(AppUser authUser, UsuarioSistema sistemaUser);
        void ClearSession();
    }
}
