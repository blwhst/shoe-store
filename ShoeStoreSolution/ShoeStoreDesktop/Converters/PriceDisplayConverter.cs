using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ShoeStoreData.Models;

namespace ShoeStoreDesktop.Converters
{
    public class PriceDisplayConverter : IValueConverter
    {
        // Отображается цена: при скидке показывает старую (красная зачеркнутая) и новую (черная жирная) цену
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Product product)
            {
                if (product.Discount > 0)
                {
                    var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };

                    var oldPrice = new TextBlock
                    {
                        Text = product.Price.ToString("N2") + " ₽",
                        Foreground = System.Windows.Media.Brushes.Red,
                        TextDecorations = TextDecorations.Strikethrough
                    };

                    var arrow = new TextBlock
                    {
                        Text = " → ",
                        Margin = new Thickness(5, 0, 5, 0)
                    };

                    var newPrice = new TextBlock
                    {
                        Text = product.DiscountPrice.ToString("N2") + " ₽",
                        Foreground = System.Windows.Media.Brushes.Black,
                        FontWeight = FontWeights.Bold
                    };

                    stackPanel.Children.Add(oldPrice);
                    stackPanel.Children.Add(arrow);
                    stackPanel.Children.Add(newPrice);

                    return stackPanel;
                }
                else
                {
                    return new TextBlock
                    {
                        Text = product.Price.ToString("N2") + " ₽",
                        Foreground = System.Windows.Media.Brushes.Black
                    };
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}