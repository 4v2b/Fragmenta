using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fragmenta.Dal.Configuration
{
    public class WorkspaceEntityTypeConfiguration : IEntityTypeConfiguration<Workspace>
    {
        public void Configure(EntityTypeBuilder<Workspace> builder)
        {
            builder.Property(e => e.Name)
                .HasMaxLength(100);
        }
    }
}
