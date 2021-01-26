using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.IEEE.Keysight.PowerSupplyes.E36XXa
{
    class E3633ADevices:E36xxA_Device
    {
        public E3633ADevices()
        {
            UserType = "E3633A";
            outputs = new[] { E364xChanels.OUTP1 };
            Ranges = new[]
            {
                new MeasPoint<Voltage, Current>(8, 20),
                new MeasPoint<Voltage, Current>(20, 10)
            };
        }
    }
}
