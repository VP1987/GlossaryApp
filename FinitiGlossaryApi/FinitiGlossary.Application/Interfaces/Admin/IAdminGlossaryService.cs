using FinitiGlossary.Application.DTOs.Request;
using System.Security.Claims;

namespace FinitiGlossary.Application.Interfaces.Admin
{
    public interface IAdminGlossaryService
    {

        Task<object> GetAllForAdminAsync(
         ClaimsPrincipal user,
         int offset,
         int limit,
         string sort,
         string? search,
         string tab);


        Task<object> PublishAsync(int id, ClaimsPrincipal user);
        Task<object> CreateAsync(CreateGlossaryRequest request, ClaimsPrincipal user);
        Task<object> UpdateAsync(int id, UpdateGlossaryRequest request, ClaimsPrincipal user);
        Task<object> ArchiveAsync(int id, ClaimsPrincipal user);
        Task<object> RestoreAsync(Guid stableId, int version, ClaimsPrincipal user);
        Task<object> GetHistoryAsync(Guid stableId, ClaimsPrincipal user);
        Task<object> DeleteAsync(int id, ClaimsPrincipal user);

    }
}