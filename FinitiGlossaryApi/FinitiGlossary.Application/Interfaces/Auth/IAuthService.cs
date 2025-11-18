namespace FinitiGlossary.Application.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<(bool Success, string Message)> RegisterAsync(string username, string email, string password);
        Task<(bool Success, string Token, string RefreshToken, string Message)> LoginAsync(string email, string password);
        Task<(bool Success, string Token, string RefreshToken, string Message)> RefreshTokenAsync(string refreshToken);
        Task<(bool Success, string Message)> ResetPasswordRequestAsync(string email);
        Task<(bool Success, string Message)> ResetPasswordConfirmAsync(string token, string newPassword);
    }
}