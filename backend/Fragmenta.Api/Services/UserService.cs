using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Utils;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Fragmenta.Api.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<UserService> _logger;
        private readonly IHashingService _hasher;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan LockoutTime = TimeSpan.FromMinutes(10);
        private const int MaxAttempts = 3;

        public UserService(ApplicationContext context, ILogger<UserService> logger, IHashingService hasher, IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _hasher = hasher;
            _cache = cache;
        }

        public AuthResult Authorize(LoginRequest model)
        {
            var key = $"failed_attempts_{model.Email}";

            // Перевірка, чи користувач заблокований
            if (_cache.TryGetValue<(int Attempts, DateTime? LockedUntil)>(key, out var entry))
            {
                if (entry.LockedUntil.HasValue && entry.LockedUntil > DateTime.UtcNow)
                {
                    return AuthResult.Locked(entry.LockedUntil.Value);
                }
            }
            else
            {
                entry = (0, null);
            }

            var entity = _context.Users.FirstOrDefault(e => e.Email == model.Email);

            if (entity != null && _hasher.Verify(model.Password, entity.PasswordHash, entity.PasswordSalt))
            {
                _cache.Remove(key); // Видаляємо інформацію про спроби при успішному вході
                return AuthResult.Success(new UserDto { Email = entity.Email, Id = entity.Id });
            }

            // Збільшуємо кількість спроб
            entry = (entry.Attempts + 1, entry.Attempts + 1 >= MaxAttempts ? DateTime.UtcNow.Add(LockoutTime) : null);
            _cache.Set(key, entry, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = entry.LockedUntil.HasValue ? LockoutTime : TimeSpan.FromMinutes(15)
            });

            if(entity == null)
            {
                return AuthResult.Failed(Enums.ErrorType.UserNonExistent);
            }

            _logger.LogWarning("Cannot authorise user with email {Email}", model.Email);
            return entry.LockedUntil.HasValue ? AuthResult.Locked(entry.LockedUntil.Value) : AuthResult.Failed(Enums.ErrorType.PasswordInvalid);
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

        public AuthResult Register(RegisterRequest model)
        {
            if (_context.Users.Any(e => e.Email == model.Email))
            {
                return AuthResult.Failed(Enums.ErrorType.UserExists);
            }

            var salt = SaltGenerator.GenerateSalt();

            var entity = new User()
            {
                Email = model.Email,
                Name = model.Name,
                PasswordHash = _hasher.Hash(model.Password, salt),
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow
            };

            _context.Add(entity);
            _context.SaveChanges();

            return AuthResult.Success(new UserDto() { Email = entity.Email, Id = entity.Id });
        }

        public bool VerifyPassword(string password, long userId)
        {
            var user = _context.Users.SingleOrDefault(e => e.Id == userId)
                ?? throw new ArgumentException("No user found with given id", nameof(userId));

            return _hasher.Verify(password, user.PasswordHash, user.PasswordSalt);
        }
    }
}
