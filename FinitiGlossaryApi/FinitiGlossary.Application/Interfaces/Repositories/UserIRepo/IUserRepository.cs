using FinitiGlossary.Domain.Entities.Users;
namespace FinitiGlossary.Application.Interfaces.Repositories.UserIRepo
{
    public interface IUserRepository
    {
        Task<bool> ExistsByEmailAsync(string email);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByResetTokenAsync(string token);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task<List<User>> GetAllUsersAsync();
    }
}
