namespace FinitiGlossary.Application.DTOs.Response
{
    public record AuthResponse(string Token, string RefreshToken, string Message);
}
