using System.Net.Http.Headers;
using System.Text.Json;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models;

namespace EcoHuellaApp.Infrastructure.Repositories
{
    /// <summary>
    /// Consulta Firestore usando la REST API v1.
    /// Funciona en todas las plataformas (Android, iOS, Windows) sin SDK nativo.
    /// Usa el ID token de Firebase para autenticar la petición.
    ///
    /// Seguridad: las reglas de Firestore deben permitir que cada usuario
    /// lea únicamente su propio documento:
    ///   match /usuarios/{userId} {
    ///     allow read: if request.auth != null &amp;&amp; request.auth.uid == userId;
    ///   }
    /// </summary>
    public sealed class FirestoreUserRepository : IUserRepository
    {
        // Project ID extraído de google-services.json → project_info.project_id
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

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/{uid}");
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", idToken);

                var response = await _http.SendAsync(request);

                // 404 → el documento no existe → usuario no registrado en el sistema
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[Firestore] Error {(int)response.StatusCode} para uid={uid}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                return ParseDocument(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Firestore] GetByUid: {ex.Message}");
                return null;
            }
        }

        // ── Parser del formato de documento Firestore ─────────────────────────
        // Firestore REST devuelve campos en este formato:
        // { "fields": { "nombre": { "stringValue": "..." }, "activo": { "booleanValue": true } } }

        private static UsuarioSistema? ParseDocument(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("fields", out var fields))
                    return null;

                // Extrae el UID del path del documento
                // Formato: "projects/.../documents/usuarios/{uid}"
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
