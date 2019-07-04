using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Core.Settings
{
    /// <summary>
    /// Предствляет параметры окна
    /// </summary>
    public class WindowSettingsBase : IWindowSettings
    {
        /// <summary>
        /// Возвращает или задает ширину окна
        /// </summary>
        public int Width
        {
            get; set;
        }

        /// <summary>
        /// Возвращает или задает высоту окна
        /// </summary>
        public int Height
        {
            get; set;
        }
    }
}
