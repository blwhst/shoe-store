using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShoeStoreData.Contexts;
using ShoeStoreData.Models;

namespace ShoeStoreWeb.Pages.Orders
{
    public class IndexModel : PageModel
    {
        private readonly ShoeStoreDbContext _context;

        public IndexModel(ShoeStoreDbContext context)
        {
            _context = context;
        }

        public List<Order> Orders { get; set; } = new();

        // Часть задания 5: загрузка заказов в зависимости от роли пользователя
        // Клиенты видят только свои заказы, Администраторы/Менеджеры - все заказы
        public async Task<IActionResult> OnGetAsync()
        {
            // Проверка: если пользователь не авторизован - отправляем на страницу входа
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToPage("/Account/Login");
            }

            // Получаем ID и роль пользователя из сессии
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("Role");

            // Загружаем заказы из базы вместе с товарами в заказе и информацией о клиенте
            IQueryable<Order> query = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ArticleNavigation)
                .Include(o => o.Customer);

            // Если пользователь - клиент, показываем только его заказы
            if (userRole == "Клиент")
            {
                // Находим пользователя в базе по ID
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == int.Parse(userId));

                if (user != null)
                {
                    // Находим клиента по ФИО пользователя
                    var customer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.FullName == user.FullName);

                    if (customer != null)
                    {
                        // Фильтруем заказы: только где CustomerId совпадает с найденным клиентом
                        query = query.Where(o => o.CustomerId == customer.CustomerId);
                    }
                    else
                    {
                        // Если клиент не найден - создаём нового клиента
                        customer = new Customer { FullName = user.FullName };
                        _context.Customers.Add(customer);
                        await _context.SaveChangesAsync();

                        // Теперь фильтруем заказы по новому клиенту
                        query = query.Where(o => o.CustomerId == customer.CustomerId);
                    }
                }
                else
                {
                    // Если пользователь не найден - показываем пустой список
                    Orders = new List<Order>();
                    return Page();
                }
            }
            // Если пользователь - менеджер или администратор, показываем все заказы

            Orders = await query.ToListAsync();
            return Page();
        }

        // Часть задания 5: расчет итоговой стоимости заказа
        // Учитывает скидки на товары в составе заказа
        public decimal GetTotalPrice(Order order)
        {
            return order.OrderItems.Sum(oi =>
                (oi.ArticleNavigation.Price * (100 - oi.ArticleNavigation.Discount) / 100) * oi.Quantity);
        }

        // Часть задания 5: проверка роли пользователя
        // Используется в Razor странице для определения, показывать ли колонку "Клиент"
        public bool IsClient()
        {
            return HttpContext.Session.GetString("Role") == "Клиент";
        }
    }
}