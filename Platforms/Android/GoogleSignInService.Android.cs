using Android.App;
using Android.Content;
using Android.Gms.Auth.Api.SignIn;
using Firebase.Auth;
using AGms = Android.Gms.Tasks;

namespace EcoHuellaApp.Platforms.Android
{
    /// <summary>
    /// Google Sign-In usando GoogleSignIn API legacy (Xamarin.GooglePlayServices.Auth).
    /// Más estable que Credential Manager para apps nuevas en Android 15+.
    /// Flujo: StartActivityForResult → OnActivityResult → Firebase credential.
    /// </summary>
    public static class GoogleSignInService
    {
        // Web Client ID tipo 3 (Web) — NO el tipo 1 (Android)
        // Firebase Console → Authentication → Google → Web client ID
        private const string WebClientId =
            "1063838909055-pupvkn7ci37p5p3a7p02pg7j6buh5c14.apps.googleusercontent.com";

        private static GoogleSignInClient? _client;
        private static TaskCompletionSource<AuthCredential?>? _tcs;

        public const int RequestCode = 9001;

        public static void Initialize(Activity activity)
        {
            var options = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                .RequestIdToken(WebClientId)
                .RequestEmail()
                .Build();

            _client = GoogleSignIn.GetClient(activity, options);
        }

        public static Task<AuthCredential?> SignInAsync(Activity activity)
        {
            if (_client is null)
                Initialize(activity);

            _tcs = new TaskCompletionSource<AuthCredential?>();

            // Forzar selección de cuenta cerrando sesión previa
            _client!.SignOut()
                .AddOnCompleteListener(new GsiCompleteListener(_ =>
                {
                    activity.StartActivityForResult(_client.SignInIntent, RequestCode);
                }));

            return _tcs.Task;
        }

        /// <summary>
        /// Llamar desde MainActivity.OnActivityResult().
        /// </summary>
        public static void HandleActivityResult(
            int requestCode, Result resultCode, Intent? data)
        {
            if (requestCode != RequestCode || _tcs is null) return;

            if (resultCode != Result.Ok)
            {
                _tcs.TrySetResult(null);
                return;
            }

            var innerTcs = new TaskCompletionSource<AuthCredential?>();

            GoogleSignIn.GetSignedInAccountFromIntent(data)
                .AddOnSuccessListener(new GsiSuccessListener<GoogleSignInAccount>(account =>
                {
                    try
                    {
                        if (string.IsNullOrEmpty(account?.IdToken))
                        {
                            innerTcs.TrySetException(
                                new Exception("Google Sign-In: IdToken vacío."));
                            return;
                        }
                        var credential = GoogleAuthProvider.GetCredential(account.IdToken, null);
                        innerTcs.TrySetResult(credential);
                    }
                    catch (Exception ex)
                    {
                        innerTcs.TrySetException(ex);
                    }
                }))
                .AddOnFailureListener(new GsiFailureListener(ex =>
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[GoogleSignIn] Failure: {ex.Message}");
                    innerTcs.TrySetException(
                        new Exception("Google Sign-In falló: " + ex.Message));
                }));

            innerTcs.Task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    _tcs.TrySetException(t.Exception!.InnerException ?? t.Exception);
                else
                    _tcs.TrySetResult(t.Result);
            });
        }

        public static async Task SignOutAsync(Activity? activity = null)
        {
            try
            {
                if (_client is null) return;
                var tcs = new TaskCompletionSource<bool>();
                _client.SignOut()
                    .AddOnCompleteListener(new GsiCompleteListener(_ =>
                        tcs.TrySetResult(true)));
                await tcs.Task;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GoogleSignIn] SignOut: {ex.Message}");
            }
        }
    }

    // ── Listeners privados al módulo Google Sign-In ───────────────────────────
    // Prefijo "Gsi" para evitar conflicto con FirebaseSuccessListener/FirebaseFailureListener

    internal sealed class GsiSuccessListener<T> : Java.Lang.Object, AGms.IOnSuccessListener
        where T : Java.Lang.Object
    {
        private readonly Action<T?> _action;
        public GsiSuccessListener(Action<T?> action) => _action = action;
        public void OnSuccess(Java.Lang.Object? result) => _action(result as T);
    }

    internal sealed class GsiFailureListener : Java.Lang.Object, AGms.IOnFailureListener
    {
        private readonly Action<Exception> _action;
        public GsiFailureListener(Action<Exception> action) => _action = action;
        public void OnFailure(Java.Lang.Exception e) => _action(e);
    }

    internal sealed class GsiCompleteListener : Java.Lang.Object, AGms.IOnCompleteListener
    {
        private readonly Action<AGms.Task> _action;
        public GsiCompleteListener(Action<AGms.Task> action) => _action = action;
        public void OnComplete(AGms.Task task) => _action(task);
    }
}