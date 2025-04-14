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

            builder.HasData(
                new AttachmentType { Id = 1, Value = "Any", ParentId = null }
            );
            
            // Root categories
            builder.HasData(
                new AttachmentType { Id = 11, Value = "Documents", ParentId = 1 },
                new AttachmentType { Id = 12, Value = "Images", ParentId = 1 },
                new AttachmentType { Id = 13, Value = "Audio", ParentId = 1 },
                new AttachmentType { Id = 14, Value = "Video", ParentId = 1 },
                new AttachmentType { Id = 15, Value = "Archives", ParentId = 1 },
                new AttachmentType { Id = 16, Value = "Code", ParentId = 1 },
                new AttachmentType { Id = 17, Value = "Data", ParentId = 1 },
                new AttachmentType { Id = 18, Value = "Design", ParentId = 1 }
            );

            // Documents
            builder.HasData(
                new AttachmentType { Id = 101, Value = ".txt", ParentId = 11 },
                new AttachmentType { Id = 102, Value = ".pdf", ParentId = 11 },
                new AttachmentType { Id = 103, Value = ".doc", ParentId = 11 },
                new AttachmentType { Id = 104, Value = ".docx", ParentId = 11 },
                new AttachmentType { Id = 105, Value = ".xls", ParentId = 11 },
                new AttachmentType { Id = 106, Value = ".xlsx", ParentId = 11 },
                new AttachmentType { Id = 107, Value = ".ppt", ParentId = 11 },
                new AttachmentType { Id = 108, Value = ".pptx", ParentId = 11 },
                new AttachmentType { Id = 109, Value = ".odt", ParentId = 11 },
                new AttachmentType { Id = 110, Value = ".ods", ParentId = 11 },
                new AttachmentType { Id = 111, Value = ".odp", ParentId = 11 },
                new AttachmentType { Id = 112, Value = ".md", ParentId = 11 },
                new AttachmentType { Id = 113, Value = ".rtf", ParentId = 11 }
            );

            // Images
            builder.HasData(
                new AttachmentType { Id = 201, Value = ".jpg", ParentId = 12 },
                new AttachmentType { Id = 202, Value = ".jpeg", ParentId = 12 },
                new AttachmentType { Id = 203, Value = ".png", ParentId = 12 },
                new AttachmentType { Id = 204, Value = ".gif", ParentId = 12 },
                new AttachmentType { Id = 205, Value = ".bmp", ParentId = 12 },
                new AttachmentType { Id = 206, Value = ".svg", ParentId = 12 },
                new AttachmentType { Id = 207, Value = ".webp", ParentId = 12 },
                new AttachmentType { Id = 208, Value = ".tiff", ParentId = 12 },
                new AttachmentType { Id = 209, Value = ".ico", ParentId = 12 }
            );

            // Audio
            builder.HasData(
                new AttachmentType { Id = 301, Value = ".mp3", ParentId = 13 },
                new AttachmentType { Id = 302, Value = ".wav", ParentId = 13 },
                new AttachmentType { Id = 303, Value = ".ogg", ParentId = 13 },
                new AttachmentType { Id = 304, Value = ".flac", ParentId = 13 },
                new AttachmentType { Id = 305, Value = ".m4a", ParentId = 13 },
                new AttachmentType { Id = 306, Value = ".aac", ParentId = 13 }
            );

            // Video
            builder.HasData(
                new AttachmentType { Id = 401, Value = ".mp4", ParentId = 14 },
                new AttachmentType { Id = 402, Value = ".avi", ParentId = 14 },
                new AttachmentType { Id = 403, Value = ".mkv", ParentId = 14 },
                new AttachmentType { Id = 404, Value = ".mov", ParentId = 14 },
                new AttachmentType { Id = 405, Value = ".wmv", ParentId = 14 },
                new AttachmentType { Id = 406, Value = ".webm", ParentId = 14 }
            );

            // Archives
            builder.HasData(
                new AttachmentType { Id = 501, Value = ".zip", ParentId = 15 },
                new AttachmentType { Id = 502, Value = ".rar", ParentId = 15 },
                new AttachmentType { Id = 503, Value = ".7z", ParentId = 15 },
                new AttachmentType { Id = 504, Value = ".tar", ParentId = 15 },
                new AttachmentType { Id = 505, Value = ".gz", ParentId = 15 }
            );

            // Code
            builder.HasData(
                new AttachmentType { Id = 601, Value = ".cs", ParentId = 16 },
                new AttachmentType { Id = 602, Value = ".js", ParentId = 16 },
                new AttachmentType { Id = 603, Value = ".ts", ParentId = 16 },
                new AttachmentType { Id = 604, Value = ".html", ParentId = 16 },
                new AttachmentType { Id = 605, Value = ".css", ParentId = 16 },
                new AttachmentType { Id = 606, Value = ".json", ParentId = 16 },
                new AttachmentType { Id = 607, Value = ".xml", ParentId = 16 },
                new AttachmentType { Id = 608, Value = ".py", ParentId = 16 },
                new AttachmentType { Id = 609, Value = ".java", ParentId = 16 },
                new AttachmentType { Id = 610, Value = ".php", ParentId = 16 },
                new AttachmentType { Id = 611, Value = ".sql", ParentId = 16 },
                new AttachmentType { Id = 612, Value = ".sh", ParentId = 16 }
            );

            // Data
            builder.HasData(
                new AttachmentType { Id = 701, Value = ".csv", ParentId = 17 },
                new AttachmentType { Id = 702, Value = ".xls", ParentId = 17 },
                new AttachmentType { Id = 703, Value = ".xlsx", ParentId = 17 },
                new AttachmentType { Id = 704, Value = ".xml", ParentId = 17 },
                new AttachmentType { Id = 705, Value = ".json", ParentId = 17 },
                new AttachmentType { Id = 706, Value = ".sqlite", ParentId = 17 },
                new AttachmentType { Id = 707, Value = ".accdb", ParentId = 17 }
            );

            // Design
            builder.HasData(
                new AttachmentType { Id = 801, Value = ".psd", ParentId = 18 },
                new AttachmentType { Id = 802, Value = ".ai", ParentId = 18 },
                new AttachmentType { Id = 803, Value = ".xd", ParentId = 18 },
                new AttachmentType { Id = 804, Value = ".fig", ParentId = 18 },
                new AttachmentType { Id = 805, Value = ".sketch", ParentId = 18 }
            );
        }
    }
}