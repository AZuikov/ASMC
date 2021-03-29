using System;
using AP.Math;


namespace ASMC.Data.Model
{
    /// <summary>
    /// Предоставляет реализацию характеристики точности.
    /// </summary>
    public sealed class AccuracyChatacteristic
    {
        #region Property

        /// <summary>
        /// Процент от измеряемой велечины
        /// </summary>
        public decimal? PercentOfMeasurePointTol { get; }

        /// <summary>
        /// Процент от предела
        /// </summary>
        public decimal? RangePercentFloor { get; }

        /// <summary>
        /// Позволяет получать или задавать разрешение, включает число едениц младшего разряда
        /// </summary>
        public decimal? Resolution { get; }

        #endregion

        public AccuracyChatacteristic(decimal? resolution, decimal? rangePercentFloor,
            decimal? percentOfMeasurePointTol)
        {
            Resolution = resolution;
            RangePercentFloor = rangePercentFloor;
            PercentOfMeasurePointTol = percentOfMeasurePointTol;
        }

        #region Methods

        /// <summary>
        /// Расчет погрешности.
        /// </summary>
        /// <param name = "val">Значение для которого расчитывается погрешность.</param>
        /// <param name = "upperRange">Значение предела измерения.</param>
        /// <returns></returns>
        public decimal GetAccuracy(decimal val, decimal upperRange =0)
        {
            val = val > 0 ? val : -val;

            decimal TolOfMeasPoint = 0;
            decimal rangeFloor = 0;
            if (PercentOfMeasurePointTol != null) TolOfMeasPoint = (decimal) (PercentOfMeasurePointTol / 100);

            if (RangePercentFloor != null) rangeFloor = (decimal) (RangePercentFloor * upperRange / 100);
            //посчитаем погрешность
            var resultTol = val * TolOfMeasPoint + rangeFloor + (Resolution ?? 0);
            
            //округлим по числу знаков разрешения
            if (Resolution != null)
            {
                var mantisa = MathStatistics.GetMantissa(Resolution);
                resultTol = Math.Round(resultTol, mantisa, MidpointRounding.AwayFromZero);
            }
            
            return resultTol;
        }

        #endregion
    }
}