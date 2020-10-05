using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Devices.IEEE.Tektronix.Oscilloscope.TDS_2022B
{
   public class TDS_2022B : TDS_Oscilloscope
    {
        public TDS_2022B()
        {
            UserType = "TDS 2022B";
            ChanelCount = 2;
        }
    }
}
