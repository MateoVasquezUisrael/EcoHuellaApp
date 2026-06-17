// ┌─────────────────────────────────────────────────────────────────────────┐                        │
// │  IMPORTANTE: este modelo es independiente de Firebase o cualquier       │
// │  proveedor. Si el día de mañana migras de Firebase a una API propia,    │
// │  este modelo NO cambia.                                                 │
// └─────────────────────────────────────────────────────────────────────────┘
namespace EcoHuellaApp.Domain.Models
{
    /// <summary>
    /// Representa al usuario autenticado en el contexto de la aplicación.
    /// Immutable por diseño (init-only) para evitar mutaciones accidentales.
    /// </summary>
    public sealed record AppUser
    {
        /// <summary>Identificador único del usuario (UID de Firebase o similar).</summary>
        public string Uid { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        /// <summary>Nombre para mostrar (puede ser nombre completo o apodo).</summary>
        public string DisplayName { get; init; } = string.Empty;

        /// <summary>Rol del usuario dentro del sistema (Admin, Operador, etc.).</summary>
        public string Role { get; init; } = string.Empty;

        /// <summary>True si el email fue verificado por el proveedor de auth.</summary>
        public bool IsEmailVerified { get; init; }

        /// <summary>
        /// True cuando el administrador crea la cuenta y el usuario inicia sesión
        /// por primera vez. Fuerza la pantalla de cambio de contraseña obligatorio.
        /// </summary>
        public bool RequiresPasswordChange { get; init; }

        /// <summary>
        /// Proveedores de auth vinculados.
        /// Ejemplos: "password", "google.com", "facebook.com"
        /// Un usuario puede tener varios proveedores simultáneamente (account linking).
        /// </summary>
        public IReadOnlyList<string> LinkedProviders { get; init; } = [];

        /// <summary>
        /// Instancia vacía que representa "sin usuario".
        /// Útil para evitar nulos en contextos donde se necesita un objeto vacío.
        /// </summary>
        public static readonly AppUser Empty = new();
    }
}
