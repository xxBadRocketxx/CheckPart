// Converters/IsMatchToBrushConverter.cs (TẠO FILE MỚI)
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CheckPart.Converters
{
    public class IsMatchToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isMatch && !isMatch)
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEEEEEE")); // Màu đỏ nhạt
            }
            return Brushes.Transparent; // Mặc định là trong suốt
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // Không cần ConvertBack trong trường hợp này
        }
    }
}