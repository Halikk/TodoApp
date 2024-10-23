using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Interfaces;
using TodoApp.Models;
using TodoApp.Repositories;
using BCrypt.Net;

public class AccountController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly SignInManager<User> _signInManager;

    public AccountController(IUserRepository userRepository, SignInManager<User> signInManager)
    {
        _userRepository = userRepository;
        _signInManager = signInManager;
    }


    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password) // Şifreyi hashleyin
            }; 

            // Kullanıcıyı veritabanına kaydedin
            await _userRepository.AddUserAsync(user);

            // Kullanıcıyı oturum açmış şekilde sisteme alın
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }
        return View(model);
    }


    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Kullanıcıyı e-posta ile bul
            var user = await _userRepository.GetUserByEmailAsync(model.Email);
            if (user != null)
            {
                // Şifreyi doğrula
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
                if (isPasswordValid)
                {
                    // Giriş işlemini gerçekleştir
                    await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);
                    return RedirectToAction("Index", "Home");
                }
            }

            // Geçersiz giriş denemesi
            ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
        }
        return View(model);
    }


    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }
}
