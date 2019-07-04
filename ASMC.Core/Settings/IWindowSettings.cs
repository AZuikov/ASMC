using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Core.Settings
{
    /// <summary>
    /// Представляет параметры окна
    /// </summary>
    public interface IWindowSettings
    {
        /// <summary>
        /// Возвращает или задает ширину окна
        /// </summary>
        int Width
        {
            get; set;
        }

        /// <summary>
        /// Возвращает или задает высоту окна
        /// </summary>
        int Height
        {
            get; set;
        }
    }
}
