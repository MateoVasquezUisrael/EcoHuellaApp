namespace EcoHuellaApp.Domain.Models
{
    /// <summary>
    /// Representa al usuario tal como está registrado en Firestore.
    /// Es independiente de Firebase Auth: un usuario puede estar autenticado
    /// (Firebase) pero no autorizado (no existe en Firestore).
    ///
    /// Estructura del documento en Firestore → colección: "usuarios" → id: {uid}
    /// {
    ///   "uid":             string  (igual al UID de Firebase Auth)
    ///   "email":           string
    ///   "nombre":          string  (nombre completo para mostrar)
    ///   "rol":             string  ("Usuario" | "Supervisor" | "Administrador")
    ///   "activo":          boolean (false = cuenta desactivada sin eliminar)
    ///   "fechaCreacion":   timestamp
    /// }
    /// </summary>
    public sealed record UsuarioSistema
    {
        public string    Uid    { get; init; } = string.Empty;
        public string    Email  { get; init; } = string.Empty;
        public string    Nombre { get; init; } = string.Empty;
        public string    Organizacion { get; init; } = string.Empty;
        public RolSistema Rol   { get; init; } = RolSistema.Usuario;

        /// <summary>
        /// Cuando es false la cuenta existe en Firestore pero no puede acceder.
        /// El administrador puede reactivarla sin recrearla.
        /// </summary>
        public bool Activo { get; init; } = true;

        /// <summary>Instancia vacía para evitar nulos.</summary>
        public static readonly UsuarioSistema Empty = new();
    }
}
