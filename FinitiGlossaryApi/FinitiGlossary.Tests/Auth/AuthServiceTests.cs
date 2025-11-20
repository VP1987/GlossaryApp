using FinitiGlossary.Domain.Entities.Auth.Token;
using FinitiGlossary.Domain.Entities.Users;
using Moq;

namespace FinitiGlossary.Tests.Auth
{
    public class AuthServiceTests : AuthServiceTestsBase
    {
        // ============================================================
        // 1. REGISTER TESTS
        // ============================================================
        [Fact]
        public async Task RegisterAsync_ShouldReturnError_WhenEmailExists()
        {
            _userRepoMock.Setup(x => x.ExistsByEmailAsync("test@test.com")).ReturnsAsync(true);

            var result = await _authService.RegisterAsync("user", "test@test.com", "pass");

            Assert.False(result.Success);
            Assert.Equal("User with this email already exists.", result.Message);
        }

        [Fact]
        public async Task RegisterAsync_ShouldSuccess_WhenDataIsValid()
        {
            _userRepoMock.Setup(x => x.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(false);
            _hasherMock.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashed_pass");

            var result = await _authService.RegisterAsync("user", "new@test.com", "pass");

            Assert.True(result.Success);
            _userRepoMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        }

        // ============================================================
        // 2. LOGIN TESTS
        // ============================================================
        [Fact]
        public async Task LoginAsync_ShouldReturnError_WhenUserNotFound()
        {
            _userRepoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            var result = await _authService.LoginAsync("wrong@test.com", "pass");

            Assert.False(result.Success);
            Assert.Equal("Invalid email or password.", result.Message);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnError_WhenPasswordWrong()
        {
            var user = new User { Email = "test@test.com", PasswordHash = "hash" };
            _userRepoMock.Setup(x => x.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _hasherMock.Setup(x => x.Verify("wrong", "hash")).Returns(false);

            var result = await _authService.LoginAsync(user.Email, "wrong");

            Assert.False(result.Success);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsValid()
        {
            var user = new User { Id = 1, Email = "ok@test.com", Role = "User", PasswordHash = "hash" };
            _userRepoMock.Setup(x => x.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _hasherMock.Setup(x => x.Verify("pass", "hash")).Returns(true);

            var result = await _authService.LoginAsync(user.Email, "pass");

            Assert.True(result.Success);
            Assert.NotEmpty(result.Token);
            Assert.NotEmpty(result.RefreshToken);
        }

        [Theory]
        [InlineData("", "password")]
        [InlineData(null, "password")]
        [InlineData("email@test.com", "")]
        [InlineData("email@test.com", null)]
        public async Task LoginAsync_ShouldReturnFalse_WhenInputsInvalid(string email, string password)
        {
            var result = await _authService.LoginAsync(email, password);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task LoginAsync_ShouldHandleExceptions_Gracefully()
        {
            _userRepoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ThrowsAsync(new Exception("DB Error"));

            var result = await _authService.LoginAsync("test@test.com", "pass");

            Assert.False(result.Success);
            Assert.Contains("Unexpected error", result.Message);
        }

        // ============================================================
        // 3. REFRESH TOKEN TESTS
        // ============================================================
        [Fact]
        public async Task RefreshTokenAsync_ShouldReturnError_WhenTokenInvalid()
        {
            _refreshRepoMock.Setup(x => x.GetValidTokenAsync(It.IsAny<string>())).ReturnsAsync((RefreshToken?)null);

            var result = await _authService.RefreshTokenAsync("bad_token");

            Assert.False(result.Success);
            Assert.Equal("Invalid or expired refresh token.", result.Message);
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldRotateToken_WhenValid()
        {
            var user = new User { Id = 1, Email = "test@test.com", Role = "User" };
            var oldToken = new RefreshToken { Token = "old", UserId = 1, User = user };

            _refreshRepoMock.Setup(x => x.GetValidTokenAsync("old")).ReturnsAsync(oldToken);

            var result = await _authService.RefreshTokenAsync("old");

            Assert.True(result.Success);
            Assert.NotEqual("old", result.RefreshToken);

            Assert.True(oldToken.IsRevoked);
            _refreshRepoMock.Verify(x => x.UpdateAsync(oldToken), Times.Once);
            _refreshRepoMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
        }

        // ============================================================
        // 4. RESET PASSWORD TESTS
        // ============================================================
        [Fact]
        public async Task ResetPasswordRequestAsync_ShouldSendEmail_WhenUserExists()
        {
            var user = new User { Email = "test@test.com" };
            _userRepoMock.Setup(x => x.GetByEmailAsync(user.Email)).ReturnsAsync(user);

            var result = await _authService.ResetPasswordRequestAsync(user.Email);

            Assert.True(result.Success);
            Assert.NotNull(user.ResetToken);

            _emailMock.Verify(x => x.SendAsync(user.Email, "Password Reset", It.IsAny<string>()), Times.Once);
            _userRepoMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task ResetPasswordConfirmAsync_ShouldReset_WhenTokenValid()
        {
            var user = new User
            {
                ResetToken = "valid_token",
                ResetTokenExpires = DateTime.UtcNow.AddHours(1)
            };

            _userRepoMock.Setup(x => x.GetByResetTokenAsync("valid_token")).ReturnsAsync(user);
            _hasherMock.Setup(x => x.Hash("new_pass")).Returns("new_hash");

            var result = await _authService.ResetPasswordConfirmAsync("valid_token", "new_pass");

            Assert.True(result.Success);
            Assert.Equal("new_hash", user.PasswordHash);
            Assert.Null(user.ResetToken);
            _userRepoMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task ResetPasswordConfirmAsync_ShouldFail_WhenTokenExpired()
        {
            var user = new User
            {
                ResetToken = "expired_token",
                ResetTokenExpires = DateTime.UtcNow.AddHours(-1)
            };

            _userRepoMock.Setup(x => x.GetByResetTokenAsync("expired_token")).ReturnsAsync(user);

            var result = await _authService.ResetPasswordConfirmAsync("expired_token", "new_pass");

            Assert.False(result.Success);
            Assert.Equal("Reset token has expired.", result.Message);
        }
    }
}