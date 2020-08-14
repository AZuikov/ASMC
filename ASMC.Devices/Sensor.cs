using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Devices.SimpleScada;
using DevExpress.Mvvm;

namespace ASMC.Devices
{

    /// <summary>
    /// Описывает датчики
    /// </summary>
    public abstract class Sensor: ViewModelBase
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
        public virtual BindingList<IParametr> Parametrs
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
