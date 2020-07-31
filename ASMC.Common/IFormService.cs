using DevExpress.Mvvm;

namespace ASMC.Common
{        /// <summary>
    /// Представляет интерфейс сервиса
    /// справочника данных.
    /// </summary>
    public interface IFormService : ISupportParameter
    {
        /// <summary>
        /// Возвращает или задает заголовок
        /// для справочника.
        /// </summary>
        string Title
        {
            get; set;
        }

        /// <summary>
        /// Возвращает или задает выбранную
        /// объектную сущность в справочнике.
        /// </summary>
        object Entity
        {
            get; set;
        }

        /// <summary>
        /// Отображает пользовательский
        /// интерфейс справочника.
        /// </summary>
        bool Show();
    }
}
