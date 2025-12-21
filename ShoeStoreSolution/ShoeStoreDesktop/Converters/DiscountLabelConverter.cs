using System.Globalization;
using System.Windows.Data;

namespace ShoeStoreDesktop.Converters
{
    public class DiscountLabelConverter : IValueConverter
    {
        // Если есть скидка - возвращается "Действующая скидка", иначе "Нет скидки"
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int discount && discount > 0)
            {
                return "Действующая скидка";
            }
            return "Нет скидки";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}