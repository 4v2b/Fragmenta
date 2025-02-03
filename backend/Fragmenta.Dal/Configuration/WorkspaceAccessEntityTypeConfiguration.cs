using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fragmenta.Dal.Configuration
{
    public class WorkspaceAccessEntityTypeConfiguration : IEntityTypeConfiguration<WorkspaceAccess>
    {
        public void Configure(EntityTypeBuilder<WorkspaceAccess> builder)
        {
            builder.HasKey(e => new { e.WorkspaceId, e.UserId });

            builder.HasOne(e => e.User)
                .WithMany(e => e.WorkspaceAccesses)
                .HasForeignKey(e => e.UserId);
            
            builder.HasOne(e => e.Workspace)
                .WithMany()
                .HasForeignKey(e => e.WorkspaceId);

            builder.HasOne(e => e.Role)
                .WithMany()
                .HasForeignKey(e => e.RoleId);
        }
    }
}
