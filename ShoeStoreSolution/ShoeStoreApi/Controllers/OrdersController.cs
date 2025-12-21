using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStoreData.Contexts;

namespace ShoeStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ShoeStoreDbContext _context;

        public OrdersController(ShoeStoreDbContext context)
        {
            _context = context;
        }

        // Получение заказов пользователя по его логину
        // Часть задания 3: доступно только авторизованным пользователям
        // GET: api/orders/user/{login} - нужна авторизация
        [HttpGet("user/{login}")]
        [Authorize]
        public IActionResult GetOrdersByUser(string login)
        {
            // Находим пользователя по логину (email)
            var user = _context.Users
                .FirstOrDefault(u => u.Login == login);

            // Если пользователь не найден - возвращаем 404
            if (user == null)
                return NotFound("Пользователь не найден");

            // Ищем заказы, где клиент совпадает с пользователем
            // Используем ФИО для связи пользователя и клиента
            var orders = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .Where(o => o.Customer.FullName == user.FullName)
                .ToList();

            return Ok(orders);
        }

        // Изменение статуса и даты доставки заказа
        // Часть задания 3: доступно только администраторам и менеджерам
        // PUT: api/orders/{id} - только Администратор/Менеджер
        [HttpPut("{id}")]
        [Authorize(Roles = "Администратор,Менеджер")]
        public IActionResult UpdateOrder(int id, [FromBody] OrderUpdateDto dto)
        {
            // Находим заказ по ID
            var order = _context.Orders.Find(id);
            if (order == null)
                return NotFound();

            // Обновляем статус заказа, если он указан в DTO
            // DTO может содержать только часть полей для обновления
            if (!string.IsNullOrEmpty(dto.Status))
                order.Status = dto.Status;

            // Обновляем дату доставки, если она указана в DTO
            if (dto.DeliveryDate.HasValue)
                order.DeliveryDate = dto.DeliveryDate.Value;

            // Сохраняем изменения в базе данных
            _context.SaveChanges();
            // Возвращаем обновлённый заказ
            return Ok(order);
        }
    }
}