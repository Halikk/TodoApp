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

// Veritabaný baðlantý dizesini kullanarak DbContext'i ekleyin
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity yapýlandýrmasý: Kullanýcý ve Roller için (int ID ile)
builder.Services.AddIdentity<User, IdentityRole<int>>()  // int türünde Id kullanýmý
    .AddEntityFrameworkStores<TodoDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);  // Çerez süresini belirleyin, örneðin 30 dakika
    options.SlidingExpiration = false;  // Kullanýcý aktif olsa bile oturum süresi uzatýlmaz
});

// Kullanýcý ve TodoItem Repository'leri
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITodoItemRepository, TodoItemRepository>();

// JSON seçenekleriyle birlikte MVC desteði ekleyin
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// MVC desteði ekleyin
builder.Services.AddControllersWithViews();

// Swagger desteði
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware ayarlarý
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


// Authentication ve Authorization middleware'lerini doðru sýrayla kullanýn
app.UseAuthentication(); // Kimlik doðrulama iþlemi
app.UseAuthorization();  // Yetkilendirme iþlemi


// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
