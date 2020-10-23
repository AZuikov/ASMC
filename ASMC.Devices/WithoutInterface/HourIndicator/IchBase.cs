using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.WithoutInterface.HourIndicator
{
    public class IchBase : IUserType
    {
        #region Property

        public MaxMeasuringForce MeasuringForce { get; set; }

        /// <summary>
        /// Максимальное допустимое отклонение стрелки при перпендикулярном нажиме на его ось.
        /// </summary>
        public double PerpendicularPressureMax { get; set; }
        /// <summary>
        /// Позволяет задать измерительный диапазон.
        /// </summary>
        /// <returns></returns>
        protected virtual RangeStorage<PhysicalRange<Length>> GetRanges()
        {
            return null;
        }
        /// <summary>
        /// Здает измерительный диапазон.
        /// </summary>
        public RangeStorage<PhysicalRange<Length>> Ranges
        {
            get => GetRanges();
        }
        public MeasPoint<Length> Range { get; set; }

        #endregion


        public AccuracyClass.Standart CurrentAccuracyClass { get; set; }

        /// <summary>
        /// Доступные класы точности.
        /// </summary>
        public AccuracyClass.Standart[] AvailabeAccuracyClass { get; } =
            {AccuracyClass.Standart.Zero, AccuracyClass.Standart.First, AccuracyClass.Standart.Second};
        /// <inheritdoc />
        public string UserType { get; set; }

        /// <summary>
        /// Максимальное допутсимое усилие
        /// </summary>
        public sealed class MaxMeasuringForce
        {
            #region Property

            /// <summary>
            /// Измерение хода
            /// </summary>
            public MeasPoint<Force> ChangeCourse { get; set; }

            /// <summary>
            /// Колибание при прямом/обратном ходу
            /// </summary>
            public MeasPoint<Force> Oscillatons { get; set; }

            /// <summary>
            /// Максимальное усилие DataGridViewHeaderBorderStyle прямом ходе
            /// </summary>
            public MeasPoint<Force> StraightRun { get; set; }

            #endregion
        }

    }

    public class IchGost577 : IchBase
    {
        /// <inheritdoc />
        protected override RangeStorage<PhysicalRange<Length>> GetRanges()
        {
            return base.GetRanges();
        }
    }
    public class Ich_10 : IchGost577
    {
        /// <inheritdoc />
        protected override RangeStorage<PhysicalRange<Length>> GetRanges()
        {
            var arr = new List<PhysicalRange<Length>>();
            for (int i = 1; i <= 10; i++)
            {
                arr.Add(GeneratoRanges(i - 1, i));
            }
            

            PhysicalRange <Length> GeneratoRanges (decimal start, decimal end)
            {
                var st = new MeasPoint<Length>(new Length(start, UnitMultiplier.Mili));
                var ed = new MeasPoint<Length>(new Length(end, UnitMultiplier.Mili));
                return new PhysicalRange<Length>(st, ed, new AccuracyChatacteristic((decimal?) 0.08,null,null));
            }

            var ac = new AccuracyChatacteristic((decimal?) 0.15,null, null);
            return new RangeStorage<PhysicalRange<Length>>(ac,arr.ToArray());
        }
    }
}