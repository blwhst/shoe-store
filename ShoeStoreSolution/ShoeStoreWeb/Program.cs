using Microsoft.EntityFrameworkCore;
using ShoeStoreData.Contexts;

var builder = WebApplication.CreateBuilder(args);

// Настройка базы данных для веб-приложения
// Используем тот же контекст, что и в API
builder.Services.AddDbContext<ShoeStoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавляем поддержку сессий
// Нужно для хранения данных авторизации между запросами (задание 5)
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

// Включаем сессии для хранения данных пользователя
app.UseSession();

// Включаем авторизацию (проверка прав доступа)
app.UseAuthorization();

// Подключаем все Razor Pages (страницы веб-приложения)
app.MapRazorPages();
app.Run();