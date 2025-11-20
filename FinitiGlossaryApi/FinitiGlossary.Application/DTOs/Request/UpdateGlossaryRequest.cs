using FinitiGlossary.Domain.Entities.Status;

namespace FinitiGlossary.Application.DTOs.Request
{
    public record UpdateGlossaryRequest(string? Term, string? Definition, TermStatus? Status);
}
