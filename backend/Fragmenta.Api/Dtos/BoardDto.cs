using Fragmenta.Dal.Models;

namespace Fragmenta.Api.Dtos
{
    public class BoardDto
    {
        public required long Id { get; set; }
        public required string Name { get; set; }
        public DateTime? ArchivedAt { get; set; } = null;
        public List<long> GuestsId { get; set; } = [];
    }
}
