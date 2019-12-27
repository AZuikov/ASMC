using System.Collections.Generic;
using ASMC.Data.Model.Devises.Parametr;
using ASMC.Data.Model.Interface;

namespace ASMC.Data.Model.SimpleScada
{
    public class Room :IRoom
    {
        #region IRoom
        /// <summary>
        /// Gets or sets the name room.
        /// </summary>
        /// <value>
        /// Наименование помещения
        /// </value>     
        public string Name { get; set; }

        /// <summary>
        /// Датчики в помещении
        /// </summary>
        /// <value>
        /// Перечень датчиков в помещении
        /// </value>
        public List<Sensor> SensorsList
        {
            get; set;
        }
        #endregion
    }
}
