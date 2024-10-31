using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using TodoApp.Interfaces;
using TodoApp.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using TodoApp.Models;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Veritaban� ba�lant� dizesini kullanarak DbContext'i ekleyin
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity yap�land�rmas�: Kullan�c� ve Roller i�in (int ID ile)
builder.Services.AddIdentity<User, IdentityRole<int>>()  // int t�r�nde Id kullan�m�
    .AddEntityFrameworkStores<TodoDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);  // �erez s�resini belirleyin, �rne�in 30 dakika
    options.SlidingExpiration = false;  // Kullan�c� aktif olsa bile oturum s�resi uzat�lmaz
});

// Kullan�c� ve TodoItem Repository'leri
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITodoItemRepository, TodoItemRepository>();

// JSON se�enekleriyle birlikte MVC deste�i ekleyin
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// MVC deste�i ekleyin
builder.Services.AddControllersWithViews();

// Swagger deste�i
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware ayarlar�
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Routing middleware'i
app.UseRouting();


// Authentication ve Authorization middleware'lerini do�ru s�rayla kullan�n
app.UseAuthentication(); // Kimlik do�rulama i�lemi
app.UseAuthorization();  // Yetkilendirme i�lemi


// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
