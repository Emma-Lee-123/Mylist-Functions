namespace MyList.Function.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Category { get; set; }
        public string TaskName { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}