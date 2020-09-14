using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Devices.USB_Device.SKBIS.Lir917
{
    public class Ppi
    {
        public double CalibrationFactor { get;  set; }

        /// <summary>
        /// Коды напровления движения датчика
        /// </summary>
        public enum Morion
        {
            /// <summary>
            /// Прямой ход
            /// </summary>
            DerectNove,
            /// <summary>
            /// Обратный ход
            /// </summary>
            ReverseMove,
            /// <summary>
            /// неопределенный холд
            /// </summary>
            UndentUncertainMove
        }

        public double MeasValue
        {
            get;
            protected set;
        }
    }
}
