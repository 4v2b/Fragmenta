using Fragmenta.Api.Contracts;
using Fragmenta.Api.Utils;
using System.Security.Cryptography;
using System.Text;

namespace Fragmenta.Api.Services
{
    public class Sha265HashingService : IHashingService
    {
        public byte[] Hash(string data, byte[]? salt = null)
        {
            using var sha256 = SHA256.Create();

            byte[] inputBytes = salt is null ? Encoding.UTF8.GetBytes(data) : salt.Concat(Encoding.UTF8.GetBytes(data)).ToArray();
            return sha256.ComputeHash(inputBytes);
        }

        public bool Verify(string data, byte[] hash, byte[]? salt = null)
        {
            using var sha256 = SHA256.Create();

            byte[] inputBytes = salt is null ? Encoding.UTF8.GetBytes(data) : salt.Concat(Encoding.UTF8.GetBytes(data)).ToArray();

            return sha256.ComputeHash(inputBytes).SequenceEqual(hash);
        }
    }
}
