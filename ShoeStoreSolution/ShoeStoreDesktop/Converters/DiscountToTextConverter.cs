using System.Globalization;
using System.Windows.Data;

namespace ShoeStoreDesktop.Converters
{
    public class DiscountToTextConverter : IValueConverter
    {
        // Форматирование скидки: "-{discount}%" или "—" если скидки нет
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int discount)
            {
                if (discount > 0)
                {
                    return $"-{discount}%";
                }
                else
                {
                    return "—"; 
                }
            }
            return "—";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}