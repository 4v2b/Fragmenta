using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using Attachment = Fragmenta.Dal.Models.Attachment;
using Task = Fragmenta.Dal.Models.Task;

namespace Fragmenta.Dal
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }

        public ApplicationContext() { }

        public DbSet<ResetToken> ResetTokens { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<WorkspaceAccess> WorkspaceAccesses { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<AttachmentType> AttachmentTypes { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<BoardAccess> BoardAccesses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AttachmentType>()
                .HasMany(e => e.Boards)
                .WithMany(e => e.AttachmentTypes)
                .UsingEntity<AllowedType>(
                    j => j.HasOne(e => e.Board).WithMany(),
                    j => j.HasOne(e => e.Type).WithMany()
                );

            modelBuilder.Entity<Task>()
                .HasMany(e => e.Tags)
                .WithMany(e => e.Tasks)
                .UsingEntity<TaskTag>(
                    j => j.HasOne(e => e.Tag).WithMany().OnDelete(DeleteBehavior.NoAction),
                    j => j.HasOne(e => e.Task).WithMany()
                );

            modelBuilder.Entity<User>()
                .HasMany(e => e.Boards)
                .WithMany(e => e.Users)
                .UsingEntity<BoardAccess>();

            modelBuilder.Entity<BoardAccess>(e => e.ToTable("BoardsAccesses"));

            modelBuilder.Entity<AllowedType>(e => e.ToTable("AllowedTypes"));

            modelBuilder.Entity<TaskTag>(e => e.ToTable("TaskTags"));

        }
    }
}
