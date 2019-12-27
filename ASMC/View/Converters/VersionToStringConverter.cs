using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ASMC.View.Converters
{
    public class VersionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(value is Version ver))
                return Binding.DoNothing;

            var param = parameter?.ToString();
            if(param != null && int.TryParse(param, out var fieldCount))
                return ver.ToString(fieldCount);

            return ver.ToString();

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
