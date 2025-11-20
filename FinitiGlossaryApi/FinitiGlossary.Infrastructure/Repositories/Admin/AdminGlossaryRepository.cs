using FinitiGlossary.Application.Interfaces.Repositories.Admin;
using FinitiGlossary.Domain.Entities.Terms;
using FinitiGlossary.Domain.Entities.Users;
using FinitiGlossary.Infrastructure.DAL;
using Microsoft.EntityFrameworkCore;

namespace FinitiGlossary.Infrastructure.Repositories.Admin
{
    public class AdminGlossaryRepository : IAdminGlossaryRepository
    {
        private readonly AppDbContext _db;

        public AdminGlossaryRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<List<GlossaryTerm>> GetActiveTermsForAdminViewAsync(string? userId, string? role)
        {
            IQueryable<GlossaryTerm> query = _db.GlossaryTerms.AsQueryable();

            bool isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

            // User vidi samo svoje; Admin vidi sve
            if (!isAdmin && userId != null)
            {
                query = query.Where(t => t.CreatedById == userId);
            }

            return query.ToListAsync();
        }


        public Task<List<ArchivedGlossaryTerm>> GetArchivedTermsForAdminViewAsync(string? userId, string? role)
        {
            IQueryable<ArchivedGlossaryTerm> query = _db.ArchivedGlossaryTerms.AsQueryable();

            bool isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && userId != null)
            {
                query = query.Where(a => a.CreatedById == userId);
            }

            return query.ToListAsync();
        }


        public Task<GlossaryTerm?> GetActiveByIdAsync(int id)
        {
            return _db.GlossaryTerms.FirstOrDefaultAsync(t => t.Id == id);
        }

        public Task<GlossaryTerm?> GetActiveByStableIdAsync(Guid stableId)
        {
            return _db.GlossaryTerms.FirstOrDefaultAsync(t => t.StableId == stableId);
        }

        public Task<List<ArchivedGlossaryTerm>> GetArchivedByStableIdAsync(Guid stableId)
        {
            return _db.ArchivedGlossaryTerms
                .Where(a => a.StableId == stableId)
                .OrderByDescending(a => a.Version)
                .ToListAsync();
        }

        public Task<ArchivedGlossaryTerm?> GetArchivedVersionAsync(Guid stableId, int version)
        {
            return _db.ArchivedGlossaryTerms
                .FirstOrDefaultAsync(a => a.StableId == stableId && a.Version == version);
        }

        public async Task<int> GetLatestVersionAsync(Guid stableId)
        {
            var latestArchived = await _db.ArchivedGlossaryTerms
                .Where(x => x.StableId == stableId)
                .MaxAsync(x => (int?)x.Version) ?? 0;

            var latestActive = await _db.GlossaryTerms
                .Where(x => x.StableId == stableId)
                .MaxAsync(x => (int?)x.Version) ?? 0;

            return Math.Max(latestArchived, latestActive);
        }

        public void AddActiveTerm(GlossaryTerm term)
        {
            _db.GlossaryTerms.Add(term);
        }

        public void AddDraftTerm(GlossaryTerm term)
        {
            _db.GlossaryTerms.Add(term);
        }

        public void RemoveActiveTerm(GlossaryTerm term)
        {
            _db.GlossaryTerms.Remove(term);
        }

        public void AddArchivedTerm(ArchivedGlossaryTerm term)
        {
            _db.ArchivedGlossaryTerms.Add(term);
        }
        public Task<List<User>> GetAllUsersAsync()
        {
            return _db.Users.ToListAsync();
        }

        public void UpdateArchivedTerm(ArchivedGlossaryTerm term)
        {
            _db.ArchivedGlossaryTerms.Update(term);
        }

        public async Task<bool> CreateAsync(GlossaryTerm term)
        {
            _db.GlossaryTerms.Add(term);
            return await SaveChangesAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
