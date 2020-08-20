using System;
using System.Globalization;
using System.Windows.Data;

namespace ASMC.View.Converters
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class SpecialBooToStringConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            return (bool)value ? "Ручной" : "Автоматический";
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
          
            if (value == null) return null;
            return value.ToString().Equals("Ручной");

        }
    }
}
