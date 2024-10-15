using Microsoft.EntityFrameworkCore;
using TodoApp.Models;

public class TodoDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<TodoItem> TodoItems { get; set; }

    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Kullanıcı ve görev arasındaki ilişki
        modelBuilder.Entity<User>()
            .HasMany(u => u.TodoItems)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId);
    }
}
