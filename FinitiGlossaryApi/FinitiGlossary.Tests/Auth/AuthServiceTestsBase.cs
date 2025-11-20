using FinitiGlossary.Application.Interfaces.Auth;
using FinitiGlossary.Application.Interfaces.Email;
using FinitiGlossary.Application.Services.Auth;
using FinitiGlossary.Domain.Interfaces.Repositories.Token;
using FinitiGlossary.Domain.Interfaces.Repositories.UserIRepo;
using Microsoft.Extensions.Configuration;
using Moq;

namespace FinitiGlossary.Tests.Auth
{
    public abstract class AuthServiceTestsBase
    {
        protected readonly Mock<IUserRepository> _userRepoMock;
        protected readonly Mock<IRefreshTokenRepository> _refreshRepoMock;
        protected readonly Mock<IPasswordHasher> _hasherMock;
        protected readonly Mock<IEmailService> _emailMock;
        protected readonly IConfiguration _config;
        protected readonly AuthService _authService;

        protected AuthServiceTestsBase()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _refreshRepoMock = new Mock<IRefreshTokenRepository>();
            _hasherMock = new Mock<IPasswordHasher>();
            _emailMock = new Mock<IEmailService>();

            var inMemoryConfig = new Dictionary<string, string?>
            {
                { "Jwt:Key", "ovo_je_jako_dugacak_kljuc_za_testiranje_bita_mora_biti_dug" },
                { "Jwt:Issuer", "FinitiGlossary" },
                { "Jwt:Audience", "FinitiGlossaryUsers" },
                { "Frontend:BaseUrl", "http://localhost:3000" }
            };

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemoryConfig)
                .Build();

            _authService = new AuthService(
                _userRepoMock.Object,
                _refreshRepoMock.Object,
                _hasherMock.Object,
                _emailMock.Object,
                _config
            );
        }
    }
}