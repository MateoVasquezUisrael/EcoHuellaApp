using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models;

namespace EcoHuellaApp.Presentation.ViewModels
{
    /// <summary>
    /// Pantalla de cambio de contraseña obligatorio en el primer login.
    /// El usuario acaba de autenticarse → Firebase permite UpdatePassword
    /// sin re-autenticación (sesión reciente).
    /// Solo se piden: nueva contraseña + confirmación.
    /// </summary>
    public sealed partial class ChangePasswordViewModel : BaseViewModel
    {
        private readonly IAuthService       _authService;
        private readonly INavigationService _navigation;

        public ChangePasswordViewModel(IAuthService authService, INavigationService navigation)
        {
            _authService = authService;
            _navigation  = navigation;
        }

        // ── Formulario ────────────────────────────────────────────────────────

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ChangePasswordCommand))]
        private string _newPassword = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ChangePasswordCommand))]
        private string _confirmPassword = string.Empty;

        // ── Indicador de fortaleza ────────────────────────────────────────────

        [ObservableProperty] private string _strengthLabel    = string.Empty;
        [ObservableProperty] private Color  _strengthColor    = Colors.Transparent;
        [ObservableProperty] private double _strengthProgress = 0;

        partial void OnNewPasswordChanged(string value)
        {
            (StrengthLabel, StrengthColor, StrengthProgress) = value.Length switch
            {
                0    => (string.Empty,      Colors.Transparent,             0.00),
                < 8  => ("Débil",           Color.FromArgb("#E74C3C"),      0.25),
                < 12 => ("Media",           Color.FromArgb("#F39C12"),      0.60),
                _    => ("Fuerte",          Color.FromArgb("#27AE60"),      1.00)
            };
            ChangePasswordCommand.NotifyCanExecuteChanged();
        }

        // ── Comando ───────────────────────────────────────────────────────────

        [RelayCommand(CanExecute = nameof(CanChangePassword))]
        private async Task ChangePasswordAsync()
        {
            if (NewPassword != ConfirmPassword)
            {
                SetError("Las contraseñas no coinciden.");
                return;
            }

            await ExecuteAsync(async () =>
            {
                var result = await _authService.UpdatePasswordAsync(NewPassword);

                // Limpiar datos sensibles de memoria
                NewPassword     = string.Empty;
                ConfirmPassword = string.Empty;

                if (!result.IsSuccess)
                {
                    SetError(result.ErrorMessage ?? "No se pudo cambiar la contraseña.");
                    return;
                }

                _navigation.GoToMainApp();
            });
        }

        private bool CanChangePassword() =>
            !string.IsNullOrWhiteSpace(NewPassword)     &&
            !string.IsNullOrWhiteSpace(ConfirmPassword) &&
            NewPassword.Length >= 8                     &&
            IsNotBusy;
    }
}
