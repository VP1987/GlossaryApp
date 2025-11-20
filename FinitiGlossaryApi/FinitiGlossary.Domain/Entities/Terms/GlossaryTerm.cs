using FinitiGlossary.Domain.Entities.Status;

namespace FinitiGlossary.Domain.Entities.Terms
{
    public class GlossaryTerm
    {
        public int Id { get; set; }
        public string Term { get; set; } = string.Empty;
        public string Definition { get; set; } = string.Empty;
        public TermStatus Status { get; set; } = TermStatus.Draft;
        public Guid StableId { get; set; } = Guid.NewGuid();
        public int Version { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedById { get; set; } = string.Empty;
    }
}
