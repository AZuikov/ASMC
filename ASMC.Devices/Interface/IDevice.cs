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
    public interface IDevice
    {
        void Getting();
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
