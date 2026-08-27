using CommunityToolkit.Mvvm.ComponentModel;

namespace EcoHuellaApp.Presentation.ViewModels
{
    /// <summary>Estado común de los ViewModels.</summary>
    public abstract partial class BaseViewModel : ObservableObject
    {
        // Carga.

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        /// <summary>Indica si está disponible.</summary>
        public bool IsNotBusy => !IsBusy;

        // Errores.

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasError))]
        private string _errorMessage = string.Empty;

        /// <summary>Indica si existe un error.</summary>
        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

        // Utilidades.

        protected void SetError(string message) => ErrorMessage = message;
        protected void ClearError()             => ErrorMessage = string.Empty;

        /// <summary>Ejecuta una operación y controla su estado.</summary>
        protected async Task ExecuteAsync(Func<Task> operation)
        {
            if (IsBusy) return; // Evita el doble toque.

            ClearError();
            IsBusy = true;
            try
            {
                await operation();
            }
            catch (Exception ex)
            {
                // Registra el error.
                System.Diagnostics.Debug.WriteLine($"[{GetType().Name}] Error: {ex}");
                SetError("Ocurrió un error inesperado. Intenta de nuevo.");
            }
            finally
            {
                // Restablece el estado.
                IsBusy = false;
            }
        }
    }
}
