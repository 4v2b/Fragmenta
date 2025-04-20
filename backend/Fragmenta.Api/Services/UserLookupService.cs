using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Utils;
using Fragmenta.Dal;
using Microsoft.Extensions.Caching.Memory;

namespace Fragmenta.Api.Services;

public class UserLookupService : IUserLookupService
{
    private readonly ApplicationContext _context;
        private readonly ILogger<UserLookupService> _logger;
        private readonly IHashingService _hasher;

        public UserLookupService(ApplicationContext context, ILogger<UserLookupService> logger, IHashingService hasher)
        {
            _context = context;
            _logger = logger;
            _hasher = hasher;
        }

        public List<UserDto> FindManyByEmails(string[] emails)
        {
            return _context.Users
                .Where(e => emails.Contains(e.Email))
                .Select(e => new UserDto() { Email = e.Email, Id = e.Id})
                .ToList();
        }

        public List<UserDto> FindByEmail(string email)
        {
            return _context.Users
                .Where(e => e.Email.Contains(email))
                .Select(e => new UserDto() { Email = e.Email, Id = e.Id })
                .ToList();
        }

        public UserFullDto? GetUserInfo(long userId)
        {
            var user = _context.Users.Find(userId);

            if(user == null)
            {
                return null;
            }

            return new UserFullDto() { Email = user.Email, Id = user.Id, Name = user.Name };
        }

      
        public bool VerifyPassword(string password, long userId)
        {
            var user = _context.Users.SingleOrDefault(e => e.Id == userId)
                ?? throw new ArgumentException("No user found with given id", nameof(userId));

            return _hasher.Verify(password, user.PasswordHash, user.PasswordSalt);
        }

        public long? FindSingleByEmail(string email) => _context.Users.FirstOrDefault(e => e.Email == email)?.Id ?? null;
}