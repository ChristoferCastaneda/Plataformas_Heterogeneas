using System.ComponentModel.DataAnnotations;

namespace Comprehension.Models
{
    public class Session
    {
        [Key]
        [StringLength(256)]
        public required string SessionId { get; set; }

        [Required]
        public required Guid UserId { get; set; }

        public User? User { get; set; }

        [Required]
        public required DateTime CreatedAt { get; set; }

        [Required]
        public required DateTime LastActivityAt { get; set; }

        [Required]
        public required DateTime ExpiresAt { get; set; }

        public DateTime? ExpiredAt { get; set; }

        public bool IsValid()
        {
            // Token válido si no ha expirado manualmente o por tiempo
            return ExpiredAt == null && DateTime.UtcNow < ExpiresAt;
        }
    }
}