using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ASMC.Core.View.Converters
{
    public class DataRowToDataRowViewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DataRow row)
            {
                return row.Table.DefaultView.Cast<DataRowView>().FirstOrDefault(v => v.Row == row);
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is DataRowView rowView)
            {
                return rowView.Row;
            }

            return null;
        }
    }
}
