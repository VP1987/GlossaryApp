using FinitiGlossary.Application.DTOs.Admin;
using FinitiGlossary.Domain.Entities.Terms;
using FinitiGlossary.Domain.Entities.Users;

namespace FinitiGlossary.Application.Aggregators.AdminView
{
    public class GlossaryAdminViewAggregator
    {
        public List<GlossaryAdminTermDTO> AggregateAdminView(
            List<GlossaryTerm> active,
            List<ArchivedGlossaryTerm> archived,
            List<User> users,
            int? currentUserId,
            bool isAdmin)
        {
            if (currentUserId.HasValue && !isAdmin)
            {
                string uid = currentUserId.Value.ToString();
                active = active.Where(x => x.CreatedById == uid).ToList();
                archived = archived.Where(x => x.CreatedById == uid).ToList();
            }

            var userMap = users.ToDictionary(
                u => u.Id.ToString(),
                u => u.Username
            );

            string U(string? id) =>
                id != null && userMap.TryGetValue(id, out var name) ? name : "Unknown";

            var flat = new List<UnifiedTermDTO>();

            foreach (var t in active)
            {
                flat.Add(new UnifiedTermDTO(
                    t.Id,
                    t.StableId,
                    t.Term ?? string.Empty,
                    t.Definition ?? string.Empty,
                    t.Version,
                    (int)t.Status,
                    t.CreatedAt,
                    t.CreatedById,
                    null,
                    null,
                    null
                ));
            }

            foreach (var a in archived)
            {
                flat.Add(new UnifiedTermDTO(
                    a.Id,
                    a.StableId,
                    a.Term ?? string.Empty,
                    a.Definition ?? string.Empty,
                    a.Version,
                    2,
                    a.ArchivedAt,
                    a.CreatedById,
                    a.ArchivedById,
                    a.RestoredAt,
                    a.RestoredById
                ));
            }

            return flat
                .GroupBy(x => x.StableId)
                .SelectMany(g =>
                {
                    var result = new List<GlossaryAdminTermDTO>();

                    var activeItem = g.FirstOrDefault(x => x.Status != 2);
                    var archivedItem = g
                        .Where(x => x.Status == 2)
                        .OrderByDescending(x => x.Timestamp)
                        .FirstOrDefault();

                    bool hasHistory = g.Count() > 1;
                    bool identical =
                        activeItem != null &&
                        archivedItem != null &&
                        activeItem.Term == archivedItem.Term &&
                        activeItem.Definition == archivedItem.Definition;

                    if (activeItem != null)
                        result.Add(ToDto(activeItem, U, hasHistory, false));

                    if (archivedItem != null)
                        result.Add(ToDto(archivedItem, U, true, !identical));

                    return result;
                })
                .ToList();
        }

        private GlossaryAdminTermDTO ToDto(
            UnifiedTermDTO x,
            Func<string?, string> U,
            bool hasHistory,
            bool canRestore)
        {
            return new GlossaryAdminTermDTO
            {
                Id = x.Id,
                StableId = x.StableId,
                Term = x.Term,
                Definition = x.Definition,
                Version = x.Version,
                Status = x.Status,
                CreatedOrArchivedAt = x.Timestamp,
                CreatedByName = U(x.CreatedById),
                ArchivedByName = U(x.ArchivedById),
                RestoredAt = x.RestoredAt,
                RestoredByName = U(x.RestoredById),
                HasHistory = hasHistory,
                CanRestore = canRestore
            };
        }
    }
}
