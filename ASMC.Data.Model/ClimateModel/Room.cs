using ASMC.Data.Model.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Data.Model.ClimateModel
{
    public class Room : IRoom
    {
        #region IRoom
        /// <summary>
        /// Gets or sets the name room.
        /// </summary>
        /// <value>
        /// Наименование помещения
        /// </value>
        public string NameRoom { get; set; }
        /// <summary>
        /// Датчики в помещении
        /// </summary>
        /// <value>
        /// Перечень датчиков в помещении
        /// </value>
        public List<ISensor> SensorsList { get; set; }
        #endregion
    }
}
