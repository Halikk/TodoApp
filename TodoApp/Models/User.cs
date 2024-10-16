namespace TodoApp.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        // Kullanıcının görevleri
        public virtual ICollection<TodoItem>? TodoItems { get; set; }
    }
}
