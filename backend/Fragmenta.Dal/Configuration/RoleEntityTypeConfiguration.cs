using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fragmenta.Dal.Configuration
{
    public class RoleEntityTypeConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.Property(e => e.Name)
                .HasMaxLength(50);

            builder.HasData(
                new Role { Id = 1, Name = "Owner" },
                new Role { Id = 2, Name = "Admin" },
                new Role { Id = 3, Name = "Member" },
                new Role { Id = 4, Name = "Guest" }
            );
        }
    }
}
