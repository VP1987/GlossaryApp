namespace FinitiGlossary.Application.Interfaces.Email
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string htmlBody);
    }
}
