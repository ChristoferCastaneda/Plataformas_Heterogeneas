namespace Cookies.Models
{
    public class Session
    {
        public int Id { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public User? User { get; set; }
    }
}
