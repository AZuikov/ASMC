using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Devices.SimpleScada
{
    public class Dv2TsSensor : Sensor
    {
        public Dv2TsSensor(string name)
        {
            SensorName = name;
        }

    }
}
