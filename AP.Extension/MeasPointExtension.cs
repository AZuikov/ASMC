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
        /// <param name="rangeEndValue">Максимальноен значение диапазона.</param>
        /// <param name="pointParcent">одно или несколько знаечние в процентах. Может принимать значение от 0 до 100</param>
        /// <returns></returns>
        public static IEnumerable<IMeasPoint<T>> GetArayMeasPointsInParcent<T>(this IMeasPoint<T> rangeEndValue, params int[] pointParcent) where T : class, IPhysicalQuantity<T>, new()
        {
            var listPoint = new List<MeasPoint<T>>();
            foreach (var countPoint in pointParcent)
            {
                if (countPoint>100 || countPoint<0)
                {
                    throw  new ArgumentOutOfRangeException(nameof(pointParcent),countPoint.ToString());
                }

                var mp = new MeasPoint<T>();
                mp.MainPhysicalQuantity.Multiplier = rangeEndValue.MainPhysicalQuantity.Multiplier;
                mp.MainPhysicalQuantity.Unit = rangeEndValue.MainPhysicalQuantity.Unit;
                mp.MainPhysicalQuantity.Value =rangeEndValue.MainPhysicalQuantity.Value * (countPoint / 100M);
                yield return  mp;
            }
        }

        public static IEnumerable<IMeasPoint<T>> GetArayMeasPointsInParcent<T>(this IMeasPoint<T> rangeEndValue ,  IMeasPoint<T> rangeStartValue, params int[] pointParcent) where T : class, IPhysicalQuantity<T>, new()
        {
            if ((MeasPoint<T>)rangeStartValue >= (MeasPoint<T>)rangeEndValue) throw new ArgumentOutOfRangeException("Начало диапазона больше конца диапазона.");

            foreach (var countPoint in pointParcent)
            {
                if (countPoint > 100 || countPoint < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(pointParcent), countPoint.ToString());
                }

                
                
                yield return ((MeasPoint<T>) rangeEndValue + (MeasPoint<T>) rangeStartValue.Abs()) * (countPoint / 100M) - (MeasPoint<T>) rangeStartValue.Abs();
                   
            }
        }
        /// <summary>
        /// Отбрасывает знак у значения точки (берет модуль).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="measPoint"></param>
        /// <returns></returns>
        public static IMeasPoint<T> Abs<T>(this IMeasPoint<T> measPoint) where T : class, IPhysicalQuantity<T>, new()
        {
            measPoint.MainPhysicalQuantity.Value = Math.Abs(measPoint.MainPhysicalQuantity.Value);
            return measPoint;
        }
        /// <summary>
        /// Округляет значение до нужного числа знаков.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="measPoint">Измерительная точка.</param>
        /// <param name="digitsCount">Число знаков после запятой.</param>
        /// <returns></returns>
        public static IMeasPoint<T> Round<T>(this IMeasPoint<T> measPoint,int digitsCount) where T : class, IPhysicalQuantity<T>, new()
        {
            measPoint.MainPhysicalQuantity.Value = Math.Round(measPoint.MainPhysicalQuantity.Value, digitsCount);
            return measPoint;
        }
    }
}
