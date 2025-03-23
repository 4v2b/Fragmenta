namespace Fragmenta.Api.Dtos
{
    public class CreateOrUpdateTaskRequest
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required DateTime? DueDate { get; set; }
        public required long StatusId { get; set; }
        public required long? AssigneeId { get; set; }
        public required float Weight { get; set; }
        public required int Priority { get; set; }
        public required List<long> TagsId { get; set; }
    }
}
