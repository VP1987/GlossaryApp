namespace FinitiGlossary.Application.DTOs.Request

{
    public record ResetPasswordConfirmRequest(string Token, string NewPassword);
}
