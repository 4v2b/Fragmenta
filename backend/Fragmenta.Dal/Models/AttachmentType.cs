using Fragmenta.Dal.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Fragmenta.Dal.Models
{
    [EntityTypeConfiguration(typeof(AttachmentTypeEntityTypeConfiguration))]
    public class AttachmentType : EntityBase
    {
        public string Value { get; set; } = null!;

        public long ParentId { get; set; }
        public AttachmentType? Parent { get; set; }

        public List<AttachmentType> Children { get; set; } = [];

        public List<Attachment> Attachments { get; set; } = [];

        public List<Board> Boards { get; set; } = [];
    }
}
