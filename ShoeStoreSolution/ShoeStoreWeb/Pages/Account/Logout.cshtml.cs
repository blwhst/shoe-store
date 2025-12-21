using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ShoeStoreWeb.Pages.Account
{
    public class Logout : PageModel
    {
        // Часть задания 5: выход из системы
        // Очищает сессию, после чего пользователь становится неавторизованным
        public IActionResult OnPost()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Products/Index");
        }
    }
}