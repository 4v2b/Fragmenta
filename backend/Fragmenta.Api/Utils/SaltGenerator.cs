using System.Security.Cryptography;

namespace Fragmenta.Api.Utils
{
    public static class SaltGenerator
    {
        public static byte[] GenerateSalt(int size = 16)
        {
            byte[] saltBytes = new byte[size];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(saltBytes);
            return saltBytes;
        }
    }
}
