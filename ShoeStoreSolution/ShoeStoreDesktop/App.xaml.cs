using System;
using System.Windows;

namespace ShoeStoreDesktop
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                // Установка русской локали для приложения
                System.Threading.Thread.CurrentThread.CurrentCulture =
                    new System.Globalization.CultureInfo("ru-RU");
                System.Threading.Thread.CurrentThread.CurrentUICulture =
                    new System.Globalization.CultureInfo("ru-RU");

                // Настройка формата чисел для отображения цен в рублях
                var culture = new System.Globalization.CultureInfo("ru-RU");
                culture.NumberFormat.CurrencySymbol = "₽";
                System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

                var loginWindow = new LoginWindow();
                loginWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска: {ex.Message}");
                Shutdown();
            }
        }
    }
}