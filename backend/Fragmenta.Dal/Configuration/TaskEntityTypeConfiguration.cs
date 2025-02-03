using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task = Fragmenta.Dal.Models.Task;

namespace Fragmenta.Dal.Configuration
{
    public class TaskEntityTypeConfiguration : IEntityTypeConfiguration<Task>
    {
        public void Configure(EntityTypeBuilder<Task> builder)
        {
            builder.Property(e => e.Title)
                .HasMaxLength(50);

            builder.Property(e => e.Description)
                .HasMaxLength(150);
        }
    }
}
