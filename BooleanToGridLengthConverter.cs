using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfApp14
{
    public class BooleanToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isVisible && parameter is string width)
            {
                if (isVisible && double.TryParse(width, out double starWidth))
                    return new GridLength(starWidth, GridUnitType.Star);
                return new GridLength(0);
            }
            return new GridLength(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}