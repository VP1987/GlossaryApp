using FinitiGlossary.Domain.Entities.Users;

namespace FinitiGlossary.Application.Interfaces.Auth
{
    public interface IJwtTokenService
    {
        string CreateToken(User user);
    }
}
