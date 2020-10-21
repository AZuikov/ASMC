using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Utils.Helps;
using ASMC.Data.Model;

namespace AP.Extension
{
    public static class MeasPointExtension
    {
        /// <summary>
        /// Формирует последовательность состающую из точек указаных в процентах.
        /// </summary>
        /// <param name="range"></param>
        /// <param name="pointParcent">одно или несколько знаечние в процентах. Может принимать значение от 0 до 100</param>
        /// <returns></returns>
        public static IEnumerable<MeasPoint<T>> GetArayMeasPointsInParcent<T>(this MeasPoint<T> range, params int[] pointParcent) where T: IPhysicalQuantity, IEquatable<T>, new()
        {
            var listPoint = new List<MeasPoint<T>>();
            foreach (var countPoint in pointParcent)
            {
                if (countPoint>100 || countPoint<0)
                {
                    throw  new ArgumentOutOfRangeException(nameof(pointParcent),countPoint.ToString());
                }
                var mp = new MeasPoint<T> {AdditionalPhysicalQuantity = range.AdditionalPhysicalQuantity};
                mp.MainPhysicalQuantity.Multipliers = range.MainPhysicalQuantity.Multipliers;
                mp.MainPhysicalQuantity.Unit = range.MainPhysicalQuantity.Unit;
                mp.MainPhysicalQuantity.Value = decimal.Parse(range.MainPhysicalQuantity.Value.ToString()) * countPoint / 100;
                yield return mp;
            }
        }
    }
}
