using FinitiGlossary.Application.DTOs.Admin;
using FinitiGlossary.Domain.Entities.Terms;
using FinitiGlossary.Domain.Entities.Users;

namespace FinitiGlossary.Application.Aggregators.AdminHistory
{
    public class GlossaryAdminViewHistoryAggregator
    {
        public List<GlossaryAdminTermDTO> AggregateHistoryView(
            List<GlossaryTerm> activeTerms,
            List<ArchivedGlossaryTerm> archivedTerms,
            List<User> allUsers)
        {
            var userNames = allUsers.ToDictionary(u => u.Id, u => u.Username);

            string ResolveUser(string? rawId)
            {
                if (rawId == null) return "Unknown";
                return int.TryParse(rawId, out int id) && userNames.TryGetValue(id, out var name)
                    ? name
                    : "Unknown";
            }

            var result = new List<GlossaryAdminTermDTO>();

            GlossaryTerm? active = activeTerms.FirstOrDefault();

            if (active != null)
            {
                result.Add(new GlossaryAdminTermDTO
                {
                    Id = active.Id,
                    StableId = active.StableId,
                    Term = active.Term,
                    Definition = active.Definition,
                    Version = active.Version,
                    Status = 1,
                    CreatedOrArchivedAt = active.CreatedAt,
                    CreatedByName = ResolveUser(active.CreatedById),
                    ArchivedByName = null,
                    RestoredAt = null,
                    RestoredByName = null,
                    HasHistory = archivedTerms.Count > 0,
                    CanRestore = false
                });
            }

            foreach (var a in archivedTerms)
            {
                bool identical =
                    active != null &&
                    active.Term.Trim() == a.Term.Trim() &&
                    active.Definition.Trim() == a.Definition.Trim();

                result.Add(new GlossaryAdminTermDTO
                {
                    Id = a.Id,
                    StableId = a.StableId,
                    Term = a.Term,
                    Definition = a.Definition,
                    Version = a.Version,
                    Status = 2,
                    CreatedOrArchivedAt = a.ArchivedAt,
                    CreatedByName = ResolveUser(a.CreatedById),
                    ArchivedByName = ResolveUser(a.ArchivedById),
                    RestoredAt = a.RestoredAt,
                    RestoredByName = ResolveUser(a.RestoredById),
                    HasHistory = true,
                    CanRestore = !identical
                });
            }

            return result
                .OrderByDescending(x => x.Version)
                .ToList();
        }
    }
}
