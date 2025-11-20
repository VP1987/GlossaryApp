using FinitiGlossary.Domain.Entities.Auth.Token;

namespace FinitiGlossary.Application.Interfaces.Repositories.Token
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken token);
        Task<RefreshToken?> GetValidTokenAsync(string token);
        Task UpdateAsync(RefreshToken token);
    }
}
