using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models;

namespace EcoHuellaApp.Infrastructure.Services
{
    /// <summary>
    /// Implementación simulada para MacCatalyst y desarrollo sin Firebase.
    /// Usuarios: admin@ecohuellaapp.com/Admin123! (primer login)
    ///           operador@ecohuellaapp.com/Oper123! (login normal)
    /// </summary>
    public sealed class FakeAuthService : IAuthService
    {
        private readonly Dictionary<string, FakeUserRecord> _users =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["admin@ecohuellaapp.com"] = new("fake-uid-001", "Admin123!", "Administrador", true),
                ["operador@ecohuellaapp.com"] = new("fake-uid-002", "Oper123!", "Operador", false)
            };

        private AppUser? _currentUser;

        public AppUser? CurrentUser => _currentUser;
        public bool HasActiveSession() => _currentUser is not null;

        public async Task<AuthResult> SignInWithEmailPasswordAsync(string email, string password)
        {
            await Task.Delay(800);

            if (!_users.TryGetValue(email, out var record))
                return AuthResult.Fail("No existe una cuenta con ese correo.", AuthErrorCode.UserNotFound);

            if (record.Password != password)
                return AuthResult.Fail("Correo o contraseña incorrectos.", AuthErrorCode.InvalidCredentials);

            _currentUser = MapUser(email, record);
            return AuthResult.Ok(_currentUser);
        }

        public Task<AuthResult> SignInWithGoogleAsync()
        {
            _currentUser = new AppUser
            {
                Uid = "fake-google-001",
                Email = "google@ecohuellaapp.com",
                DisplayName = "Usuario Google (Fake)",
                IsEmailVerified = true,
                RequiresPasswordChange = false,
                LinkedProviders = ["google.com"]
            };
            return Task.FromResult(AuthResult.Ok(_currentUser));
        }

        public async Task<AuthResult> SignOutAsync()
        {
            await Task.Delay(200);
            _currentUser = null;
            return AuthResult.Ok(AppUser.Empty);
        }

        public async Task<AuthResult> UpdatePasswordAsync(string newPassword)
        {
            await Task.Delay(600);

            if (_currentUser is null)
                return AuthResult.Fail("No hay usuario autenticado.", AuthErrorCode.UserNotFound);

            if (newPassword.Length < 8)
                return AuthResult.Fail("La contraseña debe tener al menos 8 caracteres.", AuthErrorCode.WeakPassword);

            if (_users.TryGetValue(_currentUser.Email, out var record))
                _users[_currentUser.Email] = record with { Password = newPassword, RequiresChange = false };

            _currentUser = _currentUser with { RequiresPasswordChange = false };
            return AuthResult.Ok(_currentUser);
        }

        public async Task<AuthResult> SendPasswordResetEmailAsync(string email)
        {
            await Task.Delay(500);
            return _users.ContainsKey(email)
                ? AuthResult.Ok(new AppUser { Email = email })
                : AuthResult.Fail("No existe una cuenta con ese correo.", AuthErrorCode.UserNotFound);
        }

        public Task<string?> GetFreshTokenAsync()
        {
            var token = _currentUser is not null
                ? $"fake.token.{_currentUser.Uid}.{DateTime.UtcNow.Ticks}"
                : null;
            return Task.FromResult(token);
        }

        private static AppUser MapUser(string email, FakeUserRecord r) => new()
        {
            Uid = r.Uid,
            Email = email,
            DisplayName = r.DisplayName,
            IsEmailVerified = true,
            RequiresPasswordChange = r.RequiresChange,
            LinkedProviders = ["password"]
        };
    }

    internal sealed record FakeUserRecord(
        string Uid,
        string Password,
        string DisplayName,
        bool RequiresChange);
}
