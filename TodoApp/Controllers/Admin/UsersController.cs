using Microsoft.AspNetCore.Mvc;
using TodoApp.Interfaces;
using TodoApp.Models;


[Route("admin/[controller]")]
public class UsersController : Controller
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users = await _userRepository.GetAllUsersAsync();
        return View(users);  // View döndürmek, HTML içeriği kullanıcıya sunar
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View();  // Boş formu döndürür
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(User user)
    {
        if (ModelState.IsValid)
        {
            await _userRepository.AddUserAsync(user);
            return RedirectToAction(nameof(Index));
        }
        return View(user);  // Eğer model geçersizse tekrar formu gösterir
    }

    // Benzer şekilde `Edit`, `Delete` gibi action metodlarını da yazmalısınız.
}
