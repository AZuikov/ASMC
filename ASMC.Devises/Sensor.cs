using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Devises.SimpleScada;

namespace ASMC.Devises
{

    /// <summary>
    /// Описывает датчики
    /// </summary>
    public abstract class Sensor
    {
        /// <summary>
        /// Наименование датчика
        /// </summary>
        /// <value>
        /// The name of the sensor.
        /// </value>
        public virtual string SensorName
        {
            get; set;
        }

        /// <summary>
        /// Перечень параметров измеряемых датчиком
        /// </summary>
        /// <value>
        /// The parametrs.
        /// </value>
        public virtual List<IParametr> Parametrs
        {
            get; set;
        }

        /// <summary>
        /// Выполняет считываение показаний с датчика
        /// </summary>
        public virtual void UpdateValue()
        {
            if(Parametrs == null)
                return;

            foreach(var parametr in Parametrs)
            {
                parametr.FillValue();
            }
        }
    }
}
