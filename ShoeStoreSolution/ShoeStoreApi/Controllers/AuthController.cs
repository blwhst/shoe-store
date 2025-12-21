using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShoeStoreData.Contexts;
using ShoeStoreData.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ShoeStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ShoeStoreDbContext _context;

        public AuthController(ShoeStoreDbContext context)
        {
            _context = context;
        }

        // Авторизация пользователя по логину и паролю с использованием JWT
        // Часть задания 3: проверяет данные пользователя и возвращает токен
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Поиск пользователя по email (логину) и паролю в базе данных
            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Login == request.Email && u.Password == request.Password);

            // Если пользователь не найден - возвращаем ошибку 401
            if (user == null)
            {
                return Unauthorized("Неверный логин или пароль");
            }

            // Проверяем, что у пользователя есть роль
            if (user.Role == null)
            {
                return Unauthorized("Ошибка: роль пользователя не найдена");
            }

            // Формируем данные о пользователе для включения в токен
            // Claims содержат информацию: логин, роль, ID и ФИО пользователя
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Role, user.Role.RoleName),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("FullName", user.FullName)
            };

            // Создание JWT токена с нашими настройками
            // Указываем издателя, получателя, данные пользователя и срок действия
            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(120)),
                signingCredentials: new SigningCredentials(
                    AuthOptions.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256));

            // Преобразуем токен в строку для отправки клиенту
            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            // Возвращаем токен и информацию о пользователе
            return Ok(new
            {
                token = token,
                role = user.Role.RoleName,
                name = user.FullName
            });
        }
    }
}