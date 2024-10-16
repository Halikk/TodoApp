using Microsoft.AspNetCore.Mvc;
using TodoApp.Interfaces;
using TodoApp.Models;

namespace TodoApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoItemsController : ControllerBase
    {
        private readonly ITodoItemRepository _todoItemRepository;

        public TodoItemsController(ITodoItemRepository todoItemRepository)
        {
            _todoItemRepository = todoItemRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTodoItems()
        {
            var todoItems = await _todoItemRepository.GetAllTodoItemsAsync();
            return Ok(todoItems);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTodoItemById(int id)
        {
            var todoItem = await _todoItemRepository.GetTodoItemByIdAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }
            return Ok(todoItem);
        }

        [HttpPost]
        public async Task<IActionResult> AddTodoItem(TodoItem todoItem)
        {
            await _todoItemRepository.AddTodoItemAsync(todoItem);
            return CreatedAtAction(nameof(GetTodoItemById), new { id = todoItem.TodoItemId }, todoItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodoItem(int id, TodoItem todoItem)
        {
            if (id != todoItem.TodoItemId)
            {
                return BadRequest();
            }

            await _todoItemRepository.UpdateTodoItemAsync(todoItem);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(int id)
        {
            await _todoItemRepository.DeleteTodoItemAsync(id);
            return NoContent();
        }
    }
}
