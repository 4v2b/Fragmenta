﻿using Fragmenta.Api.Contracts;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace Fragmenta.Api.Services
{
    public class ResetTokenService : IResetTokenService
    {
        private readonly ILogger<ResetTokenService> _logger;
        private readonly ApplicationContext _context;
        private readonly IHashingService _hasher;
        private readonly TimeSpan _timeout = TimeSpan.FromMinutes(30);

        public ResetTokenService(ILogger<ResetTokenService> logger, ApplicationContext context, IHashingService hasher)
        {
            _logger = logger;
            _context = context;
            _hasher = hasher;
        }

        public async Task<string> GenerateTokenAsync(long userId)
        {
            var user = await _context.Users.SingleOrDefaultAsync(e => e.Id == userId)
                ?? throw new ArgumentException("No user found for given id", nameof(userId));

            if (await _context.ResetTokens.AnyAsync(e => e.UserId == userId))
            {
                var oldTokens = _context.ResetTokens.Where(e => e.UserId == userId).ToList();
                _context.RemoveRange(oldTokens);
                await _context.SaveChangesAsync();
            }

            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            string token = Convert.ToBase64String(randomBytes);

            var entry = new ResetToken
            {
                User = user,
                TokenHash = _hasher.Hash(token),
                ExpiresAt = DateTime.UtcNow.Add(_timeout)
            };

            await _context.AddAsync(entry);
            await _context.SaveChangesAsync();

            return token;
        }

        public async Task<bool> VerifyAndDestroyTokenAsync(string token, long userId)
        {
            var tokenHash = _hasher.Hash(token);

            var entity = await _context.ResetTokens.SingleOrDefaultAsync(e => e.UserId == userId && e.TokenHash.SequenceEqual(tokenHash));


            _logger.LogInformation("Token {Token} created at {Time1} expires at {Time}", token, entity?.CreatedAt.ToString(), entity?.ExpiresAt.ToString());

            if (entity is not null)
            {
                var expiresAt = entity.ExpiresAt;

                _context.Remove(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Token {Token} destroyed", token);


                if (expiresAt > DateTime.UtcNow)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
