using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShoeStoreData.Contexts;
using ShoeStoreData.DTOs;

namespace ShoeStoreWeb.Pages.Account
{
    public class Login : PageModel
    {
        private readonly ShoeStoreDbContext _context;

        public Login(ShoeStoreDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public LoginRequest LoginData { get; set; } = new();

        public string ErrorMessage { get; set; }

        // Часть задания 5: проверка авторизации при загрузке страницы
        // Если пользователь уже вошел - перенаправляем на страницу товаров
        public IActionResult OnGet()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToPage("/Products/Index");
            }
            return Page();
        }

        // Часть задания 5: обработка входа пользователя
        // Проверяет логин и пароль, сохраняет данные в сессии для отображения ФИО
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Поиск пользователя по логину и паролю
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Login == LoginData.Email && u.Password == LoginData.Password);

            if (user == null)
            {
                ErrorMessage = "Неверный логин или пароль";
                return Page();
            }

            // Сохранение данных пользователя в сессии
            // Это нужно для отображения ФИО в правом верхнем углу (задание 5)
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserLogin", user.Login);
            HttpContext.Session.SetString("Role", user.Role.RoleName);

            return RedirectToPage("/Products/Index");
        }
    }
}