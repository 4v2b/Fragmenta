using System.ComponentModel.DataAnnotations;

namespace Fragmenta.Api.Dtos
{
    public class CreateOrUpdateWorkspaceRequest
    {
        [MinLength(1)]
        [MaxLength(100)]
        public required string Name { get; set; }
    }
}
