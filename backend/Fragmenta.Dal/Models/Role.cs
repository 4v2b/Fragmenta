using Fragmenta.Dal.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Fragmenta.Dal.Models
{
    [EntityTypeConfiguration(typeof(RoleEntityTypeConfiguration))]
    public class Role : EntityBase
    {
        public string Name { get; set; } = null!;
    }
}