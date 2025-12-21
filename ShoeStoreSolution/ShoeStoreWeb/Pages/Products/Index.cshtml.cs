using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShoeStoreData.Contexts;
using ShoeStoreData.Models;

namespace ShoeStoreWeb.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly ShoeStoreDbContext _context;

        public IndexModel(ShoeStoreDbContext context)
        {
            _context = context;
        }

        public List<Product> Products { get; set; } = new();
        public List<Manufacturer> Manufacturers { get; set; } = new();

        // Часть задания 4: загрузка товаров с фильтрацией, поиском и сортировкой
        // Все фильтры работают совместно
        public async Task OnGetAsync(
            string? search = null,
            int? manufacturerId = null,
            decimal? maxPrice = null,
            bool onlyDiscount = false,
            bool onlyInStock = false,
            string sortBy = "name")
        {
            // Загружаем производителей для выпадающего списка
            Manufacturers = await _context.Manufacturers
                .OrderBy(m => m.Name)
                .ToListAsync();

            var query = _context.Products
                .Include(p => p.Manufacturer)
                .Include(p => p.Supplier)
                .Include(p => p.Category)
                .AsQueryable();

            // Задание 4: поиск по части описания товара (без учета регистра)
            // Если есть поисковая строка - ищем в описании и названии
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(p =>
                    (p.Description != null && p.Description.ToLower().Contains(search)) ||
                    p.Name.ToLower().Contains(search));
            }

            // Задание 4: фильтрация по производителю
            // Если выбран производитель - фильтруем по нему
            if (manufacturerId.HasValue)
            {
                query = query.Where(p => p.ManufacturerId == manufacturerId.Value);
            }

            // Задание 4: фильтрация по цене не более указанной
            // Если указана максимальная цена - фильтруем
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // Задание 4: отображение только товаров со скидкой
            // Если отмечен флажок "Только со скидкой"
            if (onlyDiscount)
            {
                query = query.Where(p => p.Discount > 0);
            }

            // Задание 4: отображение только товаров в наличии
            // Если отмечен флажок "Только в наличии"
            if (onlyInStock)
            {
                query = query.Where(p => p.Quantity > 0);
            }

            // Задание 4: сортировка по названию, поставщику, цене
            query = sortBy switch
            {
                "name" => query.OrderBy(p => p.Name), // По названию (А-Я)
                "supplier" => query.OrderBy(p => p.Supplier.Name), // По имени поставщика
                "price" => query.OrderBy(p => p.Price * (100 - p.Discount) / 100), // По цене со скидкой (дешевле к дороже)
                "price_desc" => query.OrderByDescending(p => p.Price * (100 - p.Discount) / 100), // По цене со скидкой (дороже к дешевле)
                _ => query.OrderBy(p => p.Name) // По умолчанию сортируем по названию
            };

            Products = await query.ToListAsync();
        }

        // Часть задания 5: создание нового заказа при нажатии кнопки "Заказать"
        // Автоматически заполняет дату заказа, дату доставки, код получения и статус
        public async Task<IActionResult> OnPostOrderAsync(string article)
        {
            try
            {
                // Получаем данные пользователя из сессии
                var userId = HttpContext.Session.GetString("UserId");
                var userRole = HttpContext.Session.GetString("Role");

                // Если не авторизован - отправляем на вход
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToPage("/Account/Login");
                }

                // Только клиенты могут заказывать (не менеджеры/администраторы)
                if (userRole != "Клиент")
                {
                    TempData["ErrorMessage"] = "Только клиенты могут оформлять заказы";
                    return RedirectToPage("./Index");
                }

                // Находим товар по артикулу
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Article == article);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Товар не найден";
                    return RedirectToPage("./Index");
                }

                // Проверяем, есть ли товар на складе
                if (product.Quantity <= 0)
                {
                    TempData["ErrorMessage"] = $"Товар '{product.Name}' отсутствует на складе";
                    return RedirectToPage("./Index");
                }

                // Находим пользователя в базе
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserId == int.Parse(userId));

                if (user == null)
                {
                    TempData["ErrorMessage"] = "Пользователь не найден";
                    return RedirectToPage("./Index");
                }

                // Ищем клиента по ФИО пользователя
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.FullName == user.FullName);

                // Если клиента нет - создаём нового
                if (customer == null)
                {
                    customer = new Customer { FullName = user.FullName };
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                }

                // Создание заказа с автоматическим заполнением данных
                var order = new Order
                {
                    OrderDate = DateOnly.FromDateTime(DateTime.Today), // Дата заказа - сегодня
                    DeliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)), // Дата доставки - через неделю
                    CustomerId = customer.CustomerId,
                    PickupCode = new Random().Next(100, 1000), // Код получения - случайное число 100-999
                    Status = "Новый" // Статус - новый
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Добавление товара в состав заказа
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    Article = article,
                    Quantity = 1
                };

                _context.OrderItems.Add(orderItem);

                // Уменьшение количества товара на складе
                product.Quantity -= 1;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Заказ №{order.OrderId} успешно создан! Код получения: {order.PickupCode}";
                return RedirectToPage("./Index");
            }
            catch (DbUpdateException dbEx)
            {
                TempData["ErrorMessage"] = $"Ошибка базы данных при создании заказа. Возможно, товара недостаточно на складе.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                var errorDetails = ex.Message;
                if (ex.InnerException != null)
                {
                    errorDetails += $" | Внутренняя ошибка: {ex.InnerException.Message}";
                }

                TempData["ErrorMessage"] = $"Ошибка создания заказа: {errorDetails}";
                return RedirectToPage("./Index");
            }
        }
    }
}