//============================================================================
// MKV Remux - Fast Video & Audio Conversion Tool
// Copyright © 2012 JT-Soft
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//=============================================================================

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Converters
{
    [ValueConversion(typeof(Boolean), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            // Paramater is a boolean which inverts the output.
            var param = System.Convert.ToBoolean(parameter, CultureInfo.InvariantCulture);

            if (value == null)
            {
                return Visibility.Collapsed;
            }

            if (value is Boolean)
            {
                if (param)
                {
                    return (bool)value ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    return (bool)value ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}