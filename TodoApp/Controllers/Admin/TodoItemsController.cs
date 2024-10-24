using Microsoft.AspNetCore.Mvc;
using TodoApp.Interfaces;
using TodoApp.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace TodoApp.Controllers
{
    [Authorize]
    [Route("admin/[controller]")]
    public class TodoItemsController : Controller
    {
        private readonly ITodoItemRepository _todoItemRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;

        public TodoItemsController(ITodoItemRepository todoItemRepository, IUserRepository userRepository, IHttpContextAccessor httpContextAccessor, UserManager<User> userManager)
        {
            _todoItemRepository = todoItemRepository;
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
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
                // Oturum açmış kullanıcının bilgilerini alıyoruz
                var currentUser = await _userManager.GetUserAsync(User);  // Oturum açmış kullanıcıyı al

                if (currentUser != null)
                {
                    // Kullanıcıyı TodoItem ile ilişkilendiriyoruz
                    todoItem.UserRef = currentUser.Id;  // currentUser.Id kullanıcının ID'si

                    // Veritabanına ekleyip kaydediyoruz
                    await _todoItemRepository.AddTodoItemAsync(todoItem);

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                }
            }
            return View(todoItem);
        }


        // TodoItem güncelleme sayfasını gösterir
        [HttpGet("update/{id}")]
        public async Task<IActionResult> Update(int id)
        {
            var todoItem = await _todoItemRepository.GetTodoItemByIdAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            // Dropdown için seçenekleri ayarlıyoruz (true = Evet, false = Hayır)
            ViewBag.IsCompletedList = new List<SelectListItem>
    {
        new SelectListItem { Text = "Evet", Value = "true", Selected = todoItem.IsCompleted },
        new SelectListItem { Text = "Hayır", Value = "false", Selected = !todoItem.IsCompleted }
    };

            return View(todoItem);
        }




        [HttpPost("update/{id}")]
        public async Task<IActionResult> Update(int id, TodoItem todoItem)
        {
            if (ModelState.IsValid)
            {
                // Oturum açmış kullanıcının kimliğini doğruluyoruz

                var currentUser = await _userRepository.GetUserByUserNameAsync(User.Identity.Name);

                if (currentUser != null)
                {
                    // Kullanıcının Id'sini TodoItem modeline atıyoruz
                    todoItem.UserRef = currentUser.Id;

                    // TodoItem'ı güncelliyoruz
                    await _todoItemRepository.UpdateTodoItemAsync(todoItem);

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                }
            }

            // Eğer validasyon hatası varsa, dropdown listesi yeniden doldurulmalı
            ViewBag.IsCompletedList = new List<SelectListItem>
    {
        new SelectListItem { Text = "Evet", Value = "true", Selected = todoItem.IsCompleted },
        new SelectListItem { Text = "Hayır", Value = "false", Selected = !todoItem.IsCompleted }
    };

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
        [HttpPost]
        public async Task<IActionResult> MarkAsCompleted(int id)
        {
            var todoItem = await _todoItemRepository.GetTodoItemByIdAsync(id);
            if (todoItem != null && todoItem.UserRef == int.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                todoItem.IsCompleted = !todoItem.IsCompleted;
                await _todoItemRepository.UpdateTodoItemAsync(todoItem);
            }
            return RedirectToAction("MyProjects");
        }

    }
}
