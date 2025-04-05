namespace Fragmenta.Api.Dtos
{
    public class CreateBoardRequest
    {
        public required string Name { get; set; }

        public required List<long> AllowedTypeIds { get; set; } = [];
    }
}
