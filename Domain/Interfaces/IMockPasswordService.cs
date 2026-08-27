using EcoHuellaApp.Domain.Models;

namespace EcoHuellaApp.Domain.Interfaces;

public interface IMockPasswordService
{
    string GenerateTemporaryPassword(string email);
    AppUser? TrySignIn(string email, string password);
    bool HasPendingPasswordChange { get; }
    void CompletePasswordChange(string newPassword);
    void TrackUser(AppUser user);
}
