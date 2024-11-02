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
    [Route("[controller]")]
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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Giriş yapan kullanıcının ID'sini al
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account"); // Eğer giriş yapılmadıysa login'e yönlendir
            }

            IEnumerable<TodoItem> toDoItems;

            // Eğer kullanıcı adminse tüm to-do öğelerini getirin, değilse sadece kendi öğelerini getirin
            if (User.IsInRole("Admin"))
            {
                toDoItems = await _todoItemRepository.GetAllTodoItemsAsync();
            }
            else
            {
                toDoItems = await _todoItemRepository.GetTodoItemsByUserIdAsync(user.Id);
            }

            return View(toDoItems);
        }


        // Yeni TodoItem oluşturma sayfasını gösterir
        [HttpGet("create")]
        public IActionResult Create()
        {
            // IsCompleted için seçenek listesi oluşturun
            ViewBag.IsCompletedList = new List<SelectListItem>
    {
        new SelectListItem { Text = "Evet", Value = "true" },
        new SelectListItem { Text = "Hayır", Value = "false" }
    };

            return View();
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
        public async Task<IActionResult> ToggleCompletion(int id)
        {
            var todoItem = await _todoItemRepository.GetTodoItemByIdAsync(id);

            if (todoItem != null)
            {
                todoItem.IsCompleted = !todoItem.IsCompleted; // Durumu tersine çevir
                await _todoItemRepository.UpdateTodoItemAsync(todoItem);
            }

            return RedirectToAction(nameof(Index));
        }
    }

}
