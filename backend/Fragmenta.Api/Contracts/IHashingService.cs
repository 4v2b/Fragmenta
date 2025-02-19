namespace Fragmenta.Api.Contracts
{
    public interface IHashingService
    {
        public byte[] Hash(string data, byte[]? salt = null);

        public bool Verify(string data, byte[] hash, byte[]? salt = null);
    }
}
