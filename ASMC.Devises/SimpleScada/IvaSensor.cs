using System;
using System.Collections.Generic;
using ASMC.Data.Model.Interface;

namespace ASMC.Devises.SimpleScada
{
    public sealed class IvaSensor : Sensor
    {

        public IvaSensor(string name )
        {
            SensorName = name;
        }

    }
}
