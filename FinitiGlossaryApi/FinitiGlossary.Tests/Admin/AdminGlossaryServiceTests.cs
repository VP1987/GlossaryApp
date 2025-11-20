using FinitiGlossary.Application.Aggregators.AdminHistory;
using FinitiGlossary.Application.Aggregators.AdminView;
using FinitiGlossary.Application.DTOs.Request;
using FinitiGlossary.Application.Interfaces.Repositories.Admin;
using FinitiGlossary.Application.Services.Admin;
using FinitiGlossary.Domain.Entities.Terms;
using FinitiGlossary.Domain.Interfaces.Repositories.UserIRepo;
using Moq;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

// Fiktivni namespace za testiranje
namespace FinitiGlossary.Tests.Admin
{
    public class AdminGlossaryServiceTests
    {
        private readonly Mock<IAdminGlossaryRepository> _repoMock;
        private readonly Mock<GlossaryAdminViewAggregator> _viewAggregatorMock;
        private readonly Mock<GlossaryAdminViewHistoryAggregator> _historyAggregatorMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly AdminGlossaryService _sut; // System Under Test

        // Fiktivni korisnik za testiranje
        private readonly ClaimsPrincipal _testUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("id", "123"),
            new Claim(ClaimTypes.Role, "Admin")
        }, "mock"));

        public AdminGlossaryServiceTests()
        {
            // Inicijalizacija lažnih objekata za svako testiranje
            _repoMock = new Mock<IAdminGlossaryRepository>();
            _viewAggregatorMock = new Mock<GlossaryAdminViewAggregator>(/* Možda zahteva dodatne argumente */);
            _historyAggregatorMock = new Mock<GlossaryAdminViewHistoryAggregator>(/* Možda zahteva dodatne argumente */);
            _userRepoMock = new Mock<IUserRepository>();

            // Stvaranje instance servisa (SUT) sa lažnim zavisnostima
            _sut = new AdminGlossaryService(
                _repoMock.Object,
                _viewAggregatorMock.Object,
                _historyAggregatorMock.Object,
                _userRepoMock.Object);
        }

        // =========================================================================
        // Testovi za CreateAsync
        // =========================================================================

        [Fact(DisplayName = "CreateAsync_ShouldReturnSuccessMessage_WhenDraftIsCreated")]
        public async Task CreateAsync_SuccessPath()
        {
            // Arrange
            var request = new CreateGlossaryRequest("Test", "Definition");
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<GlossaryTerm>())).ReturnsAsync(true); // Simuliramo uspeh

            // Act
            var result = await _sut.CreateAsync(request, _testUser);
            var jsonResult = JObject.FromObject(result);

            // Assert
            Assert.Equal("Draft created successfully.", jsonResult["message"].ToString());
            _repoMock.Verify(r => r.CreateAsync(It.Is<GlossaryTerm>(t => t.Term == request.Term)), Times.Once);
        }

        [Fact(DisplayName = "CreateAsync_ShouldReturnFailedMessage_WhenRepositoryReturnsFalse")]
        public async Task CreateAsync_RepositoryFailure()
        {
            // Arrange
            var request = new CreateGlossaryRequest("Test", "Definition");
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<GlossaryTerm>())).ReturnsAsync(false); // Simuliramo neuspeh

            // Act
            var result = await _sut.CreateAsync(request, _testUser);
            var jsonResult = JObject.FromObject(result);

            // Assert
            Assert.Equal("Failed to create draft.", jsonResult["message"].ToString());
        }

        [Fact(DisplayName = "CreateAsync_ShouldReturnUnexpectedErrorMessage_WhenRepositoryThrowsException")]
        public async Task CreateAsync_SystemFailure()
        {
            // Arrange
            var request = new CreateGlossaryRequest("Test", "Definition");
            // Simuliramo sistemsku grešku
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<GlossaryTerm>())).ThrowsAsync(new TimeoutException("DB Timeout."));

            // Act
            var result = await _sut.CreateAsync(request, _testUser);
            var jsonResult = JObject.FromObject(result);

            // Assert
            // Provera da li je uhvaćen izuzetak i da li je vraćena generička greška (catch blok)
            Assert.Contains("unexpected server error", jsonResult["message"].ToString().ToLower());
        }

        // =========================================================================
        // Testovi za DeleteAsync
        // =========================================================================

        [Fact(DisplayName = "DeleteAsync_ShouldReturnDeletedMessage_WhenSuccessful")]
        public async Task DeleteAsync_SuccessPath()
        {
            // Arrange
            const int termId = 10;
            var termToDelete = new GlossaryTerm { Id = termId };
            _repoMock.Setup(r => r.GetActiveByIdAsync(termId)).ReturnsAsync(termToDelete);
            _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            // Act
            var result = await _sut.DeleteAsync(termId, _testUser);
            var jsonResult = JObject.FromObject(result);

            // Assert
            Assert.Equal("Deleted.", jsonResult["message"].ToString());
            _repoMock.Verify(r => r.RemoveActiveTerm(termToDelete), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact(DisplayName = "DeleteAsync_ShouldReturnNotFoundMessage_WhenTermDoesNotExist")]
        public async Task DeleteAsync_TermNotFound()
        {
            // Arrange
            const int termId = 99;
            _repoMock.Setup(r => r.GetActiveByIdAsync(termId)).ReturnsAsync((GlossaryTerm)null!);

            // Act
            var result = await _sut.DeleteAsync(termId, _testUser);
            var jsonResult = JObject.FromObject(result);

            // Assert
            Assert.Equal("Not found.", jsonResult["message"].ToString());
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact(DisplayName = "DeleteAsync_ShouldReturnFailedMessage_WhenSaveChangesFails")]
        public async Task DeleteAsync_SaveChangesFails()
        {
            // Arrange
            const int termId = 10;
            var termToDelete = new GlossaryTerm { Id = termId };
            _repoMock.Setup(r => r.GetActiveByIdAsync(termId)).ReturnsAsync(termToDelete);
            _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(false); // Simuliramo neuspeh snimanja

            // Act
            var result = await _sut.DeleteAsync(termId, _testUser);
            var jsonResult = JObject.FromObject(result);

            // Assert
            Assert.Equal("Failed to delete term.", jsonResult["message"].ToString());
        }

        [Fact(DisplayName = "DeleteAsync_ShouldReturnUnexpectedErrorMessage_WhenRepositoryThrowsException")]
        public async Task DeleteAsync_SystemFailure()
        {
            // Arrange
            const int termId = 10;
            // Simuliramo sistemsku grešku prilikom čitanja
            _repoMock.Setup(r => r.GetActiveByIdAsync(termId)).ThrowsAsync(new Exception("Network failure."));

            // Act
            var result = await _sut.DeleteAsync(termId, _testUser);
            var jsonResult = JObject.FromObject(result);

            // Assert
            Assert.Contains("unexpected server error", jsonResult["message"].ToString().ToLower());
            _repoMock.Verify(r => r.RemoveActiveTerm(It.IsAny<GlossaryTerm>()), Times.Never);
        }
    }
}