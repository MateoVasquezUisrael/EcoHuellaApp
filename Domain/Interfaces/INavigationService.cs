
namespace EcoHuellaApp.Domain.Interfaces
{
    public interface INavigationService
    {
        /// <summary>
        /// Navega a la pantalla de cambio de contraseña obligatorio.
        /// Se llama después de un primer login exitoso.
        /// </summary>
        Task GoToChangePasswordAsync();

        /// <summary>
        /// Reemplaza el árbol de navegación completo con AppShell.
        /// Se llama después de un login exitoso o tras cambiar la contraseña.
        /// Esta operación es irreversible (no hay back a Login).
        /// </summary>
        void GoToMainApp();

        /// <summary>
        /// Regresa a la pantalla anterior dentro del flujo de auth.
        /// </summary>
        Task GoBackAsync();

        Task GoToLoginAsync();
    }
}
