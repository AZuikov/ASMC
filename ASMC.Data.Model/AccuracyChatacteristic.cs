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
        /// Позволяет получать или задавать разрешение, включает отклонение едениц младшего разряда
        /// </summary>
       public decimal? Resolution { get;  }
        /// <summary>
        /// Процент от предела
        /// </summary>
        public decimal? Floor { get;  }
        /// <summary>
        /// Процент от измеряемой велечины
        /// </summary>
        public decimal? Tol { get;  }

        public AccuracyChatacteristic(decimal? resolution, decimal? floor, decimal? tol)
        {
            Resolution = resolution;
            Floor = floor;
            Tol = tol;
        }
        decimal GetAccuracy(decimal val, decimal upperRange=0)
        {
            decimal tol = 0;
            decimal flr = 0;
            if (Tol!=null) tol = (decimal) ((Tol * val) / 100);

            if (Floor!=null) flr = (decimal) (Floor * upperRange / 100);

            return tol+ flr+Resolution??0;
        }
    }
}
