using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models;
using Firebase.Auth;
using Foundation;
using DomainCode = EcoHuellaApp.Domain.Models.AuthErrorCode;
using FirebaseCode = Firebase.Auth.AuthErrorCode;

namespace EcoHuellaApp.Platforms.iOS
{
    /// <summary>
    /// IAuthService nativo para iOS usando Firebase Auth con correo y contraseña.
    /// </summary>
    public sealed class FirebaseAuthService : IAuthService
    {
        private readonly Auth _auth;

        public FirebaseAuthService()
        {
            _auth = Auth.DefaultInstance;
        }

        public AppUser? CurrentUser => MapToAppUser(_auth.CurrentUser);
        public bool HasActiveSession() => _auth.CurrentUser is not null;

        // ── Login email / contraseña ──────────────────────────────────────────

        public async Task<AuthResult> SignInWithEmailPasswordAsync(
            string email, string password)
        {
            try
            {
                var result = await _auth.SignInWithPasswordAsync(email, password);
                return MapUserResult(result.User);
            }
            catch (NSErrorException ex) { return MapNSError(ex.Error); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FirebaseAuth.iOS] SignIn: {ex}");
                return AuthResult.Fail("Error inesperado al iniciar sesión.", DomainCode.Unknown);
            }
        }

        public async Task<AuthResult> RegisterWithEmailPasswordAsync(string email, string password)
        {
            try
            {
                var result = await _auth.CreateUserAsync(email, password);
                Preferences.Default.Set($"first_login_{result.User.Uid}", false);
                return MapUserResult(result.User);
            }
            catch (NSErrorException ex) { return MapNSError(ex.Error); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FirebaseAuth.iOS] Register: {ex}");
                return AuthResult.Fail("No se pudo crear la cuenta.", DomainCode.Unknown);
            }
        }

        // ── Logout ────────────────────────────────────────────────────────────

        public Task<AuthResult> SignOutAsync()
        {
            _auth.SignOut(out var error);

            return Task.FromResult(
                error is null
                    ? AuthResult.Ok(AppUser.Empty)
                    : MapNSError(error));
        }

        // ── Cambio de contraseña ──────────────────────────────────────────────

        public async Task<AuthResult> UpdatePasswordAsync(string newPassword)
        {
            try
            {
                var user = _auth.CurrentUser;
                if (user is null)
                    return AuthResult.Fail(
                        "No hay usuario autenticado.", DomainCode.UserNotFound);

                await user.UpdatePasswordAsync(newPassword);

                Preferences.Default.Set($"first_login_{user.Uid}", false);
                return MapUserResult(_auth.CurrentUser);
            }
            catch (NSErrorException ex) { return MapNSError(ex.Error); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FirebaseAuth.iOS] UpdatePwd: {ex}");
                return AuthResult.Fail("Error al cambiar la contraseña.", DomainCode.Unknown);
            }
        }

        // ── Recuperación de contraseña ────────────────────────────────────────

        public async Task<AuthResult> SendPasswordResetEmailAsync(string email)
        {
            try
            {
                await _auth.SendPasswordResetAsync(email);
                return AuthResult.Ok(new AppUser { Email = email });
            }
            catch (NSErrorException ex) { return MapNSError(ex.Error); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FirebaseAuth.iOS] Reset: {ex}");
                return AuthResult.Fail(
                    "Error al enviar el correo de recuperación.", DomainCode.Unknown);
            }
        }

        // ── Token ─────────────────────────────────────────────────────────────

        public async Task<string?> GetFreshTokenAsync()
        {
            try
            {
                var user = _auth.CurrentUser;
                if (user is null) return null;
                var result = await user.GetIdTokenResultAsync(forceRefresh: false);
                return result?.Token;
            }
            catch { return null; }
        }

        // ── Mapeo de usuario ──────────────────────────────────────────────────

        private AuthResult MapUserResult(User? firebaseUser)
        {
            var user = MapToAppUser(firebaseUser);
            return user is not null
                ? AuthResult.Ok(user)
                : AuthResult.Fail(
                    "Error al obtener datos del usuario.", DomainCode.Unknown);
        }

        private static AppUser? MapToAppUser(User? u)
        {
            if (u is null) return null;

            var providers = u.ProviderData?
                .Select(p => p.ProviderId)
                .ToList()
                ?? [];

            // Solo los usuarios email/password tienen contraseña temporal del admin.
            // El indicador se conserva localmente después del primer acceso.
            bool isEmailPasswordUser = providers.Contains("password");
            var requiresChange = isEmailPasswordUser &&
                Preferences.Default.Get($"first_login_{u.Uid}", defaultValue: true);

            return new AppUser
            {
                Uid                    = u.Uid,
                Email                  = u.Email         ?? string.Empty,
                DisplayName            = u.DisplayName   ?? string.Empty,
                IsEmailVerified        = u.IsEmailVerified,
                RequiresPasswordChange = requiresChange,
                LinkedProviders        = providers.AsReadOnly()
            };
        }

        private static AuthResult MapNSError(NSError error)
        {
            var code = (FirebaseCode)(long)error.Code;
            return code switch
            {
                FirebaseCode.UserNotFound =>
                    AuthResult.Fail("No existe una cuenta con ese correo.",
                        DomainCode.UserNotFound),
                FirebaseCode.WrongPassword or FirebaseCode.InvalidCredential =>
                    AuthResult.Fail("Correo o contraseña incorrectos.",
                        DomainCode.InvalidCredentials),
                FirebaseCode.UserDisabled =>
                    AuthResult.Fail("Esta cuenta ha sido deshabilitada.",
                        DomainCode.UserDisabled),
                FirebaseCode.TooManyRequests =>
                    AuthResult.Fail("Demasiados intentos. Espera unos minutos.",
                        DomainCode.TooManyRequests),
                FirebaseCode.NetworkError =>
                    AuthResult.Fail("Sin conexión a internet.",
                        DomainCode.NetworkError),
                FirebaseCode.WeakPassword =>
                    AuthResult.Fail("La contraseña es demasiado débil.",
                        DomainCode.WeakPassword),
                FirebaseCode.RequiresRecentLogin =>
                    AuthResult.Fail("Inicia sesión nuevamente para continuar.",
                        DomainCode.RequiresRecentLogin),
                _ => AuthResult.Fail(
                        error.LocalizedDescription ?? "Error de autenticación.",
                        DomainCode.Unknown)
            };
        }
    }
}
