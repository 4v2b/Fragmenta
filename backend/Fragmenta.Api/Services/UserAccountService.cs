﻿using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Utils;
using Fragmenta.Dal;
using Microsoft.EntityFrameworkCore;

namespace Fragmenta.Api.Services
{
    public class UserAccountService : IUserAccountService
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<UserAccountService> _logger;
        private readonly IHashingService _hasher;

        public UserAccountService(ILogger<UserAccountService> logger, ApplicationContext context,
            IHashingService hasher)
        {
            _context = context;
            _logger = logger;
            _hasher = hasher;
        }

        public async Task<bool> ChangePasswordAsync(string newPassword, string oldPassword, long userId)
        {
            var user = await _context.Users.SingleOrDefaultAsync(e => e.Id == userId)
                       ?? throw new ArgumentException("No user found with given id", nameof(userId));

            if (_hasher.Verify(oldPassword, user.PasswordHash, user.PasswordSalt))
            {
                var salt = SaltGenerator.GenerateSalt();

                user.PasswordSalt = salt;
                user.PasswordHash = _hasher.Hash(newPassword, salt);
                _context.SaveChanges();

                return true;
            }

            return false;
        }

        public async Task<bool> ChangeNameAsync(string newUsername, long userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user is null)
            {
                return false;
            }

            user.Name = newUsername;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string password, long userId)
        {
            var user = await _context.Users.SingleOrDefaultAsync(e => e.Id == userId)
                       ?? throw new ArgumentException("No user found with given id", nameof(userId));

            var result = await _context.WorkspaceAccesses.AnyAsync(w => w.UserId == userId);
            
            if (!result && _hasher.Verify(password, user.PasswordHash, user.PasswordSalt))
            {
                _context.Remove(user);
                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> ResetPasswordAsync(string newPassword, long userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user != null)
            {
                var salt = SaltGenerator.GenerateSalt();

                var passwordHash = _hasher.Hash(newPassword, salt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = salt;

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }
    }
}