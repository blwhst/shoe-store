using Microsoft.EntityFrameworkCore;
using ShoeStoreData.Contexts;
using ShoeStoreData.Models;
using ShoeStoreDesktop.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ShoeStoreDesktop
{
    // Главное окно приложения - каталог товаров
    // Отображает товары с фильтрацией и сортировкой
    // Управляет навигацией и основными функциями системы
    public partial class MainWindow : Window
    {
        private readonly User _currentUser; // Текущий пользователь
        private List<Product> _allProducts; // Все товары из базы данных
        private List<Manufacturer> _manufacturers; // Список производителей
        private DispatcherTimer _searchTimer;

        public MainWindow(User user)
        {
            InitializeComponent();

            this.WindowState = WindowState.Maximized;

            _currentUser = user; // Сохраняем текущего пользователя

            Title = $"Магазин обуви";
            UserInfoTextBlock.Text = user.FullName; // Отображаем имя пользователя

            _searchTimer = new DispatcherTimer();
            _searchTimer.Interval = TimeSpan.FromMilliseconds(500);
            _searchTimer.Tick += SearchTimer_Tick;

            // Показываем панель администратора для менеджеров и администраторов
            if (user.Role?.RoleName == "Менеджер" || user.Role?.RoleName == "Администратор")
            {
                AdminControlsPanel.Visibility = Visibility.Visible;
            }

            InitializeFilters();
            LoadData(); // Загружаем данные
            LoadProducts(); // Загружаем товары
        }

        private void InitializeFilters()
        {
            SortComboBox.SelectedIndex = 0; // Устанавливаем сортировку по умолчанию
            OnlyDiscountCheckBox.Checked += FilterCheckBox_Changed;
            OnlyDiscountCheckBox.Unchecked += FilterCheckBox_Changed;
            OnlyInStockCheckBox.Checked += FilterCheckBox_Changed;
            OnlyInStockCheckBox.Unchecked += FilterCheckBox_Changed;
        }

        #region Обработчики событий

        // Обработчик изменения состояния чекбоксов фильтров
        private void FilterCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            LoadProducts(); // Перезагружаем товары
        }

        // Обработчик изменения выбора в комбобоксах
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == ManufacturerComboBox || sender == SortComboBox)
            {
                LoadProducts(); // Перезагружаем товары
            }
        }

        // Обработчик изменения текста в полях поиска
        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchTimer.Stop(); // Останавливаем предыдущий таймер
            _searchTimer.Start(); // Запускаем новый таймер
        }

        // Обработчик срабатывания таймера поиска
        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            _searchTimer.Stop(); // Останавливаем таймер
            LoadProducts(); // Выполняем поиск
        }

        // Обработчик кнопки выхода из системы
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var loginWindow = new LoginWindow(); // Создаем новое окно авторизации
                loginWindow.Show();
                this.Close(); // Закрываем текущее окно
            }
        }

        // Обработчик кнопки сброса фильтров
        private void ResetFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            ManufacturerComboBox.SelectedIndex = 0;
            MaxPriceTextBox.Text = "";
            OnlyDiscountCheckBox.IsChecked = false;
            OnlyInStockCheckBox.IsChecked = false;
            SortComboBox.SelectedIndex = 0;
            LoadProducts(); // Перезагружаем товары
        }

        // Обработчик кнопки добавления товара
        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditProductWindow()
            {
                Owner = this
            };

            var result = addWindow.ShowDialog();

            if (result == true && addWindow.ProductSaved)
            {
                LoadData(); // Обновляем данные
                LoadProducts(); // Перезагружаем товары

                MessageBox.Show("Товар успешно добавлен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Обработчик кнопки редактирования товара
        private void EditProductButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно выбора товара для редактирования
            var selectWindow = new SelectProductsWindow("edit")
            {
                Owner = this
            };

            var result = selectWindow.ShowDialog();

            if (result != true || selectWindow.SelectedProducts == null || selectWindow.SelectedProducts.Count != 1)
            {
                return;
            }

            var productToEdit = selectWindow.SelectedProducts.First();

            // Открываем окно редактирования товара
            var editWindow = new AddEditProductWindow(productToEdit)
            {
                Owner = this
            };

            var editResult = editWindow.ShowDialog();

            if (editResult == true && editWindow.ProductSaved)
            {
                LoadData(); // Обновляем данные
                LoadProducts(); // Перезагружаем товары

                MessageBox.Show("Товар успешно обновлен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Обработчик кнопки удаления товаров
        private void DeleteProductButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно выбора товаров для удаления
            var selectWindow = new SelectProductsWindow()
            {
                Owner = this
            };

            var result = selectWindow.ShowDialog();

            if (result != true || selectWindow.SelectedProducts == null || selectWindow.SelectedProducts.Count == 0)
            {
                return;
            }

            // Открываем окно подтверждения удаления
            var deleteWindow = new DeleteProductsWindow(selectWindow.SelectedProducts, _allProducts)
            {
                Owner = this
            };

            var deleteResult = deleteWindow.ShowDialog();

            if (deleteResult == true && deleteWindow.ProductsDeleted)
            {
                LoadData(); // Обновляем данные
                LoadProducts(); // Перезагружаем товары

                MessageBox.Show($"Удалено {selectWindow.SelectedProducts.Count} товаров!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Валидация ввода чисел
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0) && e.Text != ".")
            {
                e.Handled = true; // Блокируем ввод нечисловых символов
            }
        }

        // Обработчик кнопки заказа товара
        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем роль пользователя
            if (_currentUser.Role?.RoleName == "Менеджер" || _currentUser.Role?.RoleName == "Администратор")
            {
                MessageBox.Show("Функция заказа доступна только для клиентов",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Получаем артикул товара из свойства Tag кнопки
            if (sender is Button button && button.Tag is string article)
            {
                var product = _allProducts?.FirstOrDefault(p => p.Article == article);
                if (product != null)
                {
                    if (product.Quantity > 0)
                    {
                        MessageBox.Show($"Товар '{product.Name}' добавлен в заказ!",
                            "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Товар отсутствует на складе",
                            "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        // Обработчик кнопки просмотра заказов
        private void ViewOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            var ordersWindow = new OrdersWindow(_currentUser)
            {
                Owner = this
            };
            ordersWindow.ShowDialog(); // Открываем окно заказов
        }

        #endregion

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
                        .ToList();

                    // Сбрасываем флаги выбора для всех товаров
                    foreach (var product in _allProducts)
                    {
                        product.IsSelected = false;
                    }

                    // Загружаем производителей для фильтра
                    _manufacturers = context.Manufacturers
                        .OrderBy(m => m.Name)
                        .ToList();

                    // Создаем список для комбобокса фильтрации
                    var manufacturersForComboBox = new List<Manufacturer>();
                    manufacturersForComboBox.Add(new Manufacturer
                    {
                        ManufacturerId = 0,
                        Name = "Все производители"
                    });
                    manufacturersForComboBox.AddRange(_manufacturers);

                    // Настраиваем комбобокс фильтрации
                    ManufacturerComboBox.ItemsSource = manufacturersForComboBox;
                    ManufacturerComboBox.DisplayMemberPath = "Name";
                    ManufacturerComboBox.SelectedValuePath = "ManufacturerId";
                    ManufacturerComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Загрузка и фильтрация товаров
        private void LoadProducts()
        {
            try
            {
                if (_allProducts == null) return;

                var filteredProducts = _allProducts.AsEnumerable();

                // Фильтрация по поисковому запросу
                if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
                {
                    var search = SearchTextBox.Text.ToLower();
                    filteredProducts = filteredProducts.Where(p =>
                        (p.Description != null && p.Description.ToLower().Contains(search)) ||
                        p.Name.ToLower().Contains(search));
                }

                // Фильтрация по производителю
                if (ManufacturerComboBox.SelectedItem is Manufacturer selectedManufacturer)
                {
                    if (selectedManufacturer.ManufacturerId > 0)
                    {
                        filteredProducts = filteredProducts.Where(p =>
                            p.ManufacturerId == selectedManufacturer.ManufacturerId);
                    }
                }

                // Фильтрация по максимальной цене
                if (decimal.TryParse(MaxPriceTextBox.Text, out decimal maxPrice) && maxPrice > 0)
                {
                    filteredProducts = filteredProducts.Where(p => p.DiscountPrice <= maxPrice);
                }

                // Фильтрация товаров со скидкой
                if (OnlyDiscountCheckBox.IsChecked == true)
                {
                    filteredProducts = filteredProducts.Where(p => p.Discount > 0);
                }

                // Фильтрация товаров в наличии
                if (OnlyInStockCheckBox.IsChecked == true)
                {
                    filteredProducts = filteredProducts.Where(p => p.Quantity > 0);
                }

                // Сортировка товаров
                filteredProducts = SortComboBox.SelectedIndex switch
                {
                    0 => filteredProducts.OrderBy(p => p.Name), // По названию
                    1 => filteredProducts.OrderBy(p => p.Supplier.Name), // По поставщику
                    2 => filteredProducts.OrderBy(p => p.DiscountPrice), // По цене (возр.)
                    3 => filteredProducts.OrderByDescending(p => p.DiscountPrice), // По цене (убыв.)
                    _ => filteredProducts.OrderBy(p => p.Name)
                };

                var productsList = filteredProducts.ToList();

                // Отображаем отфильтрованные товары
                ProductsItemsControl.ItemsSource = productsList;
                ProductsCountTextBlock.Text = $"Товаров: {productsList.Count}";
                StatusTextBlock.Text = "Готово";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Ошибка: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}