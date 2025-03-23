namespace Fragmenta.Api.Dtos
{
    public class ShallowUpdateTaskRequest
    {
        public required long Id { get; set; }
        public required long StatusId { get; set; }
        public required float Weight { get; set; }
    }
}
