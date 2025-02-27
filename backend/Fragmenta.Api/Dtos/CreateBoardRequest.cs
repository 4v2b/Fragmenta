namespace Fragmenta.Api.Dtos
{
    public class CreateBoardRequest
    {
        public required string Name { get; set; }
        public long[] GuestsId { get; set; }
    }
}
