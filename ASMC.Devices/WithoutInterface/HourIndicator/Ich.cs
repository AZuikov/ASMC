using ASMC.Data.Model;

namespace ASMC.Devices.WithoutInterface.HourIndicator
{
    public class Ich : IUserType
    {
        #region Property

        public MaxMeasuringForce MeasuringForce { get; set; }

        /// <summary>
        /// Максимальное допустимое отклонение стрелки при перпендикулярном нажиме на его ось.
        /// </summary>
        public double PerpendicularPressureMax { get; set; }

        public MeasPoint Range { get; set; }

        #endregion

        public Ich(string userType)
        {
            UserType = userType;
        }

        /// <inheritdoc />
        public string UserType { get; }

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