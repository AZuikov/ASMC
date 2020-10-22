using System;
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
        protected virtual RangeStorage<Length> GetRanges()
        {
            return new RangeStorage<Length>(new PhysicalRange<Length>(ne))
        }
        /// <summary>
        /// Здает измерительный диапазон.
        /// </summary>
        public RangeStorage<Length> Ranges
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
    }
}