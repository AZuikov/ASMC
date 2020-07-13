﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ASMC.Data.Model;

namespace ASMC.Core
{
    public class EnumFlagToBoolConverter : IValueConverter
    {
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || value == null) return DependencyProperty.UnsetValue;
            if (Enum.IsDefined(value.GetType(), value)) return DependencyProperty.UnsetValue;
            return  ((Enum) value).HasFlag((Enum) Enum.Parse(value.GetType(), (string) parameter));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //if (parameter == null) return DependencyProperty.UnsetValue;
            //typeOpeation ^= (AbstraktOperation.TypeOpeation)parameter;
            //return typeOpeation;
            return parameter == null ? DependencyProperty.UnsetValue : Enum.Parse(targetType, (string)parameter);
        }
    }
}
