namespace Comprehension.Models
{
    public class Reminder
    {
        public Guid Id { get; internal set; }

        public required string Message { get; set; }

        public required DateTime ReminderTime { get; set; }

        public bool IsCompleted { get; set; } = false;
        public required Guid UserId { get; set; }
        public User? User { get; set; }

    }
}
