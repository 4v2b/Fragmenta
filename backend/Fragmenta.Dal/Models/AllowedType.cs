namespace Fragmenta.Dal.Models
{
    public class AllowedType
    {
        public long BoardId { get; set; }
        public Board Board { get; set; } = null!;

        public long TypeId { get; set; }
        public AttachmentType Type { get; set; } = null!;
    }
}