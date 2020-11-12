using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Data.Model
{
    /// <summary>
    /// Предоставляет реализацию характеристики точности.
    /// </summary>
    public sealed class AccuracyChatacteristic
    {
        /// <summary>
        /// Позволяет получать или задавать разрешение, включает число едениц младшего разряда
        /// </summary>
       public decimal? Resolution { get;  }
        /// <summary>
        /// Процент от предела
        /// </summary>
        public decimal? RangePercentFloor { get;  }
        /// <summary>
        /// Процент от измеряемой велечины
        /// </summary>
        public decimal? PercentOfMeasurePointTol { get;  }

        public AccuracyChatacteristic(decimal? resolution, decimal? rangePercentFloor, decimal? percentOfMeasurePointTol)
        {
            Resolution = resolution;
            RangePercentFloor = rangePercentFloor;
            PercentOfMeasurePointTol = percentOfMeasurePointTol;
        }
       public  decimal GetAccuracy(decimal val, decimal upperRange=0)
        {
            decimal TolOfMeasPoint = 0;
            decimal rangeFloor = 0;
            if (PercentOfMeasurePointTol!=null) TolOfMeasPoint = (decimal) (PercentOfMeasurePointTol  / 100);

            if (RangePercentFloor!=null) rangeFloor = (decimal) (RangePercentFloor * upperRange / 100);

            return val*TolOfMeasPoint + rangeFloor+Resolution??0;
        }
    }
}
