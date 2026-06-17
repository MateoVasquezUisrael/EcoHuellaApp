using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models;

namespace EcoHuellaApp.Infrastructure.Services
{
    /// <summary>
    /// Simula Firestore para MacCatalyst y pruebas locales.
    /// Los usuarios aquí deben corresponder con los de FakeAuthService.
    /// </summary>
    public sealed class FakeUserRepository : IUserRepository
    {
        private static readonly Dictionary<string, UsuarioSistema> _usuarios =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["fake-uid-001"] = new()
                {
                    Uid    = "fake-uid-001",
                    Email  = "admin@ecohuellaapp.com",
                    Nombre = "Administrador del Sistema",
                    Rol    = RolSistema.Administrador,
                    Activo = true
                },
                ["fake-uid-002"] = new()
                {
                    Uid    = "fake-uid-002",
                    Email  = "operador@ecohuellaapp.com",
                    Nombre = "Operador Demo",
                    Rol    = RolSistema.Usuario,
                    Activo = true
                }
                // Nota: "fake-google-001" (usuario Google fake) NO existe aquí
                // intencionalmente, para probar el rechazo de acceso no autorizado.
            };

        public async Task<UsuarioSistema?> GetByUidAsync(string uid, string idToken)
        {
            await Task.Delay(300); // simular latencia de red
            return _usuarios.TryGetValue(uid, out var usuario) ? usuario : null;
        }
    }
}
