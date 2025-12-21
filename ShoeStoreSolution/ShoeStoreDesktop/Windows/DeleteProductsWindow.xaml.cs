using Microsoft.EntityFrameworkCore;
using ShoeStoreData.Contexts;
using ShoeStoreData.Models;
using System.Windows;

namespace ShoeStoreDesktop.Windows
{
    // Окно подтверждения удаления выбранных товаров
    // Отображает список товаров для удаления и запрашивает подтверждение
    // Проверяет наличие товаров в заказах перед удалением
    public partial class DeleteProductsWindow : Window
    {
        private List<Product> _productsToDelete; // Список товаров, выбранных для удаления
        private List<Product> _originalProductList; // Исходный список товаров для обновления UI

        public bool ProductsDeleted { get; private set; } // Флаг успешного удаления

        public DeleteProductsWindow(List<Product> productsToDelete, List<Product> originalProductList)
        {
            InitializeComponent();
            _productsToDelete = productsToDelete ?? new List<Product>();
            _originalProductList = originalProductList ?? new List<Product>();

            LoadProducts();
            UpdateSelectedCount();
        }

        private void LoadProducts()
        {
            ProductsDataGrid.ItemsSource = _productsToDelete;
        }

        // Обновление текста с количеством выбранных товаров
        private void UpdateSelectedCount()
        {
            SelectedCountTextBlock.Text = $"Выбрано: {_productsToDelete.Count} товаров";
        }

        // Обработчик кнопки "Отмена" - закрывает окно без удаления
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ProductsDeleted = false;
            DialogResult = false;
            Close();
        }

        // Обработчик кнопки "Удалить" - выполняет удаление товаров
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_productsToDelete.Count == 0)
            {
                MessageBox.Show("Нет товаров для удаления.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Запрашиваем подтверждение удаления
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить {_productsToDelete.Count} товаров?\n\n" +
                "Эта операция необратима!",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                using (var context = new ShoeStoreDbContext())
                {
                    // Получаем список артикулов товаров для удаления
                    var articleList = _productsToDelete.Select(p => p.Article).ToList();

                    // Проверяем, есть ли товары в заказах
                    var productsWithOrders = context.OrderItems
                        .Where(oi => articleList.Contains(oi.Article))
                        .Select(oi => oi.Article)
                        .Distinct()
                        .ToList();

                    // Если товары есть в заказах - запрещаем удаление
                    if (productsWithOrders.Any())
                    {
                        var articlesString = string.Join(", ", productsWithOrders);
                        MessageBox.Show(
                            $"Нельзя удалить товары, которые есть в заказах:\n{articlesString}\n\n" +
                            "Сначала удалите связанные заказы.",
                            "Ошибка удаления",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    // Находим товары для удаления в базе данных
                    var productsToRemove = context.Products
                        .Where(p => articleList.Contains(p.Article))
                        .ToList();
                    
                    // Удаляем товары из базы данных
                    context.Products.RemoveRange(productsToRemove);
                    context.SaveChanges();

                    // Удаляем товары из исходного списка для обновления UI
                    _originalProductList.RemoveAll(p => articleList.Contains(p.Article));

                    ProductsDeleted = true; // Устанавливаем флаг успешного удаления
                    DialogResult = true; // Устанавливаем результат диалога

                    MessageBox.Show($"Успешно удалено {productsToRemove.Count} товаров.",
                        "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                    Close();
                }
            }
            catch (DbUpdateException dbEx)
            {
                // Обработка ошибок базы данных
                MessageBox.Show($"Ошибка базы данных при удалении: {dbEx.InnerException?.Message ?? dbEx.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                // Обработка общих ошибок
                MessageBox.Show($"Ошибка при удалении товаров: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}