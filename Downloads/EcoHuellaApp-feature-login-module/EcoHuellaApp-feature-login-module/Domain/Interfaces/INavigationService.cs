
namespace EcoHuellaApp.Domain.Interfaces
{
    public interface INavigationService
    {
        /// <summary>Abre el cambio de contraseña.</summary>
        Task GoToChangePasswordAsync();
        Task GoToRegistrationAsync();

        /// <summary>Abre la aplicación principal.</summary>
        void GoToMainApp();
        void GoToGuestDemo();

        /// <summary>Regresa a la pantalla anterior.</summary>
        Task GoBackAsync();

        Task GoToLoginAsync();
    }
}
