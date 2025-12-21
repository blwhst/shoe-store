using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStoreData.Contexts;
using ShoeStoreData.Models;

namespace ShoeStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ShoeStoreDbContext _context;

        public ProductsController(ShoeStoreDbContext context) 
            => _context = context;

        // Получение всех товаров
        // Часть задания 3: доступно всем пользователям (даже без авторизации)
        // GET: api/products - доступно всем
        [HttpGet]
        public IActionResult GetProducts()
        {
            // Получаем все товары из базы данных
            var products = _context.Products.ToList();
            // Возвращаем список товаров в формате JSON
            return Ok(products);
        }

        // Получение товара по артикулу
        // Часть задания 3: доступно всем пользователям (даже без авторизации)
        // GET: api/products/{article} - доступно всем
        [HttpGet("{article}")]
        public IActionResult GetProduct(string article)
        {
            // Ищем товар по артикулу в базе данных
            var product = _context.Products.Find(article);
            // Если товар не найден - возвращаем 404
            if (product == null) 
                return NotFound();
            // Возвращаем найденный товар
            return Ok(product);
        }

        // Добавление нового товара
        // Часть задания 3: доступно только администраторам и менеджерам
        // POST: api/products - только Администратор/Менеджер
        [HttpPost]
        [Authorize(Roles = "Администратор,Менеджер")]
        public IActionResult PostProduct([FromBody] Product product)
        {
            // Добавляем новый товар в базу данных
            _context.Products.Add(product);
            // Сохраняем изменения
            _context.SaveChanges();
            // Возвращаем созданный товар с кодом 201 (Created)
            return CreatedAtAction(nameof(GetProduct), new { article = product.Article }, product);
        }

        // Обновление товара
        // Часть задания 3: доступно только администраторам и менеджерам
        // PUT: api/products/{article} - только Администратор/Менеджер
        [HttpPut("{article}")]
        [Authorize(Roles = "Администратор,Менеджер")]
        public IActionResult PutProduct(string article, [FromBody] Product product)
        {
            // Проверка, что артикул в URL совпадает с артикулом в теле запроса
            // Пример: если URL = /api/products/А112Т4, то в теле запроса 
            // должно быть { "Article": "А112Т4", ... }
            // Это защита от ошибок и подмены данных
            if (article != product.Article) 
                return BadRequest();

            // Говорим Entity Framework: "Этот товар был изменён пользователем"
            // Entity Framework поймёт, какие поля изменились в объекте product
            // и автоматически обновит только эти поля в базе данных
            // Например, если изменили только цену - обновит только цену
            // Если изменили и название, и цену - обновит и то, и другое
            _context.Entry(product).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            // Сохраняем изменения в базе данных
            _context.SaveChanges();
            // Возвращаем успех (без данных)
            return NoContent();
        }

        // Удаление товара
        // Часть задания 3: доступно только администраторам и менеджерам
        // DELETE: api/products/{article} - только Администратор/Менеджер
        [HttpDelete("{article}")]
        [Authorize(Roles = "Администратор,Менеджер")]
        public IActionResult DeleteProduct(string article)
        {
            // Ищем товар для удаления
            var product = _context.Products.Find(article);
            // Если товар не найден - возвращаем 404
            if (product == null) 
                return NotFound();

            // Удаляем товар из базы данных
            _context.Products.Remove(product);
            // Сохраняем изменения
            _context.SaveChanges();
            // Возвращаем код 204 (No Content) - успешное удаление
            return NoContent();
        }
    }
}