using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Core
{
    /// <summary>
    /// Представляет интерфейс модели,
    /// поддерживающий запрос на закрытие
    /// связанного представления (View).
    /// </summary>
    public interface ISupportClose
    {
        /// <summary>
        /// Происходит при запросе на закрытие
        /// связанного представления (View).
        /// </summary>
        event EventHandler RequestClose;

        /// <summary>
        /// Возвращает истинно, если закрытие
        /// связанного представления (View)
        /// возможно при текущем состоянии
        /// модели; иначе ложно.
        /// </summary>
        /// <param name="state">Объект,
        /// содержащий состояния
        /// модели.</param>
        bool CanClose(object state);
    }
}
