using Microsoft.IdentityModel.Tokens;
using System.Text;

// Настройки для JWT токенов - используются в API (задание 3)
public class AuthOptions
{
    // Наш сервер (кто выдаёт токены)
    public const string ISSUER = "ShoeStoreServer";
    // Клиентское приложение (кому выдаём токены)
    public const string AUDIENCE = "ShoeStoreClient";
    // Секретный ключ для подписи токенов
    // На практике нужно хранить в защищённом месте
    const string KEY = "my-32-character-ultra-secure-jwt-key-123";
    // Создаём ключ для подписи токенов
    public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
}