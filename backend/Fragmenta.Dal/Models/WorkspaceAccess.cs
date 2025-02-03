using Fragmenta.Dal.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Fragmenta.Dal.Models
{
    [EntityTypeConfiguration(typeof(WorkspaceAccessEntityTypeConfiguration))]
    public class WorkspaceAccess
    {
        public long WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;

        public long UserId { get; set; }
        public User User { get; set; } = null!;

        public long RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public DateTime JoinedAt { get; set; }
    }
}
