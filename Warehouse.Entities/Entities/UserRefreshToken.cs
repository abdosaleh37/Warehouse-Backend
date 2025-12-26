using System.ComponentModel.DataAnnotations;

namespace Warehouse.Entities.Entities
{
    public class UserRefreshToken
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        public string Token { get; set; } = string.Empty;

        public DateTime ExpiryDateUtc { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
