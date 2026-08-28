namespace EcoHuellaApp.Domain.Models
{
    /// <summary>Representa al usuario autenticado.</summary>
    public sealed record AppUser
    {
        /// <summary>Identificador único.</summary>
        public string Uid { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        /// <summary>Nombre para mostrar.</summary>
        public string DisplayName { get; init; } = string.Empty;

        /// <summary>Rol del usuario.</summary>
        public string Role { get; init; } = string.Empty;

        /// <summary>Indica si el correo fue verificado.</summary>
        public bool IsEmailVerified { get; init; }

        /// <summary>Indica si debe cambiar la contraseña.</summary>
        public bool RequiresPasswordChange { get; init; }

        /// <summary>Proveedores vinculados.</summary>
        public IReadOnlyList<string> LinkedProviders { get; init; } = [];

        /// <summary>Usuario vacío.</summary>
        public static readonly AppUser Empty = new();
    }
}
