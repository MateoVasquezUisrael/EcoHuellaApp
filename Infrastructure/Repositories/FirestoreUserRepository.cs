using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models;

namespace EcoHuellaApp.Infrastructure.Repositories
{
    /// <summary>Administra perfiles mediante Firestore REST.</summary>
    public sealed class FirestoreUserRepository : IUserRepository
    {
        // Proyecto Firebase.
        private const string ProjectId  = "login-ecohuella";
        private const string Collection = "usuarios";
        private const string BaseUrl    =
            $"https://firestore.googleapis.com/v1/projects/{ProjectId}" +
            $"/databases/(default)/documents/{Collection}";

        private static readonly HttpClient _http = new();

        public async Task<UsuarioSistema?> GetByUidAsync(string uid, string idToken)
        {
            if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(idToken))
                return null;
            if (idToken == "mock-local")
                return GetLocalProfile(uid);

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/{uid}");
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", idToken);

                var response = await _http.SendAsync(request);

                // El perfil no existe.
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return GetLocalProfile(uid);

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[Firestore] Error {(int)response.StatusCode} para uid={uid}");
                    return GetLocalProfile(uid);
                }

                var json = await response.Content.ReadAsStringAsync();
                var usuario = ParseDocument(json);
                if (usuario is not null)
                    SaveLocalProfile(usuario);
                return usuario ?? GetLocalProfile(uid);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Firestore] GetByUid: {ex.Message}");
                return GetLocalProfile(uid);
            }
        }

        public async Task<bool> CreateAsync(UsuarioSistema usuario, string idToken)
        {
            if (string.IsNullOrWhiteSpace(usuario.Uid) || string.IsNullOrWhiteSpace(idToken))
                return false;

            // Respaldo local del perfil.
            SaveLocalProfile(usuario);
            if (idToken == "mock-local")
                return true;

            var document = new
            {
                fields = new
                {
                    uid = new { stringValue = usuario.Uid },
                    email = new { stringValue = usuario.Email },
                    nombre = new { stringValue = usuario.Nombre },
                    organizacion = new { stringValue = usuario.Organizacion },
                    rol = new { stringValue = usuario.Rol.ToString() },
                    activo = new { booleanValue = usuario.Activo },
                    fechaCreacion = new { timestampValue = DateTime.UtcNow.ToString("O") }
                }
            };

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Patch, $"{BaseUrl}/{usuario.Uid}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);
                request.Content = new StringContent(
                    JsonSerializer.Serialize(document), Encoding.UTF8, "application/json");
                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var detail = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine(
                        $"[Firestore] Create {(int)response.StatusCode}: {detail}");
                }

                // Conserva el respaldo si falla la sincronización.
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Firestore] Create: {ex.Message}");
                return true;
            }
        }

        private static string LocalProfileKey(string uid) => $"ecohuella_user_{uid}";

        private static void SaveLocalProfile(UsuarioSistema usuario)
        {
            try
            {
                Preferences.Default.Set(
                    LocalProfileKey(usuario.Uid),
                    JsonSerializer.Serialize(usuario));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LocalProfile] Save: {ex.Message}");
            }
        }

        private static UsuarioSistema? GetLocalProfile(string uid)
        {
            try
            {
                var json = Preferences.Default.Get(LocalProfileKey(uid), string.Empty);
                return string.IsNullOrWhiteSpace(json)
                    ? null
                    : JsonSerializer.Deserialize<UsuarioSistema>(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LocalProfile] Read: {ex.Message}");
                return null;
            }
        }

        // Convierte el documento de Firestore.

        private static UsuarioSistema? ParseDocument(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("fields", out var fields))
                    return null;

                // Extrae el UID de la ruta.
                var uid = string.Empty;
                if (root.TryGetProperty("name", out var nameProp))
                {
                    var parts = nameProp.GetString()?.Split('/');
                    if (parts?.Length > 0) uid = parts[^1];
                }

                return new UsuarioSistema
                {
                    Uid    = uid,
                    Email  = GetString(fields, "email"),
                    Nombre = GetString(fields, "nombre"),
                    Organizacion = GetString(fields, "organizacion"),
                    Rol    = ParseRol(GetString(fields, "rol")),
                    Activo = GetBool(fields, "activo", defaultValue: true)
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Firestore] Parse: {ex.Message}");
                return null;
            }
        }

        private static string GetString(JsonElement fields, string key)
        {
            if (fields.TryGetProperty(key, out var f) &&
                f.TryGetProperty("stringValue", out var v))
                return v.GetString() ?? string.Empty;
            return string.Empty;
        }

        private static bool GetBool(JsonElement fields, string key, bool defaultValue)
        {
            if (fields.TryGetProperty(key, out var f) &&
                f.TryGetProperty("booleanValue", out var v))
                return v.GetBoolean();
            return defaultValue;
        }

        private static RolSistema ParseRol(string rol) => rol switch
        {
            "Administrador" => RolSistema.Administrador,
            "Supervisor"    => RolSistema.Supervisor,
            _               => RolSistema.Usuario
        };
    }
}
