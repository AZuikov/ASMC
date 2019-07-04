using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ASMC.Core.Settings
{
    /// <summary>
    /// Представляет селектор пользовательских параметров
    /// </summary>
    public class SettingsSelector
    {
        public virtual object SelectSettings(object item, DependencyObject container)
        {
            return null;
        }
    }
}
