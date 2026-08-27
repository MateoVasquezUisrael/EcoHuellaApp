namespace EcoHuellaApp.Domain.Models
{
    /// <summary>
    /// Roles del sistema. El valor numérico define el nivel de privilegio:
    /// mayor número = mayor privilegio. Útil para comparaciones de acceso.
    /// Los strings deben coincidir exactamente con el campo "rol" en Firestore.
    /// </summary>
    public enum RolSistema
    {
        Usuario        = 0,   // Acceso básico a la aplicación
        Supervisor     = 1,   // Supervisión y reportes
        Administrador  = 2    // Acceso completo
    }
}
