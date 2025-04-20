using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fragmenta.Dal.Configuration
{
    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(e => e.Name)
                .HasMaxLength(100);

            builder.Property(e => e.Email)
                .HasMaxLength(100);

            builder.Property(e => e.PasswordHash)
                .HasMaxLength(256);

            builder.Property(e => e.PasswordSalt)
                .HasMaxLength(48);

            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
