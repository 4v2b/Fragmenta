using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Utils;
using Fragmenta.Dal;

namespace Fragmenta.Api.Services
{
    public class UserAccountService : IUserAccountService
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<UserAccountService> _logger;
        private readonly IHashingService _hasher;

        public UserAccountService(ApplicationContext context, ILogger<UserAccountService> logger,
            IHashingService hasher)
        {
            _context = context;
            _logger = logger;
            _hasher = hasher;
        }

        public bool ChangePassword(string newPassword, string oldPassword, long userId)
        {
            var user = _context.Users.SingleOrDefault(e => e.Id == userId)
                       ?? throw new ArgumentException("No user found with given id", nameof(userId));

            if (VerifyPassword(oldPassword, user.Id))
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

        public bool Delete(string password, long userId)
        {
            var user = _context.Users.SingleOrDefault(e => e.Id == userId)
                       ?? throw new ArgumentException("No user found with given id", nameof(userId));

            if (VerifyPassword(password, user.Id))
            {
                _context.Remove(user);
                _context.SaveChanges();

                return true;
            }

            return false;
        }

        public bool VerifyPassword(string password, long userId)
        {
            var user = _context.Users.SingleOrDefault(e => e.Id == userId)
                       ?? throw new ArgumentException("No user found with given id", nameof(userId));

            return _hasher.Verify(password, user.PasswordHash, user.PasswordSalt);
        }

        public bool ResetPassword(string newPassword, long userId)
        {
            var user = _context.Users.Find(userId);

            if (user != null)
            {
                var salt = SaltGenerator.GenerateSalt();

                var passwordHash = _hasher.Hash(newPassword, salt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = salt;

                _context.SaveChanges();

                return true;
            }

            return false;
        }
    }
}