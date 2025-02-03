using Fragmenta.Dal.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Fragmenta.Dal.Models
{
    [EntityTypeConfiguration(typeof(WorkspaceEntityTypeConfiguration))]
    public class Workspace : EntityBase
    {
        public string Name { get; set; } = null!;

        public List<Board> Boards { get; init; } = [];
    }
}
