using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Devices.Rohde_Schwarz
{
    interface IOscilloscope
    {
        /// <summary>
        /// Выбирает канал, с которым дальше работаем
        /// </summary>
        void SetWorkingChanel();
    }
}
