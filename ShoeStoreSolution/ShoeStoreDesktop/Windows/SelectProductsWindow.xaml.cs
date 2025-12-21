using Microsoft.EntityFrameworkCore;
using ShoeStoreData.Contexts;
using ShoeStoreData.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ShoeStoreDesktop.Windows
{
    // Окно выбора товаров для удаления или редактирования
    // Позволяет выбирать несколько товаров или один товар
    public partial class SelectProductsWindow : Window
    {
        private List<Product> _allProducts; // Все товары из базы данных
        private List<Product> _selectedProducts; // Выбранные товары
        private string _mode; // Режим работы: "edit" или "delete"

        public List<Product> SelectedProducts => _selectedProducts; // Свойство для получения выбранных товаров

        public SelectProductsWindow() : this("delete")
        {
        }

        public SelectProductsWindow(string mode)
        {
            InitializeComponent();
            _mode = mode;

            ConfigureForMode(); // Настраиваем интерфейс в зависимости от режима
            LoadData(); // Загружаем данные
        }

        // Настройка интерфейса в зависимости от режима
        private void ConfigureForMode()
        {
            if (_mode == "edit")
            {
                Title = "Выбор товара для редактирования";

                var titleBlock = FindName("TitleTextBlock") as TextBlock;
                var subtitleBlock = FindName("SubtitleTextBlock") as TextBlock;
                var continueBtn = FindName("ContinueButton") as Button;

                if (titleBlock != null)
                    titleBlock.Text = "Выберите товар для редактирования";

                if (subtitleBlock != null)
                    subtitleBlock.Text = "Выберите ОДИН товар для изменения";

                if (continueBtn != null)
                    continueBtn.Content = "Редактировать";

                // Добавляем подсказку для режима редактирования
                var hintText = new TextBlock
                {
                    Text = "Для редактирования выберите только один товар",
                    FontSize = 11,
                    FontStyle = FontStyles.Italic,
                    Foreground = Brushes.DimGray,
                    Margin = new Thickness(0, 5, 0, 0)
                };

                if (ProductsDataGrid != null)
                {
                    var headerStackPanel = ProductsDataGrid.Parent as Grid;
                    if (headerStackPanel != null && headerStackPanel.Children.Count > 0 &&
                        headerStackPanel.Children[0] is StackPanel buttonPanel)
                    {
                        buttonPanel.Children.Insert(2, hintText);
                    }
                }
            }
            else
            {
                Title = "Выбор товаров для удаления";

                var titleBlock = FindName("TitleTextBlock") as TextBlock;
                var subtitleBlock = FindName("SubtitleTextBlock") as TextBlock;
                var continueBtn = FindName("ContinueButton") as Button;

                if (titleBlock != null)
                    titleBlock.Text = "Выберите товары для удаления";

                if (subtitleBlock != null)
                    subtitleBlock.Text = "Отметьте товары, которые нужно удалить";

                if (continueBtn != null)
                    continueBtn.Content = "Продолжить";
            }
        }

        // Загрузка данных из базы данных
        private void LoadData()
        {
            try
            {
                using (var context = new ShoeStoreDbContext())
                {
                    // Загружаем все товары с связанными данными
                    _allProducts = context.Products
                        .Include(p => p.Category)
                        .Include(p => p.Manufacturer)
                        .Include(p => p.Supplier)
                        .OrderBy(p => p.Name)
                        .ToList();

                    // Сбрасываем флаги выбора
                    foreach (var product in _allProducts)
                    {
                        product.IsSelected = false;
                    }

                    ProductsDataGrid.ItemsSource = _allProducts; // Отображаем товары
                    UpdateSelectedCount(); // Обновляем счетчик выбранных товаров
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        // Обновление счетчика выбранных товаров
        private void UpdateSelectedCount()
        {
            if (_allProducts == null) return;

            int selectedCount = _allProducts.Count(p => p.IsSelected);

            var selectedCountBlock = FindName("SelectedCountTextBlock") as TextBlock;
            if (selectedCountBlock != null)
            {
                selectedCountBlock.Text = $"Выбрано: {selectedCount}";

                // В режиме редактирования выделяем красным, если выбрано больше одного товара
                if (_mode == "edit" && selectedCount > 1)
                {
                    selectedCountBlock.Text = $"Выбрано: {selectedCount} (выберите только 1 товар)";
                    selectedCountBlock.Foreground = Brushes.Red;
                }
                else
                {
                    selectedCountBlock.Foreground = Brushes.Black;
                }
            }

            // Активируем кнопку продолжения, если есть выбранные товары
            var continueBtn = FindName("ContinueButton") as Button;
            if (continueBtn != null)
                continueBtn.IsEnabled = selectedCount > 0;
        }

        // Обработчик кнопки "Выбрать все"
        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (_allProducts == null) return;

            // В режиме редактирования нельзя выбрать все товары
            if (_mode == "edit")
            {
                MessageBox.Show("В режиме редактирования нужно выбрать только один товар",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Выбираем все товары
            foreach (var product in _allProducts)
            {
                product.IsSelected = true;
            }

            ProductsDataGrid.Items.Refresh(); // Обновляем отображение
            UpdateSelectedCount(); // Обновляем счетчик
        }

        // Обработчик кнопки "Снять выбор"
        private void DeselectAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (_allProducts == null) return;

            // Снимаем выбор со всех товаров
            foreach (var product in _allProducts)
            {
                product.IsSelected = false;
            }

            ProductsDataGrid.Items.Refresh(); // Обновляем отображение
            UpdateSelectedCount(); // Обновляем счетчик
        }

        // Обработчик изменения состояния чекбокса
        private void CheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            UpdateSelectedCount(); // Обновляем счетчик
        }

        // Обработчик кнопки "Отмена"
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // Обработчик кнопки "Продолжить"
        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedProducts = _allProducts?.Where(p => p.IsSelected).ToList();

            // Проверяем, что есть выбранные товары
            if (_selectedProducts == null || _selectedProducts.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы один товар", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // В режиме редактирования проверяем, что выбран только один товар
            if (_mode == "edit")
            {
                if (_selectedProducts.Count > 1)
                {
                    MessageBox.Show("Для редактирования выберите только один товар", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            DialogResult = true; // Устанавливаем результат диалога
            Close(); // Закрываем окно
        }

        // Обработчик двойного клика по строке DataGrid
        private void ProductsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // В режиме редактирования двойной клик сразу выбирает товар
            if (_mode == "edit" && ProductsDataGrid.SelectedItem is Product selectedProduct)
            {
                // Снимаем выбор со всех товаров
                foreach (var product in _allProducts)
                {
                    product.IsSelected = false;
                }

                // Выбираем товар, по которому кликнули
                selectedProduct.IsSelected = true;
                ProductsDataGrid.Items.Refresh();
                UpdateSelectedCount();

                // Сохраняем выбранный товар и закрываем окно
                _selectedProducts = new List<Product> { selectedProduct };
                DialogResult = true;
                Close();
            }
        }
    }
}