using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Utils;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Fragmenta.Api.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationContext _context;
    private readonly ILogger<UserAccountService> _logger;
    private readonly IHashingService _hasher;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan LockoutTime = TimeSpan.FromMinutes(10);
    private const int MaxAttempts = 3;

    public AuthService(ApplicationContext context, ILogger<UserAccountService> logger, IHashingService hasher,
        IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _hasher = hasher;
        _cache = cache;
    }

    public async Task<AuthResult> AuthorizeAsync(LoginRequest model)
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

        var entity = await _context.Users.FirstOrDefaultAsync(e => e.Email == model.Email);

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

        if (entity == null)
        {
            return AuthResult.Failed(Enums.AuthErrorType.UserNonExistent);
        }

        _logger.LogWarning("Cannot authorise user with email {Email}", model.Email);
        return entry.LockedUntil.HasValue
            ? AuthResult.Locked(entry.LockedUntil.Value)
            : AuthResult.Failed(Enums.AuthErrorType.PasswordInvalid);
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest model)
    {
        if (await _context.Users.AnyAsync(e => e.Email == model.Email))
        {
            return AuthResult.Failed(Enums.AuthErrorType.UserExists);
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

        await _context.AddAsync(entity);
        await _context.SaveChangesAsync();

        return AuthResult.Success(new UserDto() { Email = entity.Email, Id = entity.Id });
    }
}