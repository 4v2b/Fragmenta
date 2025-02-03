using Fragmenta.Dal.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Fragmenta.Dal.Models
{
    [EntityTypeConfiguration(typeof(TagEntityTypeConfiguration))]
    public class Tag : EntityBase
    {
        public string Name { get; set; } = null!;

        public List<Task> Tasks { get; set; } = [];

        public long BoardId { get; set; }
        public Board Board { get; set; } = null!;
    }
}
