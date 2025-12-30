namespace FloraMind_V1.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; } = false;
        public int UserID { get; set; }
        public User User { get; set; }
    }
}
