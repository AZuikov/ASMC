using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Core.Interface
{
    /// <summary>
    /// Представляет интерфейс модели,
    /// поддерживающий запрос на закрытие
    /// связанного представления (View).
    /// </summary>
    public interface ISupportClose
    {
        event EventHandler RequestClose;

        /// <summary>
        /// Возвращает истинно, если закрытие
        /// связанного представления (View)
        /// возможно при текущем состоянии
        /// модели; иначе ложно.
        /// </summary>
        bool CanClose
        {
            get;
        }
    }
}
