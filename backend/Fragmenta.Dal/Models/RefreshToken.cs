using Fragmenta.Dal.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Fragmenta.Dal.Models
{
    [EntityTypeConfiguration(typeof(RefreshTokenEntityTypeConfiguration))]
    public class RefreshToken : EntityBase
    {
        public string Token { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public long UserId { get; set; }
        public User User { get; set; } = null!;

    }
}
