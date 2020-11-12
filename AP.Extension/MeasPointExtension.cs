using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

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
        public static IEnumerable<IMeasPoint<T>> GetArayMeasPointsInParcent<T>(this IMeasPoint<T> range, params int[] pointParcent) where T : class, IPhysicalQuantity<T>, new()
        {
            var listPoint = new List<MeasPoint<T>>();
            foreach (var countPoint in pointParcent)
            {
                if (countPoint>100 || countPoint<0)
                {
                    throw  new ArgumentOutOfRangeException(nameof(pointParcent),countPoint.ToString());
                }

                var mp = new MeasPoint<T>();
                mp.MainPhysicalQuantity.Multiplier = range.MainPhysicalQuantity.Multiplier;
                mp.MainPhysicalQuantity.Unit = range.MainPhysicalQuantity.Unit;
                mp.MainPhysicalQuantity.Value =range.MainPhysicalQuantity.Value * (countPoint / 100M);
                yield return  mp;
            }
        }
    }
}
