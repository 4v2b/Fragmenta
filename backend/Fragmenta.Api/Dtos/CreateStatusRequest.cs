using System.ComponentModel.DataAnnotations;

namespace Fragmenta.Api.Dtos
{
    public class CreateStatusRequest
    {
        [MinLength(1)]
        [MaxLength(50)]
        public required string Name { get; set; }
        public required int? MaxTasks { get; set; }
        public required float Weight { get; set; }
        public required string ColorHex { get; set; }
    }
}
