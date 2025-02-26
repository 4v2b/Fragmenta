namespace Fragmenta.Api.Dtos
{
    public class MemberDto
    {
        public required long Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; } 
    }
}
