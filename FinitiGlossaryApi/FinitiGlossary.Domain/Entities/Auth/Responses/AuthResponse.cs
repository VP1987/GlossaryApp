namespace FinitiGlossary.Domain.Entities.Auth.Responses
{
    public record AuthResponse(string Token, string RefreshToken, string Message);
}
