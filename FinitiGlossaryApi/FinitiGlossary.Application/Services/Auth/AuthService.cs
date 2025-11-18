using FinitiGlossary.Application.Interfaces.Auth;
using FinitiGlossary.Application.Interfaces.Repositories.Token;
using FinitiGlossary.Application.Interfaces.Repositories.UserIRepo;
using FinitiGlossary.Domain.Entities.Token;
using FinitiGlossary.Domain.Entities.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FinitiGlossary.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly IRefreshTokenRepository _refreshTokens;
        private readonly IConfiguration _config;
        private readonly IPasswordHasher _hasher;

        public AuthService(
            IUserRepository users,
            IRefreshTokenRepository refreshTokens,
            IPasswordHasher hasher,
            IConfiguration config)
        {
            _users = users;
            _refreshTokens = refreshTokens;
            _hasher = hasher;
            _config = config;
        }
        public async Task<(bool Success, string Message)> RegisterAsync(string username, string email, string password)
        {
            var exists = await _users.ExistsByEmailAsync(email);
            if (exists)
                return (false, "User with this email already exists.");

            var passwordHash = _hasher.Hash(password);

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                Role = "User",
                IsAdmin = false,
                CreatedAt = DateTime.UtcNow
            };

            await _users.AddAsync(user);
            return (true, "User registered successfully.");
        }

        public async Task<(bool Success, string Token, string RefreshToken, string Message)> LoginAsync(string email, string password)
        {
            var user = await _users.GetByEmailAsync(email);
            if (user == null || !_hasher.Verify(password, user.PasswordHash))
                return (false, "", "", "Invalid email or password.");

            var jwt = GenerateJwtToken(user);
            var refresh = CreateRefreshToken(user.Id);

            await _refreshTokens.AddAsync(refresh);

            return (true, jwt, refresh.Token, "Login successful.");
        }
        public async Task<(bool Success, string Token, string RefreshToken, string Message)> RefreshTokenAsync(string refreshToken)
        {
            var stored = await _refreshTokens.GetValidTokenAsync(refreshToken);
            if (stored == null)
                return (false, "", "", "Invalid or expired refresh token.");

            stored.IsRevoked = true;
            await _refreshTokens.UpdateAsync(stored);

            var newJwt = GenerateJwtToken(stored.User!);
            var newRefresh = CreateRefreshToken(stored.UserId);

            await _refreshTokens.AddAsync(newRefresh);

            return (true, newJwt, newRefresh.Token, "Token refreshed.");
        }

        public async Task<(bool Success, string Message)> ResetPasswordRequestAsync(string email)
        {
            var user = await _users.GetByEmailAsync(email);
            if (user == null)
                return (false, "No user found with that email.");

            user.ResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);

            await _users.UpdateAsync(user);
            return (true, "Password reset token generated.");
        }

        public async Task<(bool Success, string Message)> ResetPasswordConfirmAsync(string token, string newPassword)
        {
            var user = await _users.GetByResetTokenAsync(token);
            if (user == null || user.ResetTokenExpires < DateTime.UtcNow)
                return (false, "Invalid or expired token.");

            user.PasswordHash = _hasher.Hash(newPassword);
            user.ResetToken = null;
            user.ResetTokenExpires = null;

            await _users.UpdateAsync(user);
            return (true, "Password has been reset successfully.");
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim("id", user.Id.ToString()),
                new Claim("username", user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("isAdmin", user.IsAdmin.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private RefreshToken CreateRefreshToken(int userId)
        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };
        }
    }
}
