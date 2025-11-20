namespace FinitiGlossary.Domain.Entities.Auth.Requests
{
    public record RegisterRequest
    {
        public required string Username { get; init; }
        public required string Email { get; init; }
        public required string Password { get; init; }
    }
}
