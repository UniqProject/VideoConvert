using System;
using System.Globalization;
using System.Windows.Data;

namespace Converters
{
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value,
                           Type targetType,
                           object parameter,
                           CultureInfo culture)
        {
            if (value != null)
            {
                return ((DateTime) value).ToString("dd.MM.yyyy HH:mm:ss", culture);
            }
            else
            {
                return String.Empty;
            }
        }

        public object ConvertBack(object value,
                                  Type targetType,
                                  object parameter,
                                  CultureInfo culture)
        {
            return DateTime.Parse(value.ToString());
        }
    }
}
