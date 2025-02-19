using Fragmenta.Dal.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;

namespace Fragmenta.Dal.Models
{
    [EntityTypeConfiguration(typeof(UserEntityTypeConfiguration))]
    public class User : EntityBase
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public byte[] PasswordHash { get; set; } = null!;
        public byte[] PasswordSalt { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public List<WorkspaceAccess> WorkspaceAccesses { get; set; } = [];
        public List<Board> Boards { get; set; } = [];
    }
}