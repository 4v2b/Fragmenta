using Fragmenta.Api.Dtos;
using Fragmenta.Api.Enums;

namespace Fragmenta.Api.Contracts
{
    #pragma warning disable CS1591
    public interface IWorkspaceService
    {
        Task<List<WorkspaceRoleDto>> GetAllAsync(long userId);

        Task<bool> DeleteAsync(long workspaceId);

        Task<WorkspaceDto?> UpdateAsync(string name, long workspaceId);

        Task<WorkspaceDto?> CreateAsync(string name, long ownerId);

    }
}