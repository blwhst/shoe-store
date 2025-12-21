using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ShoeStoreDesktop.Converters
{
    public class StringToImageConverter : IValueConverter
    {
        // Загружается изображение по имени файла из папки Images
        // Если файл не найден, показывает заглушку (picture.png или иконку приложения)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string imagesFolder = "Images";

                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string fullImagesPath = Path.Combine(baseDirectory, imagesFolder);

                // Создание папки Images если ее нет
                if (!Directory.Exists(fullImagesPath))
                {
                    try
                    {
                        Directory.CreateDirectory(fullImagesPath);
                    }
                    catch
                    {
                        return GetDefaultImage();
                    }
                }

                string imageFileName = value as string;

                // Если имя файла пустое - показывает заглушку
                if (string.IsNullOrWhiteSpace(imageFileName))
                {
                    return GetDefaultImage();
                }

                imageFileName = imageFileName.Trim();

                // Пытаемся найти файл по полному пути
                string fullPath = Path.Combine(fullImagesPath, imageFileName);

                if (File.Exists(fullPath))
                {
                    try
                    {
                        // Загружаем изображение из файла
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.UriSource = new Uri(fullPath);
                        bitmap.EndInit();
                        bitmap.Freeze();
                        return bitmap;
                    }
                    catch
                    {
                        return GetDefaultImage();
                    }
                }

                // Ищем файл по имени без расширения
                var files = Directory.GetFiles(fullImagesPath, "*.*");
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imageFileName);

                if (!string.IsNullOrEmpty(fileNameWithoutExtension))
                {
                    foreach (var file in files)
                    {
                        string currentFileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);

                        // Сравниваем имена файлов без расширения
                        if (currentFileNameWithoutExtension.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                var bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.UriSource = new Uri(file);
                                bitmap.EndInit();
                                bitmap.Freeze();
                                return bitmap;
                            }
                            catch
                            {
                                break;
                            }
                        }
                    }
                }

                // Если ничего не найдено - показывает заглушку
                return GetDefaultImage();
            }
            catch
            {
                return GetDefaultImage();
            }
        }
        // Возвращает изображение-заглушку
        private BitmapImage GetDefaultImage()
        {
            try
            {
                string imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                string defaultImagePath = Path.Combine(imagesFolder, "picture.png");

                // Пытаемся загрузить picture.png
                if (File.Exists(defaultImagePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(defaultImagePath);
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }

                // Если picture.png нет - используется иконка приложения
                return new BitmapImage(new Uri("pack://application:,,,/Icon.ico"));
            }
            catch
            {
                // В случае ошибки тоже использует иконку приложения
                return new BitmapImage(new Uri("pack://application:,,,/Icon.ico"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}