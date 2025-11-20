using FinitiGlossary.Application.Aggregators.AdminHistory;
using FinitiGlossary.Application.Aggregators.AdminView;
using FinitiGlossary.Application.DTOs.Request;
using FinitiGlossary.Application.Interfaces.Admin;
using FinitiGlossary.Application.Interfaces.Repositories.Admin;
using FinitiGlossary.Domain.Entities.Status;
using FinitiGlossary.Domain.Entities.Terms;
using FinitiGlossary.Domain.Interfaces.Repositories.UserIRepo;
using System.Security.Claims;


namespace FinitiGlossary.Application.Services.Admin
{
    public class AdminGlossaryService : IAdminGlossaryService
    {
        private readonly IAdminGlossaryRepository _repo;
        private readonly GlossaryAdminViewAggregator _viewAgregator;
        private readonly GlossaryAdminViewHistoryAggregator _viewHistoryAggregator;
        private readonly IUserRepository _userRepo;

        public AdminGlossaryService(
            IAdminGlossaryRepository repo,
            GlossaryAdminViewAggregator viewAgregator,
            GlossaryAdminViewHistoryAggregator viewHistoryAggregator,
            IUserRepository userRepo)
        {
            _repo = repo;
            _viewAgregator = viewAgregator;
            _viewHistoryAggregator = viewHistoryAggregator;
            _userRepo = userRepo;
        }

        private string GetUserId(ClaimsPrincipal user)
        {
            return user.FindFirstValue("id") ?? "system";
        }

        public async Task<object> GetAllForAdminAsync(
            ClaimsPrincipal user, int offset, int limit, string sort, string? search, string tab)
        {
            try
            {
                var role = user.FindFirstValue(ClaimTypes.Role);
                var userId = user.FindFirst("id")?.Value;

                var active = await _repo.GetActiveTermsForAdminViewAsync(userId, role);
                var archived = await _repo.GetArchivedTermsForAdminViewAsync(userId, role);
                var users = await _repo.GetAllUsersAsync();

                var aggregated = _viewAgregator.AggregateAdminView(
                    active, archived, users, int.Parse(userId!), role == "Admin"
                );

                var statusFilter = MapTabToStatus(tab);
                if (statusFilter != null)
                    aggregated = aggregated.Where(x => x.Status == (int)statusFilter.Value).ToList();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim().ToLower();
                    aggregated = aggregated
                        .Where(x => x.Term.ToLower().Contains(s) || x.Definition.ToLower().Contains(s))
                        .ToList();
                }

                aggregated = sort switch
                {
                    "dateAsc" => aggregated.OrderBy(x => x.CreatedOrArchivedAt).ToList(),
                    "dateDesc" => aggregated.OrderByDescending(x => x.CreatedOrArchivedAt).ToList(),
                    "az" => aggregated.OrderBy(x => x.Term).ToList(),
                    "za" => aggregated.OrderByDescending(x => x.Term).ToList(),
                    _ => aggregated.OrderByDescending(x => x.CreatedOrArchivedAt).ToList(),
                };

                var total = aggregated.Count;
                var page = aggregated.Skip(offset).Take(limit).ToList();

                return new
                {
                    Meta = new
                    {
                        Offset = offset,
                        Limit = limit,
                        Total = total,
                        HasMore = offset + limit < total,
                        Sort = sort,
                        Search = search,
                        Tab = tab
                    },
                    Data = page
                };
            }
            catch
            {

                return new { message = "An unexpected server error occurred during data retrieval." };
            }
        }

        public async Task<object> ArchiveAsync(int id, ClaimsPrincipal user)
        {
            try
            {
                var userId = GetUserId(user);
                var termToArchive = await _repo.GetActiveByIdAsync(id);
                if (termToArchive == null)
                    return new { message = "Term not found." };

                var latestVersion = await _repo.GetLatestVersionAsync(termToArchive.StableId);

                var archived = new ArchivedGlossaryTerm
                {
                    OriginalTermId = termToArchive.Id,
                    StableId = termToArchive.StableId,
                    Term = termToArchive.Term,
                    Definition = termToArchive.Definition,
                    ArchivedAt = DateTime.UtcNow,
                    ArchivedById = userId,
                    CreatedById = termToArchive.CreatedById,
                    ChangeSummary = "Manual archive",
                    Version = latestVersion + 1
                };

                _repo.AddArchivedTerm(archived);
                _repo.RemoveActiveTerm(termToArchive);

                var success = await _repo.SaveChangesAsync();
                if (!success)
                    return new { message = "Failed to archive term." };

                return new { message = "Term archived successfully." };
            }
            catch
            {

                return new { message = "An unexpected server error occurred during archive operation." };
            }
        }

        public async Task<object> RestoreAsync(Guid stableId, int version, ClaimsPrincipal user)
        {
            try
            {
                var userId = GetUserId(user);

                var archivedVersion = await _repo.GetArchivedVersionAsync(stableId, version);
                if (archivedVersion == null)
                    return new { message = "Requested version not found." };

                var existingActive = await _repo.GetActiveByStableIdAsync(stableId);

                if (existingActive != null)
                {
                    bool identical =
                        existingActive.Term.Trim() == archivedVersion.Term.Trim() &&
                        existingActive.Definition.Trim() == archivedVersion.Definition.Trim();

                    if (identical)
                    {
                        return new
                        {
                            restored = false,
                            message = "Identical version already active — no restore needed.",
                            stableId
                        };
                    }
                }

                if (existingActive != null)
                {
                    var nextVersion = await _repo.GetLatestVersionAsync(stableId) + 1;

                    var autoArchived = new ArchivedGlossaryTerm
                    {
                        OriginalTermId = existingActive.Id,
                        StableId = existingActive.StableId,
                        Term = existingActive.Term,
                        Definition = existingActive.Definition,
                        ArchivedAt = DateTime.UtcNow,
                        ArchivedById = userId,
                        CreatedById = existingActive.CreatedById,
                        ChangeSummary = "Auto-archived before restore",
                        Version = nextVersion
                    };

                    _repo.AddArchivedTerm(autoArchived);
                    _repo.RemoveActiveTerm(existingActive);
                }

                var restored = new GlossaryTerm
                {
                    StableId = archivedVersion.StableId,
                    Term = archivedVersion.Term,
                    Definition = archivedVersion.Definition,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = archivedVersion.CreatedById,
                    Status = TermStatus.Published
                };

                archivedVersion.RestoredAt = DateTime.UtcNow;
                archivedVersion.RestoredById = userId;

                _repo.AddActiveTerm(restored);
                _repo.UpdateArchivedTerm(archivedVersion);

                var success2 = await _repo.SaveChangesAsync();
                if (!success2)
                    return new { message = "Failed to restore term." };

                return new
                {
                    restored = true,
                    message = $"Version {version} restored.",
                    stableId
                };
            }
            catch
            {

                return new { message = "An unexpected server error occurred during restore operation." };
            }
        }

        public async Task<object> GetHistoryAsync(Guid stableId, ClaimsPrincipal user)
        {
            try
            {
                var active = await _repo.GetActiveByStableIdAsync(stableId);
                var archived = await _repo.GetArchivedByStableIdAsync(stableId);
                var allUsers = await _userRepo.GetAllUsersAsync();

                if (active == null && archived.Count == 0)
                    return new { message = "No history found." };

                var activeList = active != null ? new List<GlossaryTerm> { active } : new List<GlossaryTerm>();
                return _viewHistoryAggregator.AggregateHistoryView(activeList, archived, allUsers);
            }
            catch
            {

                return new { message = "An unexpected server error occurred during history retrieval." };
            }
        }

        public async Task<object> CreateAsync(CreateGlossaryRequest request, ClaimsPrincipal user)
        {
            try
            {
                var userId = GetUserId(user);

                var draft = new GlossaryTerm
                {
                    StableId = Guid.NewGuid(),
                    Term = request.Term.Trim(),
                    Definition = request.Definition.Trim(),
                    Version = 1,
                    Status = TermStatus.Draft,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = userId
                };

                var success = await _repo.CreateAsync(draft);

                if (!success)
                    return new { message = "Failed to create draft." };

                return new { message = "Draft created successfully.", term = draft };
            }
            catch
            {

                return new { message = "An unexpected server error occurred during term creation." };
            }
        }

        public async Task<object> UpdateAsync(int id, UpdateGlossaryRequest request, ClaimsPrincipal user)
        {
            try
            {
                var userId = GetUserId(user);

                var active = await _repo.GetActiveByIdAsync(id);
                if (active == null)
                    return new { message = "Term not found." };

                var archivedVersions = await _repo.GetArchivedByStableIdAsync(active.StableId);


                bool identicalExists = archivedVersions.Any(a =>
                    a.Term.Trim() == active.Term.Trim() &&
                    a.Definition.Trim() == active.Definition.Trim()
                );

                if (!identicalExists)
                {
                    var latestVersion = await _repo.GetLatestVersionAsync(active.StableId);

                    var archived = new ArchivedGlossaryTerm
                    {
                        OriginalTermId = active.Id,
                        StableId = active.StableId,
                        Term = active.Term,
                        Definition = active.Definition,
                        ArchivedAt = DateTime.UtcNow,
                        ArchivedById = userId,
                        CreatedById = active.CreatedById,
                        ChangeSummary = "Updated",
                        Version = latestVersion + 1
                    };

                    _repo.AddArchivedTerm(archived);
                }

                _repo.RemoveActiveTerm(active);

                var updated = new GlossaryTerm
                {
                    StableId = active.StableId,
                    Term = request.Term,
                    Definition = request.Definition,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = active.CreatedById,
                    Status = TermStatus.Published
                };

                _repo.AddActiveTerm(updated);

                var success = await _repo.SaveChangesAsync();
                if (!success)
                    return new { message = "Failed to update term." };

                return new { message = "Term updated successfully." };
            }
            catch
            {

                return new { message = "An unexpected server error occurred during term update." };
            }
        }

        public async Task<object> PublishAsync(int id, ClaimsPrincipal user)
        {
            try
            {
                var userId = GetUserId(user);
                var term = await _repo.GetActiveByIdAsync(id);

                if (term == null)
                    return new { message = "Term not found." };

                if (term.Status == TermStatus.Published)
                    return new { message = "Already published." };

                term.Status = TermStatus.Published;

                var success = await _repo.SaveChangesAsync();
                if (!success)
                    return new { message = "Failed to publish." };

                return new { message = "Term published." };
            }
            catch
            {

                return new { message = "An unexpected server error occurred during publishing." };
            }
        }

        public async Task<object> DeleteAsync(int id, ClaimsPrincipal user)
        {
            try
            {
                var term = await _repo.GetActiveByIdAsync(id);
                if (term == null)
                    return new { message = "Not found." };

                _repo.RemoveActiveTerm(term);
                var success = await _repo.SaveChangesAsync();

                return success
                    ? new { message = "Deleted." }
                    : new { message = "Failed to delete term." };
            }
            catch
            {
                // Hvatanje neočekivanih grešaka (DB brisanje)
                return new { message = "An unexpected server error occurred during deletion." };
            }
        }

        private TermStatus? MapTabToStatus(string tab)
        {
            return tab?.ToLower() switch
            {
                "draft" => TermStatus.Draft,
                "published" => TermStatus.Published,
                "archived" => TermStatus.Archived,
                _ => null
            };
        }
    }
}