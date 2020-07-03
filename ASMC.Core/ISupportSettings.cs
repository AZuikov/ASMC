using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Core
{
    /// <summary>
    /// Представляет интерфейс модели
    /// поддерживающей пользовательские
    /// параметры.
    /// </summary>
    public interface ISupportSettings
    {
        /// <summary>
        /// Возвращает или задает объект,
        /// содержащий параметры.
        /// </summary>
        object Settings
        {
            get; set;
        }
    }
}
