using FinitiGlossary.Domain.Entities.Users;
using FinitiGlossary.Domain.Interfaces.Repositories.UserIRepo;
using FinitiGlossary.Infrastructure.DAL;
using Microsoft.EntityFrameworkCore;

namespace FinitiGlossary.Infrastructure.Repositories.UserRepo
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _db.Users.ToListAsync();
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _db.Users.AnyAsync(x => x.Email == email);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _db.Users.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<User?> GetByResetTokenAsync(string token)
        {
            return await _db.Users.FirstOrDefaultAsync(x =>
                x.ResetToken == token && x.ResetTokenExpires > DateTime.UtcNow);
        }

        public async Task AddAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }
    }
}