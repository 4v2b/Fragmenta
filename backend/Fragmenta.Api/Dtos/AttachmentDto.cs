using Fragmenta.Dal.Models;

namespace Fragmenta.Api.Dtos
{
    public class AttachmentDto
    {
        public long Id { get; set; }
        public string OriginalName { get; set; } = null!;
        public string FileName { get; set; } = null!;
        
        public long SizeBytes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
