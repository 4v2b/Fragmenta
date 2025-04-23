using Fragmenta.Api.Contracts;
using Fragmenta.Api.Dtos;
using Fragmenta.Api.Utils;
using Fragmenta.Dal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Fragmenta.Api.Services;

public class UserLookupService : IUserLookupService
{
    private readonly ApplicationContext _context;
        private readonly ILogger<UserLookupService> _logger;
        private readonly IHashingService _hasher;

        public UserLookupService(ILogger<UserLookupService> logger, ApplicationContext context, IHashingService hasher)
        {
            _context = context;
            _logger = logger;
            _hasher = hasher;
        }

        public async Task<List<UserDto>> FindManyByEmailsAsync(string[] emails)
        {
            return await _context.Users
                .Where(e => emails.Contains(e.Email))
                .Select(e => new UserDto() { Email = e.Email, Id = e.Id})
                .ToListAsync();
        }

        public async Task<List<UserDto>> FindByEmailAsync(string email)
        {
            return await _context.Users
                .Where(e => e.Email.Contains(email))
                .Select(e => new UserDto() { Email = e.Email, Id = e.Id })
                .ToListAsync();
        }

        public async Task<UserFullDto?> GetUserInfoAsync(long userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if(user == null)
            {
                return null;
            }

            return new UserFullDto() { Email = user.Email, Id = user.Id, Name = user.Name };
        }

        public async Task<long?> FindSingleByEmailAsync(string email) => (await _context.Users.FirstOrDefaultAsync(e => e.Email == email))?.Id ?? null;
}