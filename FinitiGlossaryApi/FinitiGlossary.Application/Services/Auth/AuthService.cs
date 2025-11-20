using FinitiGlossary.Application.Interfaces.Auth;
using FinitiGlossary.Application.Interfaces.Email;
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
        private readonly IEmailService _email;

        public AuthService(
            IUserRepository users,
            IRefreshTokenRepository refreshTokens,
            IPasswordHasher hasher,
            IEmailService email,
            IConfiguration config)
        {
            _users = users;
            _refreshTokens = refreshTokens;
            _hasher = hasher;
            _email = email;
            _config = config;
        }

        public async Task<(bool Success, string Message)> RegisterAsync(string username, string email, string password)
        {
            try
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
            catch
            {
                return (false, "An unexpected error occurred.");
            }
        }

        public async Task<(bool Success, string Token, string RefreshToken, string Message)>
            LoginAsync(string email, string password)
        {
            try
            {
                var user = await _users.GetByEmailAsync(email);
                if (user == null || !_hasher.Verify(password, user.PasswordHash))
                    return (false, "", "", "Invalid email or password.");

                var jwt = GenerateJwtToken(user);
                var refresh = CreateRefreshToken(user.Id);

                await _refreshTokens.AddAsync(refresh);

                return (true, jwt, refresh.Token, "Login successful.");
            }
            catch
            {
                return (false, "", "", "Unexpected error during login.");
            }
        }

        public async Task<(bool Success, string Token, string RefreshToken, string Message)>
             RefreshTokenAsync(string refreshToken)
        {
            try
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
            catch
            {
                return (false, "", "", "Unexpected error refreshing token.");
            }
        }

        public async Task<(bool Success, string Message)> ResetPasswordRequestAsync(string email)
        {
            try
            {
                var user = await _users.GetByEmailAsync(email);
                if (user == null)
                    return (false, "No user found with that email.");

                user.ResetToken = Guid.NewGuid().ToString();
                user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);

                await _users.UpdateAsync(user);

                var resetUrl = $"{_config["Frontend:BaseUrl"]}/reset-password?token={user.ResetToken}";

                var html = $@"
                    <h2>Password Reset</h2>
                    <p>Click the link below to reset your password:</p>
                    <a href=""{resetUrl}"">{resetUrl}</a>
                ";

                await _email.SendAsync(user.Email, "Password Reset", html);

                return (true, "Password reset email has been sent.");
            }
            catch
            {
                return (false, "Unexpected error generating reset token.");
            }
        }

        public async Task<(bool Success, string Message)> ResetPasswordConfirmAsync(string token, string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return (false, "Invalid reset token.");

                var user = await _users.GetByResetTokenAsync(token);
                if (user == null)
                    return (false, "Invalid or expired reset token.");

                if (user.ResetTokenExpires == null || user.ResetTokenExpires < DateTime.UtcNow)
                    return (false, "Reset token has expired.");

                user.PasswordHash = _hasher.Hash(newPassword);
                user.ResetToken = null;
                user.ResetTokenExpires = null;

                await _users.UpdateAsync(user);

                return (true, "Password has been reset successfully.");
            }
            catch
            {
                return (false, "Unexpected error resetting password.");
            }
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );

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

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _users.GetByEmailAsync(email);
            }
            catch
            {
                return null;
            }
        }

        public async Task<(bool Success, User? User, string Token, string Message)>
            GenerateAndStoreResetTokenAsync(string email)
        {
            try
            {
                var user = await _users.GetByEmailAsync(email);
                if (user == null)
                    return (false, null, "", "User not found.");

                var token = Guid.NewGuid().ToString();
                user.ResetToken = token;
                user.ResetTokenExpires = DateTime.UtcNow.AddMinutes(30);

                await _users.UpdateAsync(user);

                return (true, user, token, "Reset token generated and stored.");
            }
            catch
            {
                return (false, null, "", "Unexpected error generating reset token.");
            }
        }
    }
}
