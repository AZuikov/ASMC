using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model.Devises.Parametr;

namespace ASMC.Data.Model.Interface
{
    /// <summary>
    /// Описывает помещениея
    /// </summary>
    public interface IRoom
    {
        /// <summary>
        /// Наименование помещения
        /// </summary>
        /// <value>
        /// The name room.
        /// </value>
        string Name
        {
            get; set;
        }
        /// <summary>
        /// Датчики в помещении
        /// </summary>
        /// <value>
        /// The sensors list.
        /// </value>
        List<Sensor> SensorsList
        {
            get; set;
        }
    }
}
