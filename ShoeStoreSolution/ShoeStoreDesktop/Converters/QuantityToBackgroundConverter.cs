using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ShoeStoreDesktop.Converters
{
    public class QuantityToBackgroundConverter : IValueConverter
    {
        // Устанавливается цвет фона: зеленый для товаров в наличии, красный для отсутствующих
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int quantity)
            {
                if (quantity > 0)
                {
                    return new SolidColorBrush(Color.FromArgb(30, 0, 128, 0));
                }
                else
                {
                    return new SolidColorBrush(Color.FromArgb(30, 255, 0, 0));
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}