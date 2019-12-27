﻿using System.Windows;

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
