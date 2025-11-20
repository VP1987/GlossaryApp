namespace FinitiGlossary.Domain.Entities.Auth.Requests
{
    public record ResetPasswordConfirmRequest(string Token, string NewPassword);
}
