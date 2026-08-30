using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models;

namespace EcoHuellaApp.Infrastructure.Services;

/// <summary>Recuperación local para el mockup.</summary>
public sealed class MockPasswordService : IMockPasswordService
{
    private const string IndexPrefix = "mock_auth_index_";
    private const string CredentialPrefix = "mock_auth_credential_";
    private string? _activeEmail;

    public bool HasPendingPasswordChange => _activeEmail is not null;

    public void TrackUser(AppUser user)
    {
        if (!string.IsNullOrWhiteSpace(user.Email) && !string.IsNullOrWhiteSpace(user.Uid))
            Preferences.Default.Set(IndexKey(user.Email), user.Uid);
    }

    public string GenerateTemporaryPassword(string email)
    {
        email = Normalize(email);
        var uid = Preferences.Default.Get(IndexKey(email), string.Empty);
        if (string.IsNullOrWhiteSpace(uid))
            uid = $"mock-{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(email)))[..20].ToLowerInvariant()}";

        var temporaryPassword = $"Eco-{RandomNumberGenerator.GetInt32(100000, 999999)}";
        Save(new MockCredential(uid, email, Hash(temporaryPassword), true));
        return temporaryPassword;
    }

    public AppUser? TrySignIn(string email, string password)
    {
        email = Normalize(email);
        var credential = Read(email);
        if (credential is null || credential.PasswordHash != Hash(password))
            return null;

        _activeEmail = credential.RequiresPasswordChange ? email : null;
        return new AppUser
        {
            Uid = credential.Uid,
            Email = credential.Email,
            DisplayName = credential.Email.Split('@')[0],
            Role = "Usuario",
            IsEmailVerified = true,
            RequiresPasswordChange = credential.RequiresPasswordChange,
            LinkedProviders = ["mock-password"]
        };
    }

    public void CompletePasswordChange(string newPassword)
    {
        if (_activeEmail is null) return;
        var credential = Read(_activeEmail);
        if (credential is not null)
            Save(credential with { PasswordHash = Hash(newPassword), RequiresPasswordChange = false });
        _activeEmail = null;
    }

    public void ClearPendingState() => _activeEmail = null;

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();
    private static string IndexKey(string email) => $"{IndexPrefix}{Normalize(email)}";
    private static string CredentialKey(string email) => $"{CredentialPrefix}{Normalize(email)}";
    private static string Hash(string value) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static void Save(MockCredential credential) => Preferences.Default.Set(
        CredentialKey(credential.Email), JsonSerializer.Serialize(credential));

    private static MockCredential? Read(string email)
    {
        var json = Preferences.Default.Get(CredentialKey(email), string.Empty);
        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<MockCredential>(json);
    }

    private sealed record MockCredential(
        string Uid, string Email, string PasswordHash, bool RequiresPasswordChange);
}
