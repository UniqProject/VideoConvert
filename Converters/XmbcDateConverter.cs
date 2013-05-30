using System;
using System.Globalization;
using System.Windows.Data;

namespace Converters
{
    public class XmbcDateConverter : IValueConverter
    {
        public object Convert(object value,
                              Type targetType,
                              object parameter,
                              CultureInfo culture)
        {
            if (value != null)
            {
                return ((DateTime) value).ToString("yyyy-MM-dd", culture);
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