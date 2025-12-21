using Microsoft.EntityFrameworkCore;
using ShoeStoreData.Contexts;
using ShoeStoreData.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace ShoeStoreDesktop.Windows
{
    // Окно добавления и редактирования товаров
    // Используется в задании 6 для управления товарами (добавление, редактирование)
    // Реализует загрузку изображений товаров и сохранение их в папку Images
    public partial class AddEditProductWindow : Window
    {
        private bool _isEditMode = false; // Режим работы: true - редактирование, false - добавление
        private Product _editingProduct = null; // Товар для редактирования
        private string _currentImagePath = null; // Полный путь к текущему изображению (хранится в памяти, не показывается пользователю)

        public bool ProductSaved { get; private set; }

        public AddEditProductWindow()
        {
            InitializeComponent();

            // Устанавливаем заголовки для режима добавления
            var titleBlock = FindName("TitleTextBlock") as System.Windows.Controls.TextBlock;
            var subtitleBlock = FindName("SubtitleTextBlock") as System.Windows.Controls.TextBlock;

            if (titleBlock != null)
                titleBlock.Text = "Добавление нового товара";

            if (subtitleBlock != null)
                subtitleBlock.Text = "Заполните информацию о товаре";

            LoadReferenceData();
            LoadProductImage(null);
        }

        public AddEditProductWindow(Product product) : this()
        {
            _isEditMode = true;
            _editingProduct = product;

            Title = "Редактирование товара";

            // Устанавливаем заголовки для режима редактирования
            var titleBlock = FindName("TitleTextBlock") as System.Windows.Controls.TextBlock;
            var subtitleBlock = FindName("SubtitleTextBlock") as System.Windows.Controls.TextBlock;

            if (titleBlock != null)
                titleBlock.Text = "Редактирование товара";

            if (subtitleBlock != null)
                subtitleBlock.Text = $"Редактирование: {product.Name}";

            LoadProductData();
        }

        // Загрузка справочных данных из базы данных
        private void LoadReferenceData()
        {
            try
            {
                using (var context = new ShoeStoreDbContext())
                {
                    // Загружаем категории
                    var categories = context.Categories
                        .OrderBy(c => c.Name)
                        .ToList();
                    CategoryComboBox.ItemsSource = categories;

                    // Загружаем производителей
                    var manufacturers = context.Manufacturers
                        .OrderBy(m => m.Name)
                        .ToList();
                    ManufacturerComboBox.ItemsSource = manufacturers;

                    // Загружаем поставщиков
                    var suppliers = context.Suppliers
                        .OrderBy(s => s.Name)
                        .ToList();
                    SupplierComboBox.ItemsSource = suppliers;

                    // Устанавливаем первые элементы по умолчанию
                    if (categories.Any()) CategoryComboBox.SelectedIndex = 0;
                    if (manufacturers.Any()) ManufacturerComboBox.SelectedIndex = 0;
                    if (suppliers.Any()) SupplierComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки справочников: {ex.Message}");
            }
        }

        // Загрузка данных товара для редактирования
        private void LoadProductData()
        {
            if (_editingProduct == null)
                return;

            // Заполняем поля данными товара
            ArticleTextBox.Text = _editingProduct.Article;
            NameTextBox.Text = _editingProduct.Name;
            DescriptionTextBox.Text = _editingProduct.Description;
            UnitTextBox.Text = _editingProduct.Unit;
            PriceTextBox.Text = _editingProduct.Price.ToString();
            DiscountTextBox.Text = _editingProduct.Discount.ToString();
            QuantityTextBox.Text = _editingProduct.Quantity.ToString();

            LoadProductImage(_editingProduct.Photo); // Загружаем изображение товара

            // Устанавливаем выбранные элементы в комбобоксах
            if (CategoryComboBox.ItemsSource != null)
            {
                foreach (var item in CategoryComboBox.Items)
                {
                    if (item is Category category && category.CategoryId == _editingProduct.CategoryId)
                    {
                        CategoryComboBox.SelectedItem = item;
                        break;
                    }
                }
            }

            if (ManufacturerComboBox.ItemsSource != null)
            {
                foreach (var item in ManufacturerComboBox.Items)
                {
                    if (item is Manufacturer manufacturer && manufacturer.ManufacturerId == _editingProduct.ManufacturerId)
                    {
                        ManufacturerComboBox.SelectedItem = item;
                        break;
                    }
                }
            }

            if (SupplierComboBox.ItemsSource != null)
            {
                foreach (var item in SupplierComboBox.Items)
                {
                    if (item is Supplier supplier && supplier.SupplierId == _editingProduct.SupplierId)
                    {
                        SupplierComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        // Загрузка изображения товара
        private void LoadProductImage(string imagePath)
        {
            try
            {
                var productImagePreview = FindName("ProductImagePreview") as System.Windows.Controls.Image;
                var photoPathTextBox = FindName("PhotoPathTextBox") as System.Windows.Controls.TextBox;

                if (productImagePreview == null || photoPathTextBox == null)
                    return;

                if (!string.IsNullOrWhiteSpace(imagePath))
                {
                    // Сохраняем полный путь в памяти
                    _currentImagePath = imagePath;

                    // Показываем только имя файла пользователю
                    string fileNameOnly = Path.GetFileName(imagePath);
                    photoPathTextBox.Text = fileNameOnly;

                    // Проверяем существование файла
                    if (File.Exists(imagePath))
                    {
                        // Загружаем изображение из файла
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(imagePath);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        productImagePreview.Source = bitmap;
                    }
                    else
                    {
                        // Если файл не найден - показываем иконку по умолчанию
                        productImagePreview.Source = new BitmapImage(new Uri("pack://application:,,,/Icon.ico"));
                        // _currentImagePath остается установленным, чтобы сохранить в БД
                    }
                }
                else
                {
                    // Если путь пустой - показываем иконку по умолчанию
                    _currentImagePath = null;
                    productImagePreview.Source = new BitmapImage(new Uri("pack://application:,,,/Icon.ico"));
                    photoPathTextBox.Text = "";
                }
            }
            catch
            {
                var productImagePreview = FindName("ProductImagePreview") as System.Windows.Controls.Image;
                if (productImagePreview != null)
                    productImagePreview.Source = new BitmapImage(new Uri("pack://application:,,,/Icon.ico"));
                // В случае ошибки _currentImagePath остается как был
            }
        }

        // Обработчик кнопки "Выбрать" для загрузки изображения
        private void BrowseImageButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All files (*.*)|*.*",
                Title = "Выберите изображение товара"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string sourceFilePath = openFileDialog.FileName;
                    string fileName = Path.GetFileName(sourceFilePath);

                    string imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

                    if (!Directory.Exists(imagesFolder))
                    {
                        Directory.CreateDirectory(imagesFolder);
                    }

                    string destinationFilePath = Path.Combine(imagesFolder, fileName);

                    int counter = 1;
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    string fileExtension = Path.GetExtension(fileName);

                    // Проверяем, существует ли файл с таким именем
                    while (File.Exists(destinationFilePath))
                    {
                        string newFileName = $"{fileNameWithoutExt}_{counter}{fileExtension}";
                        destinationFilePath = Path.Combine(imagesFolder, newFileName);
                        counter++;
                    }

                    // Копируем файл в папку Images
                    File.Copy(sourceFilePath, destinationFilePath, true);

                    // Сохраняем полный путь в памяти
                    _currentImagePath = destinationFilePath;

                    var photoPathTextBox = FindName("PhotoPathTextBox") as System.Windows.Controls.TextBox;
                    if (photoPathTextBox != null)
                    {
                        // Показываем только имя файла пользователю
                        photoPathTextBox.Text = Path.GetFileName(destinationFilePath);
                    }

                    // Загружаем изображение в preview
                    LoadProductImage(destinationFilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Обработчик очистки изображения товара
        private void ClearImageButton_Click(object sender, RoutedEventArgs e)
        {
            _currentImagePath = null;
            var photoPathTextBox = FindName("PhotoPathTextBox") as System.Windows.Controls.TextBox;
            if (photoPathTextBox != null)
            {
                photoPathTextBox.Text = "";
            }
            LoadProductImage(null);
        }

        // Обработчик кнопки "Отмена" - закрывает окно без сохранения
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ProductSaved = false;
            DialogResult = false;
            Close();
        }

        // Обработчик кнопки "Сохранить" - сохраняет товар в базу данных
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearError(); // Очищаем сообщения об ошибках

                if (!ValidateForm()) // Проверяем валидность формы
                    return;

                using (var context = new ShoeStoreDbContext())
                {
                    Product product;

                    // Определяем режим работы: редактирование или добавление
                    if (_isEditMode && _editingProduct != null)
                    {
                        // Режим редактирования: ищем существующий товар
                        product = context.Products
                            .FirstOrDefault(p => p.Article == _editingProduct.Article);

                        if (product == null)
                        {
                            ShowError("Товар не найден в базе данных");
                            return;
                        }
                    }
                    else
                    {
                        // Режим добавления: проверяем уникальность артикула
                        var existingProduct = context.Products
                            .FirstOrDefault(p => p.Article == ArticleTextBox.Text.Trim());

                        if (existingProduct != null)
                        {
                            ShowError($"Товар с артикулом '{ArticleTextBox.Text}' уже существует");
                            ArticleTextBox.Focus();
                            ArticleTextBox.SelectAll();
                            return;
                        }

                        product = new Product(); // Создаем новый товар
                    }

                    // Проверяем, меняется ли изображение при редактировании
                    if (_isEditMode && _editingProduct != null)
                    {
                        string oldImagePath = _editingProduct.Photo;
                        // Удаляем старое изображение, если оно было заменено на новое
                        DeleteOldImageIfNeeded(oldImagePath, _currentImagePath);
                    }

                    // Заполняем свойства товара данными из формы
                    product.Article = ArticleTextBox.Text.Trim();
                    product.Name = NameTextBox.Text.Trim();
                    product.Description = DescriptionTextBox.Text.Trim();
                    product.Unit = UnitTextBox.Text.Trim();
                    product.Price = decimal.Parse(PriceTextBox.Text);
                    product.Discount = int.Parse(DiscountTextBox.Text);
                    product.Quantity = int.Parse(QuantityTextBox.Text);

                    // Сохраняем полный путь к изображению в БД
                    product.Photo = _currentImagePath;

                    // Устанавливаем связи с категорией, производителем и поставщиком
                    if (CategoryComboBox.SelectedItem is Category selectedCategory)
                        product.CategoryId = selectedCategory.CategoryId;

                    if (ManufacturerComboBox.SelectedItem is Manufacturer selectedManufacturer)
                        product.ManufacturerId = selectedManufacturer.ManufacturerId;

                    if (SupplierComboBox.SelectedItem is Supplier selectedSupplier)
                        product.SupplierId = selectedSupplier.SupplierId;

                    // Сохраняем изменения в базе данных
                    if (!_isEditMode)
                    {
                        context.Products.Add(product); // Добавляем новый товар
                    }
                    else
                    {
                        context.Entry(product).State = EntityState.Modified; // Обновляем существующий
                    }

                    context.SaveChanges(); // Сохраняем изменения в БД

                    ProductSaved = true; // Устанавливаем флаг успешного сохранения
                    DialogResult = true; // Устанавливаем результат диалога

                    MessageBox.Show(_isEditMode ? "Товар успешно обновлен!" : "Товар успешно добавлен!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    Close();
                }
            }
            catch (DbUpdateException dbEx)
            {
                // Обработка ошибок базы данных
                ShowError($"Ошибка сохранения в базу данных: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                // Обработка общих ошибок
                ShowError($"Ошибка при сохранении товара: {ex.Message}");
            }
        }

        // Удаление старого изображения при замене на новое
        private void DeleteOldImageIfNeeded(string oldImagePath, string newImagePath)
        {
            try
            {
                // Удаляем старое изображение только если:
                // 1. Было старое изображение
                // 2. Новое изображение отличается от старого
                // 3. Новое изображение не пустое
                if (!string.IsNullOrWhiteSpace(oldImagePath) &&
                    !string.IsNullOrWhiteSpace(newImagePath) &&
                    oldImagePath != newImagePath)
                {
                    // Проверяем, существует ли файл перед удалением
                    if (File.Exists(oldImagePath))
                    {
                        File.Delete(oldImagePath);
                    }
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не прерываем выполнение основной операции
                Debug.WriteLine($"Ошибка при удалении старого изображения: {ex.Message}");
            }
        }

        // Валидация формы перед сохранением
        private bool ValidateForm()
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(ArticleTextBox.Text))
            {
                ShowError("Введите артикул товара");
                ArticleTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                ShowError("Введите наименование товара");
                NameTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(UnitTextBox.Text))
            {
                ShowError("Введите единицу измерения");
                UnitTextBox.Focus();
                return false;
            }

            // Проверка цены
            if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price <= 0)
            {
                ShowError("Введите корректную цену (положительное число)");
                PriceTextBox.Focus();
                PriceTextBox.SelectAll();
                return false;
            }

            // Проверка скидки (0-100%)
            if (!int.TryParse(DiscountTextBox.Text, out int discount) || discount < 0 || discount > 100)
            {
                ShowError("Скидка должна быть от 0 до 100%");
                DiscountTextBox.Focus();
                DiscountTextBox.SelectAll();
                return false;
            }

            // Проверка количества (не может быть отрицательным)
            if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity < 0)
            {
                ShowError("Количество не может быть отрицательным");
                QuantityTextBox.Focus();
                QuantityTextBox.SelectAll();
                return false;
            }

            // Проверка выбора справочных значений
            if (CategoryComboBox.SelectedItem == null)
            {
                ShowError("Выберите категорию");
                CategoryComboBox.Focus();
                return false;
            }

            if (ManufacturerComboBox.SelectedItem == null)
            {
                ShowError("Выберите производителя");
                ManufacturerComboBox.Focus();
                return false;
            }

            if (SupplierComboBox.SelectedItem == null)
            {
                ShowError("Выберите поставщика");
                SupplierComboBox.Focus();
                return false;
            }

            return true; // Все проверки пройдены
        }

        // Валидация ввода чисел в текстовые поля
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != '.')
                {
                    e.Handled = true; // Блокируем ввод нечисловых символов
                    return;
                }
            }
        }

        // Отображение сообщения об ошибке
        private void ShowError(string message)
        {
            var errorTextBlock = FindName("ErrorTextBlock") as System.Windows.Controls.TextBlock;
            var errorBorder = FindName("ErrorBorder") as System.Windows.Controls.Border;

            if (errorTextBlock != null)
                errorTextBlock.Text = message;

            if (errorBorder != null)
                errorBorder.Visibility = Visibility.Visible;
        }

        // Очистка сообщений об ошибках
        private void ClearError()
        {
            var errorTextBlock = FindName("ErrorTextBlock") as System.Windows.Controls.TextBlock;
            var errorBorder = FindName("ErrorBorder") as System.Windows.Controls.Border;

            if (errorTextBlock != null)
                errorTextBlock.Text = "";

            if (errorBorder != null)
                errorBorder.Visibility = Visibility.Collapsed;
        }
    }
}