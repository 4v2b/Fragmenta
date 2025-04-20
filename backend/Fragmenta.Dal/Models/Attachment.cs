using Fragmenta.Dal.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Fragmenta.Dal.Models
{
    [EntityTypeConfiguration(typeof(AttachmentEntityTypeConfiguration))]
    public class Attachment : EntityBase
    {
        public string OriginalName { get; set; } = null!;
        public string FileName { get; set; } = null!;

        public long TypeId { get; set; }
        public AttachmentType Type { get; set; } = null!;
        
        public long SizeBytes { get; set; }
        public DateTime CreatedAt { get; set; }

        public long TaskId { get; set; }
        public Task Task { get; set; } = null!;
    }
}
