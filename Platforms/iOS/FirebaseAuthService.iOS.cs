using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models;
using Firebase.Auth;
using Foundation;
using Google.SignIn;
using GIDSignIn = Google.SignIn.SignIn;
using DomainCode = EcoHuellaApp.Domain.Models.AuthErrorCode;
using FirebaseCode = Firebase.Auth.AuthErrorCode;


namespace EcoHuellaApp.Platforms.iOS
{
    /// <summary>
    /// IAuthService nativo para iOS usando Firebase Auth SDK + Google Sign-In SDK.
    ///
    /// Google Sign-In: usa AdamE.Google.iOS.SignIn (GIDSignIn) que abre
    /// ASWebAuthenticationSession internamente. El REVERSED_CLIENT_ID registrado
    /// en Info.plist permite que Safari redirija de vuelta a la app.
    ///
    /// Requiere en Info.plist (ya configurado):
    ///   CFBundleURLSchemes = REVERSED_CLIENT_ID de GoogleService-Info.plist
    /// </summary>
    public sealed class FirebaseAuthService : IAuthService
    {
        // CLIENT_ID del GoogleService-Info.plist (cliente OAuth nativo de iOS)
        private const string iOSClientId =
            "1063838909055-cbho8glot83btdjrq0l4j54f3ac04bq6.apps.googleusercontent.com";

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

        // ── Google Sign-In ────────────────────────────────────────────────────

        /// <summary>
        /// Usa GIDSignIn (Google Sign-In iOS SDK) para obtener el id_token y
        /// access_token de Google, y luego autenticar en Firebase con ellos.
        /// </summary>
        public Task<AuthResult> SignInWithGoogleAsync()
        {
            var tcs = new TaskCompletionSource<AuthResult>();

            try
            {
                var topVc = Platform.GetCurrentUIViewController();
                if (topVc is null)
                    return Task.FromResult(AuthResult.Fail(
                        "No hay UIViewController activo.", DomainCode.Unknown));

                // Google Sign-In SDK v9: la configuración se asigna a SharedInstance
                // y el sign-in se inicia solo con el viewController presentador.
                GIDSignIn.SharedInstance.Configuration =
                    new Configuration(iOSClientId);

                // GIDSignIn abre ASWebAuthenticationSession.
                // El callback llega en el hilo principal.
                GIDSignIn.SharedInstance.SignInWithPresentingViewController(
                    topVc,
                    async (signInResult, nsError) =>
                    {
                        if (nsError is not null)
                        {
                            // GIDSignInErrorCodeCanceled = -5
                            var code = (long)nsError.Code;
                            tcs.TrySetResult(code == -5
                                ? AuthResult.Fail(string.Empty, DomainCode.Cancelled)
                                : MapNSError(nsError));
                            return;
                        }

                        try
                        {
                            var idToken     = signInResult?.User?.IdToken?.TokenString;
                            var accessToken = signInResult?.User?.AccessToken?.TokenString;

                            if (string.IsNullOrEmpty(idToken) ||
                                string.IsNullOrEmpty(accessToken))
                            {
                                tcs.TrySetResult(AuthResult.Fail(
                                    "Google no devolvió tokens válidos.",
                                    DomainCode.Unknown));
                                return;
                            }

                            // Crear credencial Firebase a partir de los tokens de Google
                            var credential = GoogleAuthProvider.GetCredential(
                                idToken, accessToken);

                            var firebaseResult =
                                await _auth.SignInWithCredentialAsync(credential);

                            tcs.TrySetResult(MapUserResult(firebaseResult.User));
                        }
                        catch (NSErrorException ex)
                        {
                            tcs.TrySetResult(MapNSError(ex.Error));
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"[FirebaseAuth.iOS] GoogleCredential: {ex}");
                            tcs.TrySetResult(AuthResult.Fail(
                                "Error al autenticar con Firebase.",
                                DomainCode.Unknown));
                        }
                    });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[FirebaseAuth.iOS] GoogleSignIn: {ex}");
                tcs.TrySetResult(AuthResult.Fail(
                    "Error al iniciar sesión con Google.", DomainCode.Unknown));
            }

            return tcs.Task;
        }

        // ── Logout ────────────────────────────────────────────────────────────

        public Task<AuthResult> SignOutAsync()
        {
            _auth.SignOut(out var error);

            // Cerrar también la sesión de Google para que el picker
            // vuelva a mostrarse en el próximo inicio de sesión
            GIDSignIn.SharedInstance.SignOutUser();

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
            // Usuarios de Google nunca necesitan cambiar contraseña.
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
