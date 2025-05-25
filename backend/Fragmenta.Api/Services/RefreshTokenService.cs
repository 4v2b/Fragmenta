using System.Runtime.InteropServices.ComTypes;
using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Enums;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Task = System.Threading.Tasks.Task;

namespace Fragmenta.Api.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {

        private readonly ILogger<RefreshTokenService> _logger;
        private readonly IHashingService _hasher;
        private readonly ApplicationContext _context;

        public RefreshTokenService(ILogger<RefreshTokenService> logger, IHashingService hasher, ApplicationContext context)
        {
            _logger = logger;
            _hasher = hasher;
            _context = context;
        }

        public async Task<string?> GenerateTokenAsync(long userId)
        {
            if (await _context.RefreshTokens.AnyAsync(e => e.UserId == userId && e.RevokedAt == null))
            {
                _logger.LogInformation("Cannot generate a refresh token. For user {Id} a valid token already exists", userId);
                return null;
            }

            var user = await _context.Users.SingleOrDefaultAsync(e => e.Id == userId)
                ?? throw new ArgumentException("No user found for given id", nameof(userId));

            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            string token = Convert.ToBase64String(randomBytes);

            var entry = new RefreshToken
            {
                User = user,
                TokenHash = _hasher.Hash(token),
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            await _context.AddAsync(entry);
            await _context.SaveChangesAsync();

            return token;
        }

        public async Task<string?> RefreshTokenAsync(string refreshToken, long userId)
        {
            if (await VerifyTokenAsync(refreshToken, userId) is not (RefreshTokenStatus.Valid or RefreshTokenStatus.Expired))
            {
                return null;
            }

            await RevokeTokensAsync(userId);
            return await GenerateTokenAsync(userId);
        }

        public async Task RevokeTokensAsync(long userId)
        {
            var revokedTokens = _context.RefreshTokens.Where(e => e.UserId == userId && e.RevokedAt != null);
            _context.RemoveRange(revokedTokens);
            await _context.SaveChangesAsync();
            
            foreach (var token in _context.RefreshTokens.Where(e => e.UserId == userId))
            {
                token.RevokedAt = DateTime.UtcNow;
                _context.RefreshTokens.Update(token);
            }
            await _context.SaveChangesAsync();
        }
        
        public async Task<RefreshTokenStatus> VerifyTokenAsync(string refreshToken, long userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(e => e.UserId == userId && e.RevokedAt == null)
                .ToListAsync();

            if (tokens.Count != 1 || !_hasher.Hash(refreshToken).SequenceEqual(tokens[0].TokenHash))
            {
                return RefreshTokenStatus.InvalidOrRevoked;
            }

            return tokens[0].ExpiresAt <= DateTime.UtcNow
                ? RefreshTokenStatus.Expired
                : RefreshTokenStatus.Valid;
        }
    }
}
