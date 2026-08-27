namespace EcoHuellaApp.Domain.Models
{
    public sealed class AuthResult
    {
        private AuthResult() { }

        public bool      IsSuccess    { get; private init; }
        public AppUser?  User         { get; private init; }
        public string?   ErrorMessage { get; private init; }
        public AuthErrorCode ErrorCode { get; private init; }

        public static AuthResult Ok(AppUser user) => new()
        {
            IsSuccess = true,
            User      = user
        };

        public static AuthResult Fail(
            string message,
            AuthErrorCode code = AuthErrorCode.Unknown) => new()
        {
            IsSuccess    = false,
            ErrorMessage = message,
            ErrorCode    = code
        };
    }

    public enum AuthErrorCode
    {
        Unknown,
        InvalidCredentials,
        UserNotFound,
        UserDisabled,
        WeakPassword,
        PasswordMismatch,
        RequiresRecentLogin,
        NetworkError,
        TooManyRequests,
        Cancelled,

        // Autorización Firestore
        UserNotAuthorized,  // Autenticado en Firebase pero sin documento en Firestore
        UserDeactivated     // Existe en Firestore pero activo = false
    }
}
