using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ShoeStoreDesktop.Converters
{
    public class DiscountToForegroundConverter : IValueConverter
    {
        // Устанавливается цвет текста: для скидки >15% - белый, иначе черный
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int discount && discount > 15)
            {
                return Brushes.White;
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}