using System.ComponentModel.DataAnnotations;

namespace ShoeStoreData.DTOs
{
    // DTO для передачи данных авторизации
    // Используется в заданиях 3 (веб-API) и 6 (WPF приложение)
    public class LoginRequest
    {
        // Обязательное поле для email-логина
        // Валидация на корректность email формата
        [Required(ErrorMessage = "Введите логин")]
        [EmailAddress(ErrorMessage = "Введите корректный email")]
        [Display(Name = "Логин (email)")]
        public string Email { get; set; } = string.Empty;

        // Обязательное поле для пароля
        // Тип Password скрывает ввод символов в интерфейсе
        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;
    }
}