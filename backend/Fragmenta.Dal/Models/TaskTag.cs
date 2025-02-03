namespace Fragmenta.Dal.Models
{
    public class TaskTag
    {
        public long TaskId { get; set; }
        public Task Task { get; set; } = null!;

        public long TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
