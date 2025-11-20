namespace FinitiGlossary.Application.DTOs.Admin
{
    public record GlossaryAdminTermDTO
    {
        public int Id { get; init; }
        public Guid StableId { get; init; }
        public string Term { get; init; } = string.Empty;
        public string Definition { get; init; } = string.Empty;
        public int Version { get; init; }
        public int Status { get; init; }
        public DateTime CreatedOrArchivedAt { get; init; }
        public string? CreatedById { get; init; }
        public string? CreatedByName { get; init; }
        public string? ArchivedByName { get; init; }
        public DateTime? RestoredAt { get; init; }
        public string? RestoredByName { get; init; }
        public bool HasHistory { get; init; }
        public bool CanRestore { get; init; }
    }
}