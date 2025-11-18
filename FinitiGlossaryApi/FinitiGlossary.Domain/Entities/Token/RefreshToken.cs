using FinitiGlossary.Domain.Entities.Users;

namespace FinitiGlossary.Domain.Entities.Token
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
