using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Devices.IEEE.Keysight.Multimeter
{
    public class Keysight34401A_Mult: BaseDigitalMultimetr344xx
    {
        public Keysight34401A_Mult()
        {
            UserType = "34401A";
        }
    }
}
