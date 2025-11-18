namespace FinitiGlossary.Domain.Entities.Auth
{
    public class ResetPasswordConfirmRequest
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
