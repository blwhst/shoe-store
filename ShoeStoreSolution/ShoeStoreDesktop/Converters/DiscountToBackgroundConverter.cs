using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ShoeStoreDesktop.Converters
{
    public class DiscountToBackgroundConverter : IValueConverter
    {
        // Устанавливается цвет фона: для скидки >15% - #2E8B57, для скидки >0 - полупрозрачный зеленый
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int discount)
            {
                if (discount > 15)
                {
                    return new SolidColorBrush(Color.FromRgb(46, 139, 87));
                }
                else if (discount > 0)
                {
                    return new SolidColorBrush(Color.FromArgb(30, 127, 255, 0));
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}