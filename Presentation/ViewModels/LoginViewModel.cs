using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models;

namespace EcoHuellaApp.Presentation.ViewModels
{
    public sealed partial class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService        _authService;
        private readonly IUserRepository     _userRepository;
        private readonly IUserSessionService _session;
        private readonly INavigationService  _navigation;

        public LoginViewModel(
            IAuthService        authService,
            IUserRepository     userRepository,
            IUserSessionService session,
            INavigationService  navigation)
        {
            _authService    = authService;
            _userRepository = userRepository;
            _session        = session;
            _navigation     = navigation;
        }

        // ── Formulario ────────────────────────────────────────────────────────

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SignInCommand))]
        private string _email = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SignInCommand))]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _isPasswordVisible;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasSuccess))]
        private string _successMessage = string.Empty;

        public bool HasSuccess => !string.IsNullOrWhiteSpace(SuccessMessage);

        // ── Login email / contraseña ──────────────────────────────────────────

        [RelayCommand(CanExecute = nameof(CanSignIn))]
        private async Task SignInAsync()
        {
            await ExecuteAsync(async () =>
            {
                SuccessMessage = string.Empty;

                var result = await _authService.SignInWithEmailPasswordAsync(
                    Email.Trim().ToLowerInvariant(), Password);

                Password = string.Empty; // limpiar de memoria

                if (!result.IsSuccess)
                {
                    SetError(result.ErrorMessage ?? "Error al iniciar sesión.");
                    return;
                }

                await AuthorizeAndNavigateAsync(result.User!);
            });
        }

        // ── Login con Google ──────────────────────────────────────────────────

        [RelayCommand]
        private async Task SignInWithGoogleAsync()
        {
            await ExecuteAsync(async () =>
            {
                SuccessMessage = string.Empty;

                var result = await _authService.SignInWithGoogleAsync();

                if (!result.IsSuccess)
                {
                    // Cancelación silenciosa del selector de cuentas
                    if (result.ErrorCode == AuthErrorCode.Cancelled &&
                        string.IsNullOrEmpty(result.ErrorMessage))
                        return;

                    SetError(result.ErrorMessage ?? "Error al iniciar sesión con Google.");
                    return;
                }

                await AuthorizeAndNavigateAsync(result.User!);
            });
        }

        // ── Recuperar contraseña ──────────────────────────────────────────────

        [RelayCommand]
        private async Task ForgotPasswordAsync()
        {
            Page? page = Application.Current?.Windows[0].Page is NavigationPage np
                ? np.CurrentPage
                : Application.Current?.Windows[0].Page;

            if (page is null) return;

            var email = await page.DisplayPromptAsync(
                title:       "Recuperar contraseña",
                message:     "Ingresa tu correo para recibir las instrucciones.",
                accept:      "Enviar",
                cancel:      "Cancelar",
                placeholder: "correo@ejemplo.com",
                keyboard:    Keyboard.Email);

            if (string.IsNullOrWhiteSpace(email)) return;

            await ExecuteAsync(async () =>
            {
                var result = await _authService.SendPasswordResetEmailAsync(
                    email.Trim().ToLowerInvariant());

                if (result.IsSuccess)
                {
                    SuccessMessage = "Te enviamos un correo con las instrucciones.";
                    ClearError();
                }
                else
                {
                    SetError(result.ErrorMessage ?? "No se pudo enviar el correo.");
                }
            });
        }

        // ── Visibilidad de contraseña ─────────────────────────────────────────

        [RelayCommand]
        private void TogglePasswordVisibility() =>
            IsPasswordVisible = !IsPasswordVisible;

        // ── Autorización Firestore + navegación ───────────────────────────────

        /// <summary>
        /// Paso 2 del flujo de acceso: valida que el usuario autenticado
        /// existe en Firestore y está activo antes de permitir el ingreso.
        ///
        /// Flujo completo:
        ///   Firebase Auth → token → Firestore /usuarios/{uid}
        ///   → activo=true → SetSession → NavegaR
        /// </summary>
        private async Task AuthorizeAndNavigateAsync(AppUser authUser)
        {
            // Obtener token para firmar la petición a Firestore
            var token = await _authService.GetFreshTokenAsync();

            if (string.IsNullOrEmpty(token))
            {
                await _authService.SignOutAsync();
                SetError("No se pudo verificar la sesión. Intenta de nuevo.");
                return;
            }

            // Consultar Firestore
            var usuario = await _userRepository.GetByUidAsync(authUser.Uid, token);

            if (usuario is null)
            {
                // Autenticado en Firebase pero sin registro en el sistema
                await _authService.SignOutAsync();
                SetError("No tienes permiso para acceder a esta aplicación. " +
                         "Contacta al administrador.");
                return;
            }

            if (!usuario.Activo)
            {
                // El administrador desactivó la cuenta
                await _authService.SignOutAsync();
                SetError("Tu cuenta está desactivada. Contacta al administrador.");
                return;
            }

            // Acceso concedido — establecer sesión completa
            _session.SetSession(authUser, usuario);

            if (authUser.RequiresPasswordChange)
                await _navigation.GoToChangePasswordAsync();
            else
                _navigation.GoToMainApp();
        }

        private bool CanSignIn() =>
            !string.IsNullOrWhiteSpace(Email)    &&
            Email.Contains('@')                   &&
            Email.Contains('.')                   &&
            !string.IsNullOrWhiteSpace(Password)  &&
            Password.Length >= 6                  &&
            IsNotBusy;
    }
}
