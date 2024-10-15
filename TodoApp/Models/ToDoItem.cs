namespace TodoApp.Models
{
    public class TodoItem
    {
        public int TodoItemId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }

        // Hangi kullanıcıya ait olduğunu belirtir
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}
