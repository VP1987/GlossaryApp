namespace FinitiGlossary.Domain.Entities.Terms
{
    public class ArchivedGlossaryTerm
    {
        public int Id { get; set; }
        public int OriginalTermId { get; set; }
        public string? Term { get; set; } = string.Empty;
        public string? Definition { get; set; } = string.Empty;
        public DateTime ArchivedAt { get; set; } = DateTime.UtcNow;
        public string? ArchivedById { get; set; } = string.Empty;
        public string? ChangeSummary { get; set; } = string.Empty;
        public string? CreatedById { get; set; } = string.Empty;
        public DateTime? RestoredAt { get; set; }
        public string? RestoredById { get; set; }
        public int Version { get; set; } = 1;
        public Guid StableId { get; set; }

    }
}
