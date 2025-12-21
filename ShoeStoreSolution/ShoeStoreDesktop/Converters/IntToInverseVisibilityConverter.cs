using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ShoeStoreDesktop.Converters
{
    public class IntToInverseVisibilityConverter : IValueConverter
    {
        // Скрывает элемент если число больше 0, показывает если 0
        // Нужен чтобы скрывать сообщение "Товара нет" когда товар есть в наличии
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue > 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}