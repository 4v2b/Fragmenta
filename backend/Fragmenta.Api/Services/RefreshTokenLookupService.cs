using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Dal;
using Fragmenta.Dal.Models;
using Microsoft.EntityFrameworkCore;

namespace Fragmenta.Api.Services;

public class RefreshTokenLookupService : IRefreshTokenLookupService
{
    
    private readonly ILogger<RefreshTokenLookupService> _logger;
    private readonly IHashingService _hasher;
    private readonly ApplicationContext _context;

    public RefreshTokenLookupService(ILogger<RefreshTokenLookupService> logger, IHashingService hasher, ApplicationContext context)
    {
        _logger = logger;
        _hasher = hasher;
        _context = context;
    }
    
    // TODO Review sequence equals with quering the database
    public async Task<UserDto?> GetUserByTokenAsync(string token)
    {
        var hashedToken = _hasher.Hash(token);

        var user = (await _context.RefreshTokens.Include(e => e.User).SingleOrDefaultAsync(e => e.TokenHash.SequenceEqual(hashedToken)))?.User;
            
        if(user == null)
        {
            return null;
        }
        
        _logger.LogInformation("User {Id} - {Email} retrieved", user.Id ,user.Email);
        return new UserDto() { Email = user.Email, Id = user.Id };
    }

    public async Task<bool> HasValidTokenAsync(long userId)
    {
        return await _context.RefreshTokens.AnyAsync(e => e.UserId == userId && e.RevokedAt == null && e.ExpiresAt > DateTime.UtcNow);
    }
}