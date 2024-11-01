using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Interfaces;
using TodoApp.Models;
using TodoApp.Repositories;
using BCrypt.Net;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly IUserRepository _userRepository;
    private readonly SignInManager<User> _signInManager;

    public AccountController(IUserRepository userRepository, SignInManager<User> signInManager, UserManager<User> userManager)
    {
        _userManager = userManager;
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
                SecurityStamp = Guid.NewGuid().ToString()
                // PasswordHash'i burada manuel olarak set etmeye gerek yok, UserManager bu işlemi yapar
            };

            // Kullanıcıyı veritabanına kaydedin
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Kullanıcıya "User" rolünü atayın
                await _userManager.AddToRoleAsync(user, "User");

                // Kullanıcıyı oturum açmış şekilde sisteme alın
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            // Eğer kullanıcı oluşturma başarısızsa, hataları ModelState'e ekleyin
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // ModelState geçersizse veya kullanıcı oluşturulamadıysa formu tekrar gösterin
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
