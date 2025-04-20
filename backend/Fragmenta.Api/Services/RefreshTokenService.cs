using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Enums;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Fragmenta.Api.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        // 1. User logins into account - give a valid refresh token or generate one
        // 2. User sends refresh token to renew jwt
        // 2.1. Refresh token is valid, jwt generated
        // 2.2. Refresh token is valid, but expired, new token generated
        // 2.2. Refresh token invalid or revoked, return error, ask for re-login

        private readonly ILogger<RefreshTokenService> _logger;
        private readonly IHashingService _hasher;
        private readonly ApplicationContext _context;

        public RefreshTokenService(ILogger<RefreshTokenService> logger, IHashingService hasher, ApplicationContext context)
        {
            _logger = logger;
            _hasher = hasher;
            _context = context;
        }

        public string? GenerateToken(long userId)
        {
            if (_context.RefreshTokens.Any(e => e.UserId == userId && e.RevokedAt == null))
            {
                _logger.LogInformation("Cannot generate a refresh token. For user {Id} a valid token already exists", userId);
                return null;
            }

            var user = _context.Users.SingleOrDefault(e => e.Id == userId)
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

            _context.Add(entry);
            _context.SaveChanges();

            return token;
        }

        public string? RefreshToken(string refreshToken, long userId)
        {
            if (VerifyToken(refreshToken, userId) is not (RefreshTokenStatus.Valid or RefreshTokenStatus.Expired))
            {
                return null;
            }

            RevokeTokens(userId);
            return GenerateToken(userId);
        }

        public void RevokeTokens(long userId)
        {
            foreach (var token in _context.RefreshTokens.Where(e => e.UserId == userId && e.RevokedAt == null))
            {
                token.RevokedAt = DateTime.UtcNow;
                _context.RefreshTokens.Update(token);
            }
            _context.SaveChanges();
        }

        public RefreshTokenStatus VerifyToken(string refreshToken, long userId)
        {
            var tokens = _context.RefreshTokens
                .Where(e => e.UserId == userId && e.RevokedAt == null)
                .ToList();

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
