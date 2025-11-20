namespace FinitiGlossary.Application.DTOs.Admin
{
    public record UnifiedTermDTO(
        int Id,
        Guid StableId,
        string Term,
        string Definition,
        int Version,
        int Status,
        DateTime Timestamp,
        string? CreatedById,
        string? ArchivedById,
        DateTime? RestoredAt,
        string? RestoredById
    );
}