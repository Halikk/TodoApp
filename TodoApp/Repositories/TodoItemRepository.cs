using Microsoft.EntityFrameworkCore;
using TodoApp.Interfaces;
using TodoApp.Models;

namespace TodoApp.Repositories
{
    public class TodoItemRepository : ITodoItemRepository
    {
        private readonly TodoDbContext _context;

        public TodoItemRepository(TodoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TodoItem>> GetAllTodoItemsAsync()
        {
            return await _context.TodoItems
        .Include(t => t.User) // User bilgilerini dahil et
        .ToListAsync();
        }

        public async Task<TodoItem> GetTodoItemByIdAsync(int id)
        {
            return await _context.TodoItems.FindAsync(id);
        }

        public async Task AddTodoItemAsync(TodoItem todoItem)
        {
            await _context.TodoItems.AddAsync(todoItem);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTodoItemAsync(TodoItem todoItem)
        {
            _context.TodoItems.Update(todoItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTodoItemAsync(int id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem != null)
            {
                _context.TodoItems.Remove(todoItem);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<TodoItem>> GetTodoItemsByUserIdAsync(int userId)
        {
            return await _context.TodoItems.Where(t => t.UserRef == userId).ToListAsync();
        }
    }
}
