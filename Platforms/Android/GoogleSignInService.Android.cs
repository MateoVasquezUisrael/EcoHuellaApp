using Android.App;
using Android.Content;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common.Apis;
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
            System.Diagnostics.Debug.WriteLine(
                "[GoogleSignIn] Initialize called for activity: " + activity?.LocalClassName);

            var options = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                .RequestIdToken(WebClientId)
                .RequestEmail()
                .Build();

            _client = GoogleSignIn.GetClient(activity, options);
            System.Diagnostics.Debug.WriteLine("[GoogleSignIn] Client created with WebClientId.");
        }

        public static Task<AuthCredential?> SignInAsync(Activity activity)
        {
            System.Diagnostics.Debug.WriteLine(
                "[GoogleSignIn] SignInAsync called. Client is null: " + (_client is null));

            if (_client is null)
                Initialize(activity);

            _tcs = new TaskCompletionSource<AuthCredential?>();

            // Forzar selección de cuenta cerrando sesión previa
            _client!.SignOut()
                .AddOnCompleteListener(new GsiCompleteListener(_ =>
                {
                    System.Diagnostics.Debug.WriteLine(
                        "[GoogleSignIn] Starting SignInHubActivity with RequestCode " + RequestCode);
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
            System.Diagnostics.Debug.WriteLine(
                $"[GoogleSignIn] HandleActivityResult called. requestCode={requestCode}, expected={RequestCode}, resultCode={resultCode}, tcsIsNull={_tcs is null}, clientIsNull={_client is null}");

            if (requestCode != RequestCode || _tcs is null)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[GoogleSignIn] Ignoring activity result (wrong request code or no pending TCS).");
                return;
            }

            // Robustez: si el proceso fue destruido y recreado, el cliente puede ser null.
            if (_client is null && data is not null)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[GoogleSignIn] Client was null on result arrival. Reinitializing from current activity.");
                var activity = Platform.CurrentActivity;
                if (activity is not null)
                    Initialize(activity);
            }

            if (resultCode != Result.Ok)
            {
                // Intentar obtener el código de error exacto de Google Sign-In.
                string errorDetail = "ResultCode=" + resultCode;
                try
                {
                    var task = GoogleSignIn.GetSignedInAccountFromIntent(data);
                    if (task != null && task.Exception is ApiException apiEx)
                    {
                        int statusCode = apiEx.StatusCode;
                        string statusMessage = GetStatusCodeMessage(statusCode);
                        errorDetail = $"GoogleSignInStatusCode={statusCode} ({statusMessage})";
                    }
                    else if (task?.Exception != null)
                    {
                        errorDetail = $"Exception={task.Exception.GetType().Name}: {task.Exception.Message}";
                    }
                }
                catch (Exception ex)
                {
                    errorDetail += $"; Error reading status: {ex.GetType().Name}: {ex.Message}";
                }

                System.Diagnostics.Debug.WriteLine(
                    "[GoogleSignIn] Result was not OK. " + errorDetail);
                _tcs.TrySetException(new Exception($"Google Sign-In cancelado/error. {errorDetail}"));
                return;
            }

            var innerTcs = new TaskCompletionSource<AuthCredential?>();

            GoogleSignIn.GetSignedInAccountFromIntent(data)
                .AddOnSuccessListener(new GsiSuccessListener<GoogleSignInAccount>(account =>
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "[GoogleSignIn] Account retrieved. IdToken empty: " + string.IsNullOrEmpty(account?.IdToken));

                        if (string.IsNullOrEmpty(account?.IdToken))
                        {
                            innerTcs.TrySetException(
                                new Exception("Google Sign-In: IdToken vacío."));
                            return;
                        }
                        var credential = GoogleAuthProvider.GetCredential(account.IdToken, null);
                        System.Diagnostics.Debug.WriteLine("[GoogleSignIn] Firebase credential created.");
                        innerTcs.TrySetResult(credential);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("[GoogleSignIn] Exception in success handler: " + ex);
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
                {
                    System.Diagnostics.Debug.WriteLine(
                        "[GoogleSignIn] innerTcs faulted: " + t.Exception?.InnerException?.Message);
                    _tcs.TrySetException(t.Exception!.InnerException ?? t.Exception);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(
                        "[GoogleSignIn] innerTcs completed. Credential is null: " + (t.Result is null));
                    _tcs.TrySetResult(t.Result);
                }
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

        private static string GetStatusCodeMessage(int statusCode)
        {
            // Códigos de error comunes de Google Sign-In.
            // Ver: https://developers.google.com/android/reference/com/google/android/gms/auth/api/signin/GoogleSignInStatusCodes
            return statusCode switch
            {
                12500 => "SIGN_IN_FAILED",
                12501 => "SIGN_IN_CANCELLED",
                12502 => "SIGN_IN_CURRENTLY_IN_PROGRESS",
                10    => "DEVELOPER_ERROR",
                8     => "INTERNAL_ERROR",
                7     => "NETWORK_ERROR",
                16    => "API_NOT_CONNECTED",
                22    => "TIMEOUT",
                _     => $"UNKNOWN_CODE_{statusCode}"
            };
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