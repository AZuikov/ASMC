using System.ComponentModel;
using System.Reflection;
using ASMC.Data.Model;

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

        public MeasPoint Range { get; set; }

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
        public class MaxMeasuringForce
        {
            #region Property

            /// <summary>
            /// Измерение хода
            /// </summary>
            public MeasPoint ChangeCourse { get; set; }

            /// <summary>
            /// Колибание при прямом/обратном ходу
            /// </summary>
            public MeasPoint Oscillatons { get; set; }

            /// <summary>
            /// Максимальное усилие DataGridViewHeaderBorderStyle прямом ходе
            /// </summary>
            public MeasPoint StraightRun { get; set; }

            #endregion
        }

    }
}