using Microsoft.EntityFrameworkCore;
using ShoeStoreData.Contexts;
using ShoeStoreData.Models;
using System.Windows;

namespace ShoeStoreDesktop.Windows
{
    // Окно просмотра заказов
    // Отображает список заказов с детальной информацией
    // Для клиентов показывает только их заказы, для менеджеров - все заказы
    public partial class OrdersWindow : Window
    {
        private readonly User _currentUser; // Текущий пользователь

        public User CurrentUser => _currentUser;

        public OrdersWindow(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            DataContext = this; // Устанавливаем контекст данных

            // Настройка интерфейса в зависимости от роли пользователя
            if (_currentUser.Role?.RoleName == "Клиент")
            {
                Title = "Мои заказы";
                TitleTextBlock.Text = "Мои заказы";
                SubtitleTextBlock.Text = "Список ваших заказов";
                CustomerColumn.Visibility = Visibility.Collapsed; // Скрываем колонку с клиентом
                UserRoleTextBlock.Text = "Пользователь";
            }
            else
            {
                Title = "Все заказы";
                TitleTextBlock.Text = "Все заказы";
                SubtitleTextBlock.Text = "Список всех заказов системы";
                CustomerColumn.Visibility = Visibility.Visible; // Показываем колонку с клиентом
                UserRoleTextBlock.Text = _currentUser.Role?.RoleName ?? "Менеджер";
            }

            LoadOrders(); // Загружаем заказы
        }

        // Загрузка заказов из базы данных
        private void LoadOrders()
        {
            try
            {
                using (var context = new ShoeStoreDbContext())
                {
                    IQueryable<Order> query = context.Orders
                        .Include(o => o.OrderItems) // Включаем элементы заказа
                            .ThenInclude(oi => oi.ArticleNavigation) // Включаем товары
                        .Include(o => o.Customer) // Включаем данные о клиенте
                        .OrderByDescending(o => o.OrderDate); // Сортируем по дате

                    // Для клиентов показываем только их заказы
                    if (_currentUser.Role?.RoleName == "Клиент")
                    {
                        var customer = context.Customers
                            .FirstOrDefault(c => c.FullName == _currentUser.FullName);

                        if (customer != null)
                        {
                            query = query.Where(o => o.CustomerId == customer.CustomerId);
                        }
                    }

                    var orders = query.ToList();

                    // Рассчитываем общую стоимость каждого заказа
                    foreach (var order in orders)
                    {
                        decimal totalPrice = 0;
                        foreach (var orderItem in order.OrderItems)
                        {
                            var product = orderItem.ArticleNavigation;
                            if (product != null)
                            {
                                // Учитываем скидку при расчете стоимости
                                decimal itemPrice = product.Price * (100 - product.Discount) / 100;
                                totalPrice += itemPrice * orderItem.Quantity;
                            }
                        }
                        order.TotalPrice = totalPrice; // Сохраняем общую стоимость
                    }

                    OrdersDataGrid.ItemsSource = orders; // Отображаем заказы
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчик кнопки закрытия окна
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}