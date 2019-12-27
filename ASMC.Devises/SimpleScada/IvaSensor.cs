using System;
using System.Collections.Generic;
using ASMC.Data.Model.Devises.Parametr;
using ASMC.Data.Model.Interface;
using ASMC.Data.Model.SimpleScada;

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
