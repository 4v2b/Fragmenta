using Fragmenta.Dal.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Fragmenta.Dal.Models
{
    [EntityTypeConfiguration(typeof(BoardEntityTypeConfiguration))]
    public class Board : EntityBase
    {
        public string Name { get; set; } = null!;

        public DateTime? ArchivedAt { get; set; }

        public long WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;

        public List<Tag> Tags { get; set; } = [];
        public List<Status> Statuses { get; set; } = [];
        public List<User> Users { get; set; } = [];
        public List<AttachmentType> AttachmentTypes { get; set; } = [];
        public List<BoardAccess> AccessList { get; set; } = [];
    }
}
