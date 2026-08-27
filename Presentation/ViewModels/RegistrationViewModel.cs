using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models;

namespace EcoHuellaApp.Presentation.ViewModels;

public partial class RegistrationViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionService _session;
    private readonly INavigationService _navigation;
    private readonly IMockPasswordService _mockPassword;

    [ObservableProperty] private string _fullName = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowPasswordMismatch))]
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowPasswordMismatch))]
    private string _confirmPassword = string.Empty;
    [ObservableProperty] private string _organization = string.Empty;

    public bool ShowPasswordMismatch =>
        !string.IsNullOrEmpty(ConfirmPassword) &&
        !string.Equals(Password, ConfirmPassword, StringComparison.Ordinal);

    public RegistrationViewModel(IAuthService authService, IUserRepository userRepository,
        IUserSessionService session, INavigationService navigation,
        IMockPasswordService mockPassword)
    {
        _authService = authService;
        _userRepository = userRepository;
        _session = session;
        _navigation = navigation;
        _mockPassword = mockPassword;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        ClearError();
        if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            SetError("Completa todos los campos obligatorios.");
            return;
        }
        if (!Email.Contains('@') || !Email.Contains('.'))
        {
            SetError("Ingresa un correo electrónico válido.");
            return;
        }
        if (Password.Length < 8)
        {
            SetError("La contraseña debe tener al menos 8 caracteres.");
            return;
        }
        if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
        {
            SetError("Las contraseñas no coinciden.");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var result = await _authService.RegisterWithEmailPasswordAsync(
                Email.Trim().ToLowerInvariant(), Password);
            Password = ConfirmPassword = string.Empty;
            if (!result.IsSuccess)
            {
                SetError(result.ErrorMessage ?? "No se pudo crear la cuenta.");
                return;
            }

            var token = await _authService.GetFreshTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                await _authService.SignOutAsync();
                SetError("No se pudo verificar la cuenta creada.");
                return;
            }

            var authUser = result.User!;
            var usuario = new UsuarioSistema
            {
                Uid = authUser.Uid,
                Email = authUser.Email,
                Nombre = FullName.Trim(),
                Organizacion = Organization.Trim(),
                Rol = RolSistema.Usuario,
                Activo = true
            };
            if (!await _userRepository.CreateAsync(usuario, token))
            {
                await _authService.SignOutAsync();
                SetError("La cuenta fue creada, pero no se pudo guardar el perfil en Firestore.");
                return;
            }

            _session.SetSession(authUser, usuario);
            _mockPassword.TrackUser(authUser);
            _navigation.GoToMainApp();
        });
    }

    [RelayCommand]
    private Task BackToLoginAsync() => _navigation.GoBackAsync();
}
