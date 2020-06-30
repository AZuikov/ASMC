using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ASMC.Devises.Annotations;
using Palsys.Data.Model.Metr;

namespace ASMC.Devises.VirtMeasInst
{
    public class NetworkVoltQualityMeter : Sensor
    {
        public NetworkVoltQualityMeter(string Name)
        {
            SensorName = Name;
        }
    }
}
