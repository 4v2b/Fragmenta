using Fragmenta.Dal.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Fragmenta.Dal.Models
{
    [EntityTypeConfiguration(typeof(StatusEntityTypeConfiguration))]
    public class Status : EntityBase
    {
        public string Name { get; set; } = null!;

        public float Weight { get; set; }

        public string ColorHex { get; set; } = null!;

        public int TaskLimit { get; set; }

        public long BoardId { get; set; }
        public Board Board { get; set; } = null!;

        public List<Task> Tasks { get; init; } = [];
    }
}
