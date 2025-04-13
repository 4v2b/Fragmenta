using Fragmenta.Dal.Models;

namespace Fragmenta.Api.Dtos
{
    public class BoardDto
    {
        public required long Id { get; set; }
        public required string Name { get; set; }
        public required DateTime? ArchivedAt { get; set; } = null;
        public required List<long> AllowedTypeIds { get; set; }
    }
}
