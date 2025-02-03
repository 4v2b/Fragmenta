namespace Fragmenta.Dal.Models
{
    public class BoardAccess
    {
        public long BoardId { get; set; }
        public Board Board { get; set; } = null!;

        public long UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
