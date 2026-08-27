using System.Net.Http.Json;
using System.Text.Json;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models;

namespace EcoHuellaApp.Infrastructure.Services
{
    /// <summary>Autenticación Firebase para Windows.</summary>
    public sealed class FirebaseRestAuthService : IAuthService
    {
        // Firebase.

        private const string FirebaseApiKey =
            "AIzaSyAtg3FGiNg3cK4aMpup8_H1JUxcTx5Pj9Q";
        private const string FirebaseBaseUrl =
            "https://identitytoolkit.googleapis.com/v1/accounts";

        // Sesión.

        private static readonly HttpClient _http = new();
        private AppUser? _currentUser;
        private string?  _idToken;

        public AppUser? CurrentUser    => _currentUser;
        public bool HasActiveSession() => _currentUser is not null;

        // Acceso.

        public async Task<AuthResult> SignInWithEmailPasswordAsync(
            string email, string password)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync(
                    $"{FirebaseBaseUrl}:signInWithPassword?key={FirebaseApiKey}",
                    new { email, password, returnSecureToken = true });

                if (!resp.IsSuccessStatusCode)
                    return MapRestError(await ReadErrorAsync(resp));

                return BuildAuthResult(
                    await resp.Content.ReadFromJsonAsync<FirebaseSignInResponse>());
            }
            catch (HttpRequestException)
            {
                return AuthResult.Fail("Sin conexión a internet.", AuthErrorCode.NetworkError);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FirebaseRest] SignIn: {ex}");
                return AuthResult.Fail("Error inesperado.", AuthErrorCode.Unknown);
            }
        }

        public async Task<AuthResult> RegisterWithEmailPasswordAsync(string email, string password)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync(
                    $"{FirebaseBaseUrl}:signUp?key={FirebaseApiKey}",
                    new { email, password, returnSecureToken = true });

                if (!resp.IsSuccessStatusCode)
                    return MapRestError(await ReadErrorAsync(resp));

                var result = BuildAuthResult(
                    await resp.Content.ReadFromJsonAsync<FirebaseSignInResponse>());
                if (result.User is not null)
                    Preferences.Default.Set($"first_login_{result.User.Uid}", false);
                return result.IsSuccess && result.User is not null
                    ? AuthResult.Ok(result.User with { RequiresPasswordChange = false })
                    : result;
            }
            catch (HttpRequestException)
            {
                return AuthResult.Fail("Sin conexión a internet.", AuthErrorCode.NetworkError);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FirebaseRest] Register: {ex}");
                return AuthResult.Fail("No se pudo crear la cuenta.", AuthErrorCode.Unknown);
            }
        }

        // Cierre de sesión.

        public Task<AuthResult> SignOutAsync()
        {
            _currentUser = null;
            _idToken     = null;
            return Task.FromResult(AuthResult.Ok(AppUser.Empty));
        }

        // Cambio de contraseña.

        public async Task<AuthResult> UpdatePasswordAsync(string newPassword)
        {
            if (string.IsNullOrEmpty(_idToken))
                return AuthResult.Fail("No hay sesión activa.", AuthErrorCode.UserNotFound);

            try
            {
                var resp = await _http.PostAsJsonAsync(
                    $"{FirebaseBaseUrl}:update?key={FirebaseApiKey}",
                    new { idToken = _idToken, password = newPassword,
                          returnSecureToken = true });

                if (!resp.IsSuccessStatusCode)
                    return MapRestError(await ReadErrorAsync(resp));

                var data = await resp.Content
                    .ReadFromJsonAsync<FirebaseSignInResponse>();
                _idToken = data?.IdToken;

                if (_currentUser is not null)
                {
                    Preferences.Default.Set(
                        $"first_login_{_currentUser.Uid}", false);
                    _currentUser = _currentUser with { RequiresPasswordChange = false };
                }

                return AuthResult.Ok(_currentUser ?? AppUser.Empty);
            }
            catch (HttpRequestException)
            {
                return AuthResult.Fail("Sin conexión a internet.", AuthErrorCode.NetworkError);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FirebaseRest] UpdatePwd: {ex}");
                return AuthResult.Fail("Error al cambiar la contraseña.", AuthErrorCode.Unknown);
            }
        }

        // Recuperación.

        public async Task<AuthResult> SendPasswordResetEmailAsync(string email)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync(
                    $"{FirebaseBaseUrl}:sendOobCode?key={FirebaseApiKey}",
                    new { requestType = "PASSWORD_RESET", email });

                if (!resp.IsSuccessStatusCode)
                    return MapRestError(await ReadErrorAsync(resp));

                return AuthResult.Ok(new AppUser { Email = email });
            }
            catch (HttpRequestException)
            {
                return AuthResult.Fail("Sin conexión a internet.", AuthErrorCode.NetworkError);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FirebaseRest] Reset: {ex}");
                return AuthResult.Fail("Error al enviar el correo.", AuthErrorCode.Unknown);
            }
        }

        // Token.

        public Task<string?> GetFreshTokenAsync() => Task.FromResult(_idToken);

        // Usuario.

        private AuthResult BuildAuthResult(FirebaseSignInResponse? data)
        {
            if (data is null)
                return AuthResult.Fail(
                    "Respuesta inválida del servidor.", AuthErrorCode.Unknown);

            _idToken = data.IdToken;

            // La contraseña temporal aplica al acceso local.
            var requiresChange = data.LocalId is not null &&
                Preferences.Default.Get(
                    $"first_login_{data.LocalId}", defaultValue: true);

            _currentUser = new AppUser
            {
                Uid                    = data.LocalId     ?? string.Empty,
                Email                  = data.Email       ?? string.Empty,
                DisplayName            = data.DisplayName ?? string.Empty,
                IsEmailVerified        = data.Registered  ?? false,
                RequiresPasswordChange = requiresChange,
                LinkedProviders        = ["password"]
            };

            return AuthResult.Ok(_currentUser);
        }

        // Utilidades.

        private static async Task<string?> ReadErrorAsync(HttpResponseMessage resp)
        {
            try
            {
                var env = await resp.Content
                    .ReadFromJsonAsync<FirebaseErrorEnvelope>();
                return env?.Error?.Message;
            }
            catch { return null; }
        }

        private static AuthResult MapRestError(string? msg) => msg switch
        {
            "EMAIL_NOT_FOUND" or "INVALID_LOGIN_CREDENTIALS" =>
                AuthResult.Fail("Correo o contraseña incorrectos.",
                    AuthErrorCode.InvalidCredentials),
            "INVALID_PASSWORD" =>
                AuthResult.Fail("Contraseña incorrecta.",
                    AuthErrorCode.InvalidCredentials),
            "USER_DISABLED" =>
                AuthResult.Fail("Esta cuenta ha sido deshabilitada.",
                    AuthErrorCode.UserDisabled),
            "TOO_MANY_ATTEMPTS_TRY_LATER" =>
                AuthResult.Fail("Demasiados intentos. Espera unos minutos.",
                    AuthErrorCode.TooManyRequests),
            _ => AuthResult.Fail(
                    $"Error de autenticación: {msg}", AuthErrorCode.Unknown)
        };

        // Respuestas de Firebase.

        private sealed record FirebaseSignInResponse(
            string? IdToken, string? Email, string? LocalId,
            string? DisplayName, bool? Registered);

        private sealed record FirebaseErrorEnvelope(FirebaseError? Error);
        private sealed record FirebaseError(string? Message);
    }
}
