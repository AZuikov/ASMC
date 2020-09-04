using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AP.Themes.Converters
{
    public class Thickness3DConverter : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Thickness thickness))
                return Binding.DoNothing;

            switch (parameter)
            {
                case "0":
                    return new Thickness(thickness.Left, thickness.Top, 0, 0);
                case "1":
                    return new Thickness(0, 0, thickness.Right, thickness.Bottom);
                default:
                    return thickness;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
