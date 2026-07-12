using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models;
using Firebase;
using Firebase.Auth;
using AGms = Android.Gms.Tasks;

namespace EcoHuellaApp.Platforms.Android
{
    public sealed class FirebaseAuthService : IAuthService
    {
        private readonly FirebaseAuth _auth;

        public FirebaseAuthService()
        {
            _auth = FirebaseAuth.Instance;
        }

        public AppUser? CurrentUser => MapToAppUser(_auth.CurrentUser);
        public bool HasActiveSession() => _auth.CurrentUser is not null;

        // Login email/contraseña

        public async Task<AuthResult> SignInWithEmailPasswordAsync(string email, string password)
        {
            try
            {
                await RunFirebaseTask(
                    _auth.SignInWithEmailAndPassword(email, password));

                return MapUserResult(_auth.CurrentUser);
            }
            catch (FirebaseAuthInvalidUserException)
            {
                return AuthResult.Fail("No existe una cuenta con ese correo.", AuthErrorCode.UserNotFound);
            }
            catch (FirebaseAuthInvalidCredentialsException)
            {
                return AuthResult.Fail("Correo o contraseña incorrectos.", AuthErrorCode.InvalidCredentials);
            }
            catch (FirebaseException ex) when (ex.Message?.Contains("TOO_MANY_ATTEMPTS") == true)
            {
                return AuthResult.Fail("Demasiados intentos. Espera unos minutos.", AuthErrorCode.TooManyRequests);
            }
            catch (Java.IO.IOException)
            {
                return AuthResult.Fail("Sin conexión a internet.", AuthErrorCode.NetworkError);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[FirebaseAuth.Android] SignIn: " + ex);
                return AuthResult.Fail("Error inesperado al iniciar sesión.", AuthErrorCode.Unknown);
            }
        }

        // Google Sign-In 

        public async Task<AuthResult> SignInWithGoogleAsync()
        {
            try
            {
                var activity = Platform.CurrentActivity
                    ?? throw new InvalidOperationException("No hay Activity activa.");

                System.Diagnostics.Debug.WriteLine(
                    "[FirebaseAuth.Android] SignInWithGoogleAsync started. Activity: " + activity.LocalClassName);

                // null = usuario cancela explícitamente  sin error
                // GoogleSignInException = error real (config, red, etc.)
                var credential = await GoogleSignInService.SignInAsync(activity);

                System.Diagnostics.Debug.WriteLine(
                    "[FirebaseAuth.Android] Credential obtained. IsNull: " + (credential is null));

                if (credential is null)
                    return AuthResult.Fail(string.Empty, AuthErrorCode.Cancelled);

                System.Diagnostics.Debug.WriteLine(
                    "[FirebaseAuth.Android] Calling Firebase SignInWithCredential...");
                await RunFirebaseTask(
                    _auth.SignInWithCredential(credential));

                System.Diagnostics.Debug.WriteLine(
                    "[FirebaseAuth.Android] Firebase sign-in completed. CurrentUser is null: " + (_auth.CurrentUser is null));

                return MapUserResult(_auth.CurrentUser);
            }
            
            catch (FirebaseAuthInvalidCredentialsException)
            {
                System.Diagnostics.Debug.WriteLine("[FirebaseAuth.Android] FirebaseAuthInvalidCredentialsException");
                return AuthResult.Fail("Credencial de Google inválida.", AuthErrorCode.InvalidCredentials);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[FirebaseAuth.Android] GoogleSignIn exception: " + ex);
                return AuthResult.Fail("Error inesperado al iniciar sesión con Google.", AuthErrorCode.Unknown);
            }
        }

        public async Task<AuthResult> SignOutAsync()
        {
            try
            {
                _auth.SignOut();
                await GoogleSignInService.SignOutAsync(
                    Platform.CurrentActivity as global::Android.App.Activity);
                return AuthResult.Ok(AppUser.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[FirebaseAuth.Android] SignOut: " + ex);
                return AuthResult.Fail("Error al cerrar sesión.", AuthErrorCode.Unknown);
            }
        }

        // Cambio de contraseña (primer login)

        public async Task<AuthResult> UpdatePasswordAsync(string newPassword)
        {
            try
            {
                var user = _auth.CurrentUser;
                if (user is null)
                    return AuthResult.Fail("No hay usuario autenticado.", AuthErrorCode.UserNotFound);

                await RunFirebaseTask(user.UpdatePassword(newPassword));

                // Marcar localmente que el primer login fue completado
                Preferences.Default.Set($"first_login_{user.Uid}", false);

                return MapUserResult(_auth.CurrentUser);
            }
            catch (FirebaseAuthRecentLoginRequiredException)
            {
                return AuthResult.Fail("Inicia sesión nuevamente para continuar.", AuthErrorCode.RequiresRecentLogin);
            }
            catch (FirebaseAuthWeakPasswordException)
            {
                return AuthResult.Fail("La contraseña debe tener al menos 6 caracteres.", AuthErrorCode.WeakPassword);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[FirebaseAuth.Android] UpdatePwd: " + ex);
                return AuthResult.Fail("Error al cambiar la contraseña.", AuthErrorCode.Unknown);
            }
        }

        // Recuperación de contraseña 

        public async Task<AuthResult> SendPasswordResetEmailAsync(string email)
        {
            try
            {
                await RunFirebaseTask(_auth.SendPasswordResetEmail(email));
                return AuthResult.Ok(new AppUser { Email = email });
            }
            catch (FirebaseAuthInvalidUserException)
            {
                return AuthResult.Fail("No existe una cuenta con ese correo.", AuthErrorCode.UserNotFound);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[FirebaseAuth.Android] Reset: " + ex);
                return AuthResult.Fail("Error al enviar el correo de recuperación.", AuthErrorCode.Unknown);
            }
        }

        // Token 

        public async Task<string?> GetFreshTokenAsync()
        {
            try
            {
                var user = _auth.CurrentUser;
                if (user is null) return null;

                var tcs = new TaskCompletionSource<Java.Lang.Object?>();
                user.GetIdToken(false)
                    .AddOnSuccessListener(new SuccessListener(tcs.SetResult))
                    .AddOnFailureListener(new FailureListener(_ => tcs.SetResult(null)));

                return (await tcs.Task as GetTokenResult)?.Token;
            }
            catch
            {
                return null;
            }
        }

        // Helpers 

        /// <summary>Adapta una Task de Firebase (Java) a Task de C#.</summary>
        private static Task RunFirebaseTask(global::Android.Gms.Tasks.Task task)
        {
            var tcs = new TaskCompletionSource<Java.Lang.Object?>();
            task.AddOnSuccessListener(new SuccessListener(tcs.SetResult))
                .AddOnFailureListener(new FailureListener(tcs.SetException));
            return tcs.Task;
        }

        private AuthResult MapUserResult(FirebaseUser? firebaseUser)
        {
            var user = MapToAppUser(firebaseUser);
            return user is not null
                ? AuthResult.Ok(user)
                : AuthResult.Fail("Error al obtener datos del usuario.", AuthErrorCode.Unknown);
        }

        private static AppUser? MapToAppUser(FirebaseUser? u)
        {
            if (u is null) return null;

            var providers = u.ProviderData?
                .Select(p => p.ProviderId)
                .ToList()
                ?? [];

            // Solo los usuarios de email/password tienen contraseña temporal del admin.
            // Los usuarios de Google/Facebook no necesitan cambiar contraseña.
            bool isEmailPasswordUser = providers.Contains("password");
            var requiresChange = isEmailPasswordUser &&
                Preferences.Default.Get($"first_login_{u.Uid}", defaultValue: true);

            return new AppUser
            {
                Uid = u.Uid,
                Email = u.Email ?? string.Empty,
                DisplayName = u.DisplayName ?? string.Empty,
                IsEmailVerified = u.IsEmailVerified,
                RequiresPasswordChange = requiresChange,
                LinkedProviders = providers.AsReadOnly()
            };
        }
    }

    //Helpers Firebase para TaskCompletionSource 

    internal sealed class SuccessListener : Java.Lang.Object, AGms.IOnSuccessListener
    {
        private readonly Action<Java.Lang.Object?> _onSuccess;
        public SuccessListener(Action<Java.Lang.Object?> onSuccess) => _onSuccess = onSuccess;
        public void OnSuccess(Java.Lang.Object? result) => _onSuccess(result);
    }

    internal sealed class FailureListener : Java.Lang.Object, AGms.IOnFailureListener
    {
        private readonly Action<Exception> _onFailure;
        public FailureListener(Action<Exception> onFailure) => _onFailure = onFailure;
        public void OnFailure(Java.Lang.Exception e) => _onFailure(e);
    }
}
