using System;

namespace ASMC.Common
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
        /// <summary>
        /// Вызывается при закрытии представления.
        /// </summary>
        void Close();
    }
}
