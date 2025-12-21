using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ShoeStoreDesktop.Converters
{
    public class IntToVisibilityConverter : IValueConverter
    {
        // Показывается элемент если число больше 0, скрывает если 0
        // Например: показывает кнопку "Заказать" только если товар есть в наличии
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}