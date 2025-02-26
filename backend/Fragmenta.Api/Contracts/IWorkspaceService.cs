using Fragmenta.Api.Dtos;
using Fragmenta.Api.Enums;

namespace Fragmenta.Api.Contracts
{
    #pragma warning disable CS1591
    public interface IWorkspaceService
    {
        List<WorkspaceRoleDto> GetAll(long userId);

        bool Delete(long workspaceId);

        WorkspaceDto? Update(string name, long workspaceId);

        WorkspaceDto? Create(string name, long ownerId);

    }
}