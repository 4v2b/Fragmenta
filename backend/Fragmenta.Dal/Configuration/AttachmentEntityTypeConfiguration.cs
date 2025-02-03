using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fragmenta.Dal.Configuration
{
    public class AttachmentEntityTypeConfiguration : IEntityTypeConfiguration<Attachment>
    {
        public void Configure(EntityTypeBuilder<Attachment> builder)
        {
            builder.Property(e => e.Value)
                .HasMaxLength(200);

            builder.HasOne(e => e.User)
                .WithMany()
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
