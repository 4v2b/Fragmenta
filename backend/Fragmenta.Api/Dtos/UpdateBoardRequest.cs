namespace Fragmenta.Api.Dtos
{
    public class UpdateBoardRequest
    {
        public required string Name { get; set; }

        public required DateTime? ArchivedAt { get; set; }
    }
}
