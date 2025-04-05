namespace Fragmenta.Api.Dtos
{
    public class AttachmentTypeDto
    {
        public long Id { get; set; }
        public string Value { get; set; } = null!;
        public List<AttachmentTypeDto> Children { get; set; } = [];
    }
}
