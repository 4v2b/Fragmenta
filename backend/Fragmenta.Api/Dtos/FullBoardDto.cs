namespace Fragmenta.Api.Dtos
{
    public class FullBoardDto
    {
        public required long Id { get; set; }
        public required string Name { get; set; }
        public List<StatusDto> Statuses { get; set; }
        public required List<long> AllowedTypeIds { get; set; }
    }
}
