using Microsoft.AspNetCore.Mvc;
using TodoApp.Interfaces;
using TodoApp.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TodoApp.Controllers
{
    [Authorize]
    [Route("admin/[controller]")]
    public class TodoItemsController : Controller
    {
        private readonly ITodoItemRepository _todoItemRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TodoItemsController(ITodoItemRepository todoItemRepository, IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
        {
            _todoItemRepository = todoItemRepository;
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        // Tüm TodoItem'ları listelemek için Index view'ı döner
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var todoItems = await _todoItemRepository.GetAllTodoItemsAsync();
            return View(todoItems); // Tüm TodoItem'lar listesi görüntülenir
        }

        // Yeni TodoItem oluşturma sayfasını gösterir
        [HttpGet("create")]
        public IActionResult Create()
        {
            ViewBag.IsCompletedList = new SelectList(new List<SelectListItem>
    {
        new SelectListItem { Text = "Evet", Value = "true" },
        new SelectListItem { Text = "Hayır", Value = "false" }
    }, "Value", "Text");

            return View(); // Boş form döndürür
        }

        // Yeni TodoItem oluşturma işlemi
        [HttpPost("create")]
        public async Task<IActionResult> Create(TodoItem todoItem)
        {
            if (ModelState.IsValid)
            {
                await _todoItemRepository.AddTodoItemAsync(todoItem);
                return RedirectToAction(nameof(Index));
            }
            return View(todoItem); // Hata varsa formu tekrar döndürür
        }

        // TodoItem güncelleme sayfasını gösterir
        [HttpGet("update/{id}")]
        public IActionResult Update(int id)
        {
            var todoItem = _todoItemRepository.GetTodoItemByIdAsync(id).Result;

            if (todoItem == null)
            {
                return NotFound();
            }

            // Dropdown için seçenekleri ayarlıyoruz (true = Evet, false = Hayır)
            ViewBag.IsCompletedList = new SelectList(new List<SelectListItem>
    {
        new SelectListItem { Text = "Hayır", Value = "false" },
        new SelectListItem { Text = "Evet", Value = "true" }
    }, "Value", "Text", todoItem.IsCompleted ? "true" : "false");

            return View(todoItem);
        }


        [HttpPost("update/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, TodoItem todoItem)
        {
            // ID eşleşmesini kontrol et
            if (id != todoItem.TodoItemId)
            {
                return BadRequest(); // Eğer ID'ler uyuşmazsa hata döndür
            }

            // Model doğrulama hatası olup olmadığını kontrol et
            if (ModelState.IsValid)
            {
                try
                {
                    // Veritabanında güncelleme işlemini gerçekleştir
                    await _todoItemRepository.UpdateTodoItemAsync(todoItem);
                    // Başarılı olursa Index sayfasına yönlendir
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Eğer bir hata olursa logla ve hata mesajı göster
                    ModelState.AddModelError(string.Empty, $"Güncelleme sırasında bir hata oluştu: {ex.Message}");
                }
            }

            // Hatalı veri girildiyse formu tekrar kullanıcıya göster
            return View(todoItem);
        }


        // TodoItem silme sayfasını gösterir
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var todoItem = await _todoItemRepository.GetTodoItemByIdAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }
            return View(todoItem); // Silme onay sayfası
        }

        // TodoItem silme işlemi
        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _todoItemRepository.DeleteTodoItemAsync(id);
            return RedirectToAction(nameof(Index)); // Listeye geri döner
        }
        public async Task<IActionResult> MyProjects()
        {
            var userId = int.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var todoItems = await _todoItemRepository.GetTodoItemsByUserIdAsync(userId);
            return View(todoItems);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsCompleted(int id)
        {
            var todoItem = await _todoItemRepository.GetTodoItemByIdAsync(id);
            if (todoItem != null && todoItem.UserId == int.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                todoItem.IsCompleted = !todoItem.IsCompleted;
                await _todoItemRepository.UpdateTodoItemAsync(todoItem);
            }
            return RedirectToAction("MyProjects");
        }
    }
}
