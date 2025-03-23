namespace Fragmenta.Api.Dtos
{
    public class CreateOrUpdateStatusRequest
    {
        public required string Name { get; set; }
        public required string ColorHex { get; set; }
        public required int? MaxTasks { get; set; }
        public required float Weight { get; set; }
    }
}
