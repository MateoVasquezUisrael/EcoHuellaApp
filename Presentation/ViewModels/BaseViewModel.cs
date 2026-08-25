using CommunityToolkit.Mvvm.ComponentModel;

namespace EcoHuellaApp.Presentation.ViewModels
{
    /// <summary>
    /// ViewModel base. Todos los ViewModels del proyecto deben heredar de esta clase.
    /// La keyword 'partial' es obligatoria para que CommunityToolkit genere
    /// el código de INotifyPropertyChanged en tiempo de compilación.
    /// </summary>
    public abstract partial class BaseViewModel : ObservableObject
    {
        // ── Estado de carga ──────────────────────────────────────────────────

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        /// <summary>Inverso de IsBusy — para deshabilitar controles en XAML.</summary>
        public bool IsNotBusy => !IsBusy;

        // ── Estado de error ──────────────────────────────────────────────────

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasError))]
        private string _errorMessage = string.Empty;

        /// <summary>True cuando hay un mensaje de error visible al usuario.</summary>
        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

        // ── Helpers protegidos ───────────────────────────────────────────────

        protected void SetError(string message) => ErrorMessage = message;
        protected void ClearError()             => ErrorMessage = string.Empty;

        /// <summary>Página visible actualmente — único punto de acceso a la UI para diálogos/navegación modal.</summary>
        protected static Page? PaginaActual() =>
            Application.Current?.Windows is { Count: > 0 } windows ? windows[0].Page : null;

        protected async Task MostrarAvisoAsync(string mensaje, string titulo = "Aviso")
        {
            var pagina = PaginaActual();
            if (pagina is not null)
                await pagina.DisplayAlertAsync(titulo, mensaje, "Aceptar");
        }

        protected async Task<bool> ConfirmarAsync(string mensaje, string titulo = "Confirmar")
        {
            var pagina = PaginaActual();
            return pagina is not null && await pagina.DisplayAlertAsync(titulo, mensaje, "Sí", "No");
        }

        /// <summary>
        /// Wrapper estándar para comandos async.
        /// Maneja automáticamente: IsBusy=true/false, captura de excepciones,
        /// y limpieza de errores previos antes de cada ejecución.
        ///
        /// USO en cualquier ViewModel:
        ///   await ExecuteAsync(async () => {
        ///       var result = await _service.DoSomethingAsync();
        ///       ...
        ///   });
        /// </summary>
        protected async Task ExecuteAsync(Func<Task> operation)
        {
            if (IsBusy) return;   // Previene doble-tap en botones

            ClearError();
            IsBusy = true;
            try
            {
                await operation();
            }
            catch (Exception ex)
            {
                // Error inesperado — loguea en Debug y muestra mensaje genérico
                System.Diagnostics.Debug.WriteLine($"[{GetType().Name}] Error: {ex}");
                SetError("Ocurrió un error inesperado. Intenta de nuevo.");
            }
            finally
            {
                // Always: IsBusy vuelve a false sin importar lo que ocurra
                IsBusy = false;
            }
        }
    }
}
