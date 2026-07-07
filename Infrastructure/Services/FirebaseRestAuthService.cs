using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models;

namespace EcoHuellaApp.Infrastructure.Services
{
    /// <summary>
    /// IAuthService para Windows usando Firebase Identity Toolkit REST API v1.
    ///
    /// Google Sign-In: Authorization Code + PKCE (RFC 7636).
    /// Los clientes OAuth de tipo "Aplicación de escritorio" NO usan client_secret;
    /// el code_verifier es la prueba de autenticidad del caller.
    /// HttpListener escucha en un puerto dinámico de localhost para el callback OAuth.
    /// </summary>
    public sealed class FirebaseRestAuthService : IAuthService
    {
        // ── Firebase ──────────────────────────────────────────────────────────

        private const string FirebaseApiKey =
            "AIzaSyAtg3FGiNg3cK4aMpup8_H1JUxcTx5Pj9Q";
        private const string FirebaseBaseUrl =
            "https://identitytoolkit.googleapis.com/v1/accounts";

        // ── Google OAuth ──────────────────────────────────────────────────────

        /// <summary>
        /// Client ID del cliente OAuth tipo "Aplicación de escritorio".
        /// Google Cloud Console → APIs y servicios → Credenciales → EcoHuellaApp Desktop.
        /// Los clientes de escritorio permiten http://127.0.0.1 automáticamente;
        /// NO se requiere client_secret cuando se usa PKCE.
        /// </summary>
        private const string DesktopClientId =
            "1063838909055-6438an0trotnc6b2hfb75auge7drodrk.apps.googleusercontent.com";

        private const string DesktopClientSecret =
          "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";

        private const string GoogleTokenUrl =
            "https://oauth2.googleapis.com/token";
        private const string GoogleAuthUrl  =
            "https://accounts.google.com/o/oauth2/v2/auth";

        // Tiempo máximo que el usuario tiene para completar el OAuth en el navegador
        private static readonly TimeSpan OAuthTimeout = TimeSpan.FromMinutes(3);

        // ── Estado en memoria ─────────────────────────────────────────────────

        private static readonly HttpClient _http = new();
        private AppUser? _currentUser;
        private string?  _idToken;

        public AppUser? CurrentUser    => _currentUser;
        public bool HasActiveSession() => _currentUser is not null;

        // ── Login email / contraseña ──────────────────────────────────────────

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
                    await resp.Content.ReadFromJsonAsync<FirebaseSignInResponse>(),
                    isGoogleUser: false);
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

        // ── Google Sign-In (PKCE + HttpListener local) ────────────────────────

        public async Task<AuthResult> SignInWithGoogleAsync()
        {
            // Puerto dinámico en rango no privilegiado para evitar conflictos
            var port     = GetAvailablePort();
            var callback = $"http://127.0.0.1:{port}/";

            using var cts      = new CancellationTokenSource(OAuthTimeout);
            using var listener = new HttpListener();

            try
            {
                // 1. PKCE: el code_verifier reemplaza al client_secret para Desktop apps
                var verifier  = GeneratePkceVerifier();
                var challenge = GeneratePkceChallenge(verifier);
                var state     = Guid.NewGuid().ToString("N");

                // 2. URL de autorización
                var authUrl =
                    $"{GoogleAuthUrl}" +
                    $"?client_id={Uri.EscapeDataString(DesktopClientId)}" +
                    $"&redirect_uri={Uri.EscapeDataString(callback)}" +
                    $"&response_type=code" +
                    $"&scope=openid%20email%20profile" +
                    $"&code_challenge={Uri.EscapeDataString(challenge)}" +
                    $"&code_challenge_method=S256" +
                    $"&state={Uri.EscapeDataString(state)}";

                // 3. Iniciar listener ANTES de abrir el navegador
                listener.Prefixes.Add(callback);
                listener.Start();

                // 4. Abrir el navegador del sistema con la URL de Google
                await Launcher.Default.OpenAsync(new Uri(authUrl));

                // 5. Esperar el callback con timeout.
                //    Si el usuario cierra el navegador, el CTS cancela en OAuthTimeout.
                HttpListenerContext context;
                try
                {
                    var getContextTask = listener.GetContextAsync();
                    var completedTask  = await Task.WhenAny(
                        getContextTask,
                        Task.Delay(OAuthTimeout, cts.Token));

                    if (completedTask != getContextTask)
                    {
                        // Timeout o cancelación
                        return AuthResult.Fail(string.Empty, AuthErrorCode.Cancelled);
                    }

                    context = await getContextTask;
                }
                catch (OperationCanceledException)
                {
                    return AuthResult.Fail(string.Empty, AuthErrorCode.Cancelled);
                }

                // 6. Responder al navegador para que pueda cerrarse
                await SendBrowserCloseResponseAsync(context);

                // 7. Extraer y validar parámetros del callback
                var query         = HttpUtility.ParseQueryString(
                    context.Request.Url?.Query ?? string.Empty);
                var code           = query["code"];
                var returnedState  = query["state"];

                if (string.IsNullOrWhiteSpace(code))
                    return AuthResult.Fail(
                        "No se obtuvo el código de autorización.", AuthErrorCode.Unknown);

                if (returnedState != state)
                    return AuthResult.Fail(
                        "Error de seguridad OAuth: state inválido.", AuthErrorCode.Unknown);

                // 8. Canjear código por id_token (sin client_secret — PKCE es suficiente)
                var idToken = await ExchangeCodeForIdTokenAsync(code, verifier, callback);

                if (string.IsNullOrEmpty(idToken))
                    return AuthResult.Fail(
                        "No se pudo obtener el token de Google.", AuthErrorCode.Unknown);

                // 9. Autenticar en Firebase con el id_token de Google
                return await SignInWithGoogleTokenAsync(idToken);
            }
            catch (TaskCanceledException)
            {
                return AuthResult.Fail(string.Empty, AuthErrorCode.Cancelled);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FirebaseRest] GoogleSignIn: {ex}");
                return AuthResult.Fail(
                    "Error al iniciar sesión con Google.", AuthErrorCode.Unknown);
            }
            finally
            {
                // Garantizar que el listener siempre se detiene
                if (listener.IsListening) listener.Stop();
            }
        }

        // ── Logout ────────────────────────────────────────────────────────────

        public Task<AuthResult> SignOutAsync()
        {
            _currentUser = null;
            _idToken     = null;
            return Task.FromResult(AuthResult.Ok(AppUser.Empty));
        }

        // ── Cambio de contraseña ──────────────────────────────────────────────

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

        // ── Recuperación de contraseña ────────────────────────────────────────

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

        // ── Token ─────────────────────────────────────────────────────────────

        public Task<string?> GetFreshTokenAsync() => Task.FromResult(_idToken);

        // ── PKCE ─────────────────────────────────────────────────────────────

        private static string GeneratePkceVerifier()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Base64UrlEncode(bytes);
        }

        private static string GeneratePkceChallenge(string verifier)
        {
            var hash = SHA256.HashData(Encoding.ASCII.GetBytes(verifier));
            return Base64UrlEncode(hash);
        }

        private static string Base64UrlEncode(byte[] bytes) =>
            Convert.ToBase64String(bytes)
                .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        // ── OAuth ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Canjea el código OAuth por un id_token de Google.
        /// Desktop apps + PKCE: Google no requiere client_secret.
        /// El code_verifier es la prueba criptográfica de que quien pide el token
        /// es el mismo que generó el code_challenge original.
        /// </summary>
        private async Task<string?> ExchangeCodeForIdTokenAsync(
            string code, string verifier, string redirectUri)
        {
            var body = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"]          = code,
                ["client_id"]     = DesktopClientId,
                ["client_secret"] = DesktopClientSecret,// Sin client_secret: PKCE para Desktop es un flujo de cliente público
                ["redirect_uri"]  = redirectUri,
                ["grant_type"]    = "authorization_code",
                ["code_verifier"] = verifier
            });

            var resp = await _http.PostAsync(GoogleTokenUrl, body);
            if (!resp.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[FirebaseRest] TokenExchange: {await resp.Content.ReadAsStringAsync()}");
                return null;
            }

            using var doc = JsonDocument.Parse(
                await resp.Content.ReadAsStringAsync());

            return doc.RootElement.TryGetProperty("id_token", out var t)
                ? t.GetString() : null;
        }

        private async Task<AuthResult> SignInWithGoogleTokenAsync(string googleIdToken)
        {
            var postBody = $"id_token={Uri.EscapeDataString(googleIdToken)}" +
                           "&providerId=google.com";

            var resp = await _http.PostAsJsonAsync(
                $"{FirebaseBaseUrl}:signInWithIdp?key={FirebaseApiKey}",
                new
                {
                    postBody,
                    requestUri          = "http://localhost",
                    returnIdpCredential = true,
                    returnSecureToken   = true
                });

            if (!resp.IsSuccessStatusCode)
                return MapRestError(await ReadErrorAsync(resp));

            // Google Sign-In → marcar proveedor correcto para evitar RequiresPasswordChange
            return BuildAuthResult(
                await resp.Content.ReadFromJsonAsync<FirebaseSignInResponse>(),
                isGoogleUser: true);
        }

        // ── Construcción de AppUser ───────────────────────────────────────────

        private AuthResult BuildAuthResult(
            FirebaseSignInResponse? data, bool isGoogleUser)
        {
            if (data is null)
                return AuthResult.Fail(
                    "Respuesta inválida del servidor.", AuthErrorCode.Unknown);

            _idToken = data.IdToken;

            // Solo los usuarios email/password tienen contraseña temporal del admin.
            // Coherente con la lógica de MapToAppUser en Android e iOS.
            var requiresChange = !isGoogleUser && data.LocalId is not null &&
                Preferences.Default.Get(
                    $"first_login_{data.LocalId}", defaultValue: true);

            _currentUser = new AppUser
            {
                Uid                    = data.LocalId     ?? string.Empty,
                Email                  = data.Email       ?? string.Empty,
                DisplayName            = data.DisplayName ?? string.Empty,
                IsEmailVerified        = data.Registered  ?? false,
                RequiresPasswordChange = requiresChange,
                LinkedProviders        = isGoogleUser
                    ? ["google.com"]
                    : ["password"]
            };

            return AuthResult.Ok(_currentUser);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Responde al navegador con una página que se cierra sola,
        /// indicando que puede volver a la aplicación.
        /// </summary>
        private static async Task SendBrowserCloseResponseAsync(
            HttpListenerContext context)
        {
            const string html = """
                <!DOCTYPE html>
                <html lang="es">
                <head><meta charset="UTF-8"><title>EcoHuellaApp</title></head>
                <body>
                  <p>Autenticación completada. Puedes cerrar esta ventana.</p>
                  <script>setTimeout(() => window.close(), 1000);</script>
                </body>
                </html>
                """;

            var buffer = Encoding.UTF8.GetBytes(html);
            context.Response.ContentType      = "text/html; charset=utf-8";
            context.Response.ContentLength64  = buffer.Length;
            context.Response.StatusCode       = 200;
            await context.Response.OutputStream.WriteAsync(buffer);
            context.Response.Close();
        }

        /// <summary>
        /// Encuentra un puerto TCP libre en el rango 49152-65535 (puertos efímeros).
        /// Evita conflictos con servicios que puedan estar usando puertos fijos.
        /// </summary>
        private static int GetAvailablePort()
        {
            using var socket = new System.Net.Sockets.TcpListener(
                System.Net.IPAddress.Loopback, 0);
            socket.Start();
            var port = ((IPEndPoint)socket.LocalEndpoint).Port;
            socket.Stop();
            return port;
        }

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

        // ── DTOs ─────────────────────────────────────────────────────────────

        private sealed record FirebaseSignInResponse(
            string? IdToken, string? Email, string? LocalId,
            string? DisplayName, bool? Registered);

        private sealed record FirebaseErrorEnvelope(FirebaseError? Error);
        private sealed record FirebaseError(string? Message);
    }
}
