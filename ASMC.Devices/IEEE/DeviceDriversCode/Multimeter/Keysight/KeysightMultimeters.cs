using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Utils.Data;
using ASMC.Devices.Interface;

namespace ASMC.Devices.IEEE.Keysight.Multimeter
{
    public class Keysight34401A: BaseDigitalMultimetr344xx
    {
        public Keysight34401A()
        {
            UserType = "34401A";

        }
    }

    public class Keysight34461A : BaseDigitalMultimetr344xx
    {
        public Keysight34461A()
        {
            UserType = "34461A";
        }
    }
}
