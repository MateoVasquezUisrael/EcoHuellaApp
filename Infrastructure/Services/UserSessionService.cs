using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models;

namespace EcoHuellaApp.Infrastructure.Services
{
    /// <summary>
    /// Singleton que mantiene el estado de la sesión activa.
    /// Se pobla después de que Login + Firestore sean exitosos.
    /// Se limpia al hacer logout o cuando se deniega el acceso.
    /// </summary>
    public sealed class UserSessionService : IUserSessionService
    {
        private AppUser?       _authUser;
        private UsuarioSistema? _sistemaUser;

        public AppUser?        AuthUser     => _authUser;
        public UsuarioSistema? SistemaUser  => _sistemaUser;
        public bool            IsAuthenticated =>
            _authUser is not null && _sistemaUser is not null;

        public void SetSession(AppUser authUser, UsuarioSistema sistemaUser)
        {
            _authUser    = authUser;
            _sistemaUser = sistemaUser;
        }

        public void ClearSession()
        {
            _authUser    = null;
            _sistemaUser = null;
        }
    }
}
