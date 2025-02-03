using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace Fragmenta.Dal.Configuration
{
    public class AttachmentTypeEntityTypeConfiguration : IEntityTypeConfiguration<AttachmentType>
    {
        public void Configure(EntityTypeBuilder<AttachmentType> builder)
        {
            builder.Property(e => e.Value)
                .HasMaxLength(50);

            builder.HasMany(e => e.Children)
                .WithOne(e => e.Parent)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
