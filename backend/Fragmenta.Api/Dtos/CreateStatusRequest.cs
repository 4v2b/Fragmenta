namespace Fragmenta.Api.Dtos
{
    public class CreateStatusRequest
    {
        public required string Name { get; set; }
        public required int? MaxTasks { get; set; }
        public required float Weight { get; set; }
        public required string ColorHex { get; set; }
    }
}
