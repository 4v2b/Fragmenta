using System.ComponentModel.DataAnnotations;

namespace Fragmenta.Api.Dtos
{
    public class CreateBoardRequest
    {
        [MinLength(1)]
        [MaxLength(100)]
        public required string Name { get; set; }

        public required List<long> AllowedTypeIds { get; set; } = [];
    }
}
