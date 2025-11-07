using System.ComponentModel.DataAnnotations;


namespace Comprehension.Models
{
    public enum ResourceType
    {
        Note,
        Reminder,
        Event
    }

    public enum PermissionLevel
    {
        ReadOnly,       // Puede hacer GET
        ReadWrite,      // Puede hacer GET y PUT
        Admin           // Puede hacer todo porque tiene el poder absoluto
    }

    public class ResourcePermission
    {
        public Guid Id { get; set; }

        [Required]
        public required Guid ResourceId { get; set; }

        [Required]
        public required ResourceType ResourceType { get; set; }

        [Required]
        public required Guid SharedWithUserId { get; set; }

        public User? SharedWithUser { get; set; }

        [Required]
        public required Guid OwnerId { get; set; }

        public User? Owner { get; set; }

        [Required]
        public required PermissionLevel PermissionLevel { get; set; }

        public DateTime SharedAt { get; set; }
    }
}
