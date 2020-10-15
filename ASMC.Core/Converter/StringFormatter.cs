using System;
using System.Globalization;
using System.Windows.Data;

namespace ASMC.Core.Converter
{
    public class StringFormatter: IMultiValueConverter
    {
        /// <inheritdoc />
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[1]==null) return values[0];
            return string.Format((string) values[1], values[0]);
        }

        /// <inheritdoc />
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
