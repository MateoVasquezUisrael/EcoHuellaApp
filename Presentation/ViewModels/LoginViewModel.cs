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
        private readonly IMockPasswordService _mockPassword;

        public LoginViewModel(
            IAuthService        authService,
            IUserRepository     userRepository,
            IUserSessionService session,
            INavigationService  navigation,
            IMockPasswordService mockPassword)
        {
            _authService    = authService;
            _userRepository = userRepository;
            _session        = session;
            _navigation     = navigation;
            _mockPassword   = mockPassword;
        }

        // Formulario.

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

        // Acceso.

        [RelayCommand(CanExecute = nameof(CanSignIn))]
        private async Task SignInAsync()
        {
            await ExecuteAsync(async () =>
            {
                SuccessMessage = string.Empty;

                var normalizedEmail = Email.Trim().ToLowerInvariant();
                var mockUser = _mockPassword.TrySignIn(normalizedEmail, Password);
                if (mockUser is not null)
                {
                    Password = string.Empty;
                    await AuthorizeMockUserAsync(mockUser);
                    return;
                }

                var result = await _authService.SignInWithEmailPasswordAsync(
                    normalizedEmail, Password);

                Password = string.Empty; // limpiar de memoria

                if (!result.IsSuccess)
                {
                    SetError(result.ErrorMessage ?? "Error al iniciar sesión.");
                    return;
                }

                await AuthorizeAndNavigateAsync(result.User!);
            });
        }

        // Recuperación.

        [RelayCommand]
        private async Task ForgotPasswordAsync()
        {
            Page? page = Application.Current?.Windows[0].Page is NavigationPage np
                ? np.CurrentPage
                : Application.Current?.Windows[0].Page;

            if (page is null) return;

            var email = await page.DisplayPromptAsync(
                title:       "Recuperar contraseña",
                message:     "Ingresa el correo de tu cuenta para generar una contraseña temporal de demostración.",
                accept:      "Generar",
                cancel:      "Cancelar",
                placeholder: "correo@ejemplo.com",
                initialValue: Email.Trim(),
                keyboard:    Keyboard.Email);

            if (string.IsNullOrWhiteSpace(email)) return;

            email = email.Trim().ToLowerInvariant();
            if (!email.Contains('@') || !email.Contains('.'))
            {
                await page.DisplayAlertAsync(
                    "Correo no válido",
                    "Escribe una dirección de correo válida para continuar.",
                    "Aceptar");
                return;
            }

            try
            {
                var temporaryPassword = _mockPassword.GenerateTemporaryPassword(email);
                SuccessMessage = "Contraseña temporal generada. Úsala para iniciar sesión.";
                ClearError();
                await page.DisplayAlertAsync(
                    "Contraseña temporal • Mockup",
                    $"Tu contraseña temporal es:\n\n{temporaryPassword}\n\nInicia sesión con ella. La aplicación te pedirá crear una contraseña nueva inmediatamente.",
                    "Entendido");
            }
            catch (Exception ex)
            {
                await page.DisplayAlertAsync("Error", $"No se pudo generar la contraseña temporal: {ex.Message}", "Aceptar");
            }
        }

        // Visibilidad.

        [RelayCommand]
        private void TogglePasswordVisibility() =>
            IsPasswordVisible = !IsPasswordVisible;

        [RelayCommand]
        private Task NewUserAsync() => _navigation.GoToRegistrationAsync();

        [RelayCommand]
        private void SignInAsGuest()
        {
            var guestUser = new AppUser
            {
                Uid = $"guest-{Guid.NewGuid():N}",
                Email = "invitado.local@ecohuella.app",
                DisplayName = "Invitado",
                Role = "Invitado",
                IsEmailVerified = true,
                RequiresPasswordChange = false,
                LinkedProviders = ["guest"]
            };

            var guestProfile = new UsuarioSistema
            {
                Uid = guestUser.Uid,
                Email = guestUser.Email,
                Nombre = "Invitado",
                Rol = RolSistema.Usuario,
                Activo = true
            };

            _session.SetSession(guestUser, guestProfile);
            _navigation.GoToGuestDemo();
        }

        // Autorización y navegación.

        /// <summary>Valida el perfil y abre la aplicación.</summary>
        private async Task AuthorizeAndNavigateAsync(AppUser authUser)
        {
            // Obtiene el token.
            var token = await _authService.GetFreshTokenAsync();

            if (string.IsNullOrEmpty(token))
            {
                await _authService.SignOutAsync();
                SetError("No se pudo verificar la sesión. Intenta de nuevo.");
                return;
            }

            // Consulta el perfil.
            var usuario = await _userRepository.GetByUidAsync(authUser.Uid, token);

            if (usuario is null)
            {
                // Recupera perfiles pendientes.
                usuario = new UsuarioSistema
                {
                    Uid = authUser.Uid,
                    Email = authUser.Email,
                    Nombre = string.IsNullOrWhiteSpace(authUser.DisplayName)
                        ? authUser.Email.Split('@')[0]
                        : authUser.DisplayName,
                    Rol = RolSistema.Usuario,
                    Activo = true
                };

                if (!await _userRepository.CreateAsync(usuario, token))
                {
                    await _authService.SignOutAsync();
                    SetError("La cuenta es válida, pero no se pudo preparar el perfil local.");
                    return;
                }
            }

            if (!usuario.Activo)
            {
                // Cuenta desactivada.
                await _authService.SignOutAsync();
                SetError("Tu cuenta está desactivada. Contacta al administrador.");
                return;
            }

            // Acceso concedido.
            _session.SetSession(authUser, usuario);
            _mockPassword.TrackUser(authUser);

            if (authUser.RequiresPasswordChange)
                await _navigation.GoToChangePasswordAsync();
            else
                _navigation.GoToMainApp();
        }

        private async Task AuthorizeMockUserAsync(AppUser mockUser)
        {
            var usuario = await _userRepository.GetByUidAsync(mockUser.Uid, "mock-local");
            usuario ??= new UsuarioSistema
            {
                Uid = mockUser.Uid,
                Email = mockUser.Email,
                Nombre = mockUser.DisplayName,
                Rol = RolSistema.Usuario,
                Activo = true
            };

            await _userRepository.CreateAsync(usuario, "mock-local");
            _session.SetSession(mockUser, usuario);
            if (mockUser.RequiresPasswordChange)
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
