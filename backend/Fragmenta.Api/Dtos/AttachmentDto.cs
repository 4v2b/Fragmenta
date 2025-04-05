using Fragmenta.Dal.Models;

namespace Fragmenta.Api.Dtos
{
    public class AttachmentDto
    {
        public string Value { get; set; } = null!;

        public long TypeId { get; set; }
        public AttachmentType Type { get; set; } = null!;

        public long? UserId { get; set; }

        public long TaskId { get; set; }
    }
}
