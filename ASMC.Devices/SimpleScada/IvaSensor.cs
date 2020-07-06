using System;
using System.Collections.Generic;


namespace ASMC.Devices.SimpleScada
{
    public sealed class IvaSensor : Sensor
    {

        public IvaSensor(string name )
        {
            SensorName = name;
        }

    }
}
