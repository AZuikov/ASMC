using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace AP.Extension
{
    public static class ObjectExtension
    {
        public static decimal ConvertToDecimal(this object number)
        {
            return decimal.Parse(number.ToString().Trim()
                .Replace(".",
                    Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator));
        }
        public static MeasPoint<T> ConvertToMeasPint<T>(this object number, UnitMultiplier multiplier = UnitMultiplier.None) where T : class, IPhysicalQuantity<T>, new()
        {
            var val= decimal.Parse(number.ToString().Trim()
                .Replace(".",
                    Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator));
            return new MeasPoint<T>(val, multiplier);
        }

    }
}
