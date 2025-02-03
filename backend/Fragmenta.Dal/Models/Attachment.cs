using Fragmenta.Dal.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Fragmenta.Dal.Models
{
    [EntityTypeConfiguration(typeof(AttachmentEntityTypeConfiguration))]
    public class Attachment : EntityBase
    {
        public string Value { get; set; } = null!;

        public long TypeId { get; set; }
        public AttachmentType Type { get; set; } = null!;

        public long? UserId { get; set; }
        public User? User { get; set; }

        public long TaskId { get; set; }
        public Task Task { get; set; } = null!;
    }
}
