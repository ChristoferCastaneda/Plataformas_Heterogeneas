using System.ComponentModel.DataAnnotations;

namespace Comprehension.Models
{
    public class User
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string Username { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public required string Email { get; set; }

        [Required]
        public required DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(256)]
        public required string PasswordHash { get; set; }

        [Required]
        [StringLength(256)]
        public required string PasswordSalt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
