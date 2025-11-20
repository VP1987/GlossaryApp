using FinitiGlossary.Domain.Entities.Auth.Token;
using FinitiGlossary.Domain.Interfaces.Repositories.Token;
using FinitiGlossary.Infrastructure.DAL;
using Microsoft.EntityFrameworkCore;

namespace FinitiGlossary.Infrastructure.Repositories.Token
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _db;

        public RefreshTokenRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task AddAsync(RefreshToken token)
        {
            _db.RefreshTokens.Add(token);
            await _db.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetValidTokenAsync(string token)
        {
            return await _db.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.Token == token &&
                    !x.IsRevoked &&
                    x.ExpiresAt > DateTime.UtcNow);
        }

        public async Task UpdateAsync(RefreshToken token)
        {
            _db.RefreshTokens.Update(token);
            await _db.SaveChangesAsync();
        }
    }
}
