using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Devices.Interface
{
    /// <summary>
    /// Интерфейс получения и записи всех настроек прибора.
    /// </summary>
    public interface IDeviceSettingsControl
    {
        /// <summary>
        /// Получить параметры (настройки) устройства.
        /// </summary>
        void Getting();
        /// <summary>
        /// Записать созданные настройки в устройство.
        /// </summary>
        void Setting();
    }
    /// <summary>
    /// Работа с передней и задней панелью прибора.
    /// </summary>
    public interface IFrontRearPanel
    {
        bool IsFrontTerminal { get;  }
    }
}
