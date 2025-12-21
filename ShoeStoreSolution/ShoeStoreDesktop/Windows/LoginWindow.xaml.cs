using Microsoft.EntityFrameworkCore;
using ShoeStoreData.Contexts;
using ShoeStoreData.Models;
using System;
using System.Linq;
using System.Windows;

namespace ShoeStoreDesktop
{
    // Окно авторизации пользователей
    // Проверяет логин и пароль, определяет роль пользователя
    // Открывает главное окно при успешной авторизации
    public partial class LoginWindow : Window
    {
        public User CurrentUser { get; private set; } // Текущий авторизованный пользователь

        public LoginWindow()
        {
            InitializeComponent();
            LoginTextBox.Focus();
        }

        // Обработчик кнопки "Войти" - выполняет авторизацию
        private void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearError(); // Очищаем сообщения об ошибках

                // Проверка заполнения полей
                if (string.IsNullOrWhiteSpace(LoginTextBox.Text))
                {
                    ShowError("Введите логин");
                    LoginTextBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(PasswordTextBox.Password))
                {
                    ShowError("Введите пароль");
                    PasswordTextBox.Focus();
                    return;
                }

                using (var context = new ShoeStoreDbContext())
                {
                    // Поиск пользователя в базе данных
                    var user = context.Users
                        .Include(u => u.Role) // Включаем данные о роли
                        .FirstOrDefault(u => u.Login == LoginTextBox.Text && u.Password == PasswordTextBox.Password);

                    if (user == null)
                    {
                        ShowError("Неверный логин или пароль");
                        LoginTextBox.Focus();
                        LoginTextBox.SelectAll();
                        return;
                    }

                    CurrentUser = user; // Сохраняем текущего пользователя

                    this.Hide(); // Скрываем окно авторизации

                    var mainWindow = new MainWindow(user); // Создаем и показываем главное окно
                    mainWindow.Show();

                    this.Close(); // Закрываем окно авторизации
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка авторизации: {ex.Message}");
            }
        }

        // Отображение сообщения об ошибке
        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorBorder.Visibility = Visibility.Visible;
        }

        // Очистка сообщений об ошибках
        private void ClearError()
        {
            ErrorTextBlock.Text = "";
            ErrorBorder.Visibility = Visibility.Collapsed;
        }
    }
}