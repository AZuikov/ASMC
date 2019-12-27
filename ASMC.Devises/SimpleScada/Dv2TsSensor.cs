using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model.Devises.Parametr;
using ASMC.Data.Model.Interface;

namespace ASMC.Devises.SimpleScada
{
    public class Dv2TsSensor : Sensor
    {
        public Dv2TsSensor(string name)
        {
            SensorName = name;
        }

    }
}
