using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fragmenta.Dal.Configuration
{
    public class AttachmentEntityTypeConfiguration : IEntityTypeConfiguration<Attachment>
    {
        public void Configure(EntityTypeBuilder<Attachment> builder)
        {
            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            
            builder.Property(e => e.OriginalName)
                .HasMaxLength(200);

            builder.Property(e => e.FileName)
                .HasMaxLength(200);
        }
    }
}
