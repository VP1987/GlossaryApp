using FinitiGlossary.Domain.Entities.Users;

namespace FinitiGlossary.Application.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<(bool Success, string Message)> RegisterAsync(string username, string email, string password);
        Task<(bool Success, string Token, string RefreshToken, string Message)> LoginAsync(string email, string password);
        Task<(bool Success, string Token, string RefreshToken, string Message)> RefreshTokenAsync(string refreshToken);
        Task<(bool Success, string Message)> ResetPasswordRequestAsync(string email);
        Task<(bool Success, string Message)> ResetPasswordConfirmAsync(string token, string newPassword);
        Task<(bool Success, User? User, string Token, string Message)> GenerateAndStoreResetTokenAsync(string email);

    }
}