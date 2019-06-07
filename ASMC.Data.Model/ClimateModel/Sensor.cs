using ASMC.Data.Model.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Data.Model.ClimateModel
{
    /// <summary>
    /// Описывает датчик 
    /// </summary>
    public class Sensor : ISensor
    {
        #region ISensors
        public string SensorName { get; set; }

        public List<IParametrs> Parametrs { get; set; }
        #endregion

    }
}
