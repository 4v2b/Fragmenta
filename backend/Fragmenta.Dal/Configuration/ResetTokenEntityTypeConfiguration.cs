using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fragmenta.Dal.Configuration
{
    public class ResetTokenEntityTypeConfiguration : IEntityTypeConfiguration<ResetToken>
    {
        public void Configure(EntityTypeBuilder<ResetToken> builder)
        {
            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.TokenHash)
                .HasMaxLength(256);
        }
    }
}
