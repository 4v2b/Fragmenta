using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Utils;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using Microsoft.Extensions.Logging;

namespace Fragmenta.Api.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<UserService> _logger;
        private readonly IHashingService _hasher;

        public UserService(ApplicationContext context, ILogger<UserService> logger, IHashingService hasher)
        {
            _context = context;
            _logger = logger;
            _hasher = hasher;
        }

        public UserDto? Authorize(LoginRequest model)
        {
            var entity = _context.Users.FirstOrDefault(e => e.Email == model.Email);

            if(entity != null && _hasher.Verify(model.Password, entity.PasswordHash, entity.PasswordSalt))
            {
                return new UserDto() { Email = entity.Email, Id = entity.Id };
            }

            _logger.LogWarning("Cannot authorise a user with email {Email}", model.Email);

            return null;
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

        public UserDto? Register(RegisterRequest model)
        {
            if (_context.Users.Any(e => e.Email == model.Email))
            {
                return null;
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

            return new UserDto() { Email = entity.Email, Id = entity.Id };
        }

        public bool VerifyPassword(string password, long userId)
        {
            var user = _context.Users.SingleOrDefault(e => e.Id == userId)
                ?? throw new ArgumentException("No user found with given id", nameof(userId));

            return _hasher.Verify(password, user.PasswordHash, user.PasswordSalt);
        }
    }
}
