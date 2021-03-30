using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AP.Math;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Data.Model
{
    /// <summary>
    /// Предоставляет реализацию хранилища диапазонов (по виду измерения). Фактически перечень пределов СИ.
    /// </summary>
    public class RangeStorage<T> : IEnumerable where T : IPhysicalRange
    {
        #region Property

        
        /// <summary>
        /// Позволяет получить характеристику точности
        /// </summary>
        public AccuracyChatacteristic AccuracyChatacteristic { get; set; }

        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name { get; set; }

        public T[] Ranges { get; set; }

        #endregion

        public RangeStorage(AccuracyChatacteristic accuracy, params T[] inPhysicalRanges)
        {
            AccuracyChatacteristic = accuracy;
            Ranges = inPhysicalRanges;
        }

        public RangeStorage(params T[] inPhysicalRanges)
        {
            Ranges = inPhysicalRanges;
        }

        #region Methods

        

        /// <summary>
        /// Возвращает диапазон к которому относится точка.
        /// </summary>
        /// <typeparam name = "T1">Основная физическ4ая величина.</typeparam>
        /// <param name = "inPoint">Точка (значение) физической велечины.</param>
        /// <returns></returns>
        public T GetRangePointBelong<T1>(IMeasPoint<T1> inPoint) where T1 : class, IPhysicalQuantity<T1>, new()
        {
            var range = Ranges as IPhysicalRange<T1>[];
            var result = (T)range?.FirstOrDefault(q => q.Start as MeasPoint<T1> <= (inPoint as MeasPoint<T1>) &&
                                                       q.End as MeasPoint<T1> >= (inPoint as MeasPoint<T1>));
            return result;
        }
        /// <summary>
        /// Возвращает диапазон к которому относится точка.
        /// </summary>
        /// <typeparam name="T1">Основная физическая величина.</typeparam>
        /// <typeparam name="T2">Дополнительная физическая величина.</typeparam>
        /// <param name="inPoint"></param>
        /// <returns></returns>
        public T GetRangePointBelong<T1, T2>(IMeasPoint<T1, T2> inPoint) where T1 : class, IPhysicalQuantity<T1>, new()
                                                                         where T2 : class, IPhysicalQuantity<T2>, new()
        {
            var range = Ranges as IPhysicalRange<T1, T2>[];
            T returnRange = (T)range?.FirstOrDefault(q => q.Start.MainPhysicalQuantity.GetNoramalizeValueToSi() <=
                                                          inPoint.MainPhysicalQuantity.GetNoramalizeValueToSi() &&
                                                          q.End.MainPhysicalQuantity.GetNoramalizeValueToSi() >=
                                                          inPoint.MainPhysicalQuantity.GetNoramalizeValueToSi()
                                                          && q.Start.AdditionalPhysicalQuantity.GetNoramalizeValueToSi() <=
                                                          inPoint.AdditionalPhysicalQuantity.GetNoramalizeValueToSi()
                                                          && q.End.AdditionalPhysicalQuantity.GetNoramalizeValueToSi() >=
                                                          inPoint.AdditionalPhysicalQuantity.GetNoramalizeValueToSi());

            return returnRange;
        }

        /// <summary>
        /// Возвращает величину погрешности физической величины, если она входит хотя бы в один диапазон.
        /// </summary>
        /// <typeparam name = "T1"></typeparam>
        /// <param name = "inPoint"></param>
        /// <returns></returns>
        public MeasPoint<T1> GetTolMeasPoint<T1>(IMeasPoint<T1> inPoint) where T1 : class, IPhysicalQuantity<T1>, new()
        {
            var range = GetRangePointBelong(inPoint);
            var returnPoint = new MeasPoint<T1>(range
                                               .AccuracyChatacteristic
                                               .GetAccuracy(inPoint.MainPhysicalQuantity.GetNoramalizeValueToSi()));
            returnPoint.MainPhysicalQuantity.ChangeMultiplier(inPoint.MainPhysicalQuantity.Multiplier);
            return returnPoint;
        }
        /// <summary>
        /// Возвращает величину погрешности физической величины, если она входит хотя бы в один диапазон.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="inPoint"></param>
        /// <returns></returns>
        public MeasPoint<T1,T2> GetTolMeasPoint<T1,T2>(IMeasPoint<T1,T2> inPoint) where T1 : class, IPhysicalQuantity<T1>, new() 
                                                                                  where T2 : class, IPhysicalQuantity<T2>, new()
        {
            var returnPoint = new MeasPoint<T1,T2>(GetRangePointBelong(inPoint).AccuracyChatacteristic.GetAccuracy(inPoint.MainPhysicalQuantity.GetNoramalizeValueToSi()),
                                                   inPoint.AdditionalPhysicalQuantity);
            returnPoint.MainPhysicalQuantity.ChangeMultiplier(inPoint.MainPhysicalQuantity.Multiplier);
            return returnPoint;
        }

        /// <summary>
        /// Расчитывает приведенную погрешность.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inPoint">Точка, которую измеряем. Нужна для поиска нужно диапазона.</param>
        /// <returns>Измерительная точка со значением погрешности измерения на найденном пределе.</returns>
        public MeasPoint<T> GetReducerTolMeasPoint<T>(IMeasPoint<T> inPoint)
            where T : class, IPhysicalQuantity<T>, new()
        {
            var range = GetRangePointBelong(inPoint) as PhysicalRange<T>;
            MeasPoint<T> rangeLeght = (MeasPoint<T>)range.GetRangeLeght;
            decimal val1 = 0;
            decimal val2 = 0;
            if (range.AccuracyChatacteristic.RangePercentFloor != null)
            {
                val1 = (decimal)range.AccuracyChatacteristic.RangePercentFloor / 100;
            }
            if (rangeLeght != null)
            {
                val2 = val1 * rangeLeght.MainPhysicalQuantity.Value;
                int mantisa = MathStatistics.GetMantissa(range.AccuracyChatacteristic.Resolution);
                val2 = Math.Round(val2,mantisa, MidpointRounding.AwayFromZero);
            }
            MeasPoint<T> result = new MeasPoint<T>(val2, range.Start.MainPhysicalQuantity.Multiplier);
            return result;
        }


        /// <summary>
        /// Принадлежит ли точка диапазону.
        /// </summary>
        /// <typeparam name = "T1"></typeparam>
        /// <param name = "point"></param>
        /// <returns></returns>
        public bool IsPointBelong<T1>(IMeasPoint<T1> point) where T1 : class, IPhysicalQuantity<T1>, new()
        {
            return GetRangePointBelong(point) != null;
        }

        /// <summary>
        /// Принадлежит ли точка диапазону.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool IsPointBelong<T1, T2>(IMeasPoint<T1, T2> point) where T1 : class, IPhysicalQuantity<T1>, new()
                                                                    where T2 : class, IPhysicalQuantity<T2>, new()
        {
            return GetRangePointBelong(point) != null;
        }



        #endregion

        /// <inheritdoc />
        public IEnumerator GetEnumerator()
        {
            return Ranges.GetEnumerator();
        }
    }
}