using Fragmenta.Dal.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Fragmenta.Dal.Models
{
    [EntityTypeConfiguration(typeof(TaskEntityTypeConfiguration))]
    public class Task : EntityBase
    {
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public int Priority { get; set; }

        public float Weight { get; set; }

        public DateTime? DueDate { get; set; }

        public long? AssigneeId { get; set; }
        public User? Assignee { get; set; }

        public long StatusId { get; set; }
        public Status Status { get; set; } = null!;

        public List<Attachment> Attachments { get; set; } = [];
        public List<Tag> Tags { get; set; } = [];
    }
}
