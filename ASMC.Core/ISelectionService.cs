using System.ComponentModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;

namespace ASMC.Core
{
    public interface ISelectionService: ISupportParameter
    {
        /// <summary>
        /// Возвращает или задает выбранную
        /// объектную сущность.
        /// </summary>
        object Entity { get; set; }
        INotifyPropertyChanged ViewModel { get; set; } 
        /// <summary>
        /// Возвращает или задает название
        /// представления.
        /// </summary>
        string DocumentType { get; set; }
        /// <summary>
        /// Возвращает или задает локатор
        /// представлений для поиска представления.
        /// </summary>
        ViewLocator ViewLocator { get; set; }
        /// <summary>
        /// Возвращает или задает заголовок
        /// для окна выбора.
        /// </summary>
        string Title
        {
            get; set;
        }
        /// <summary>
        /// Отображает интерфес
        /// </summary>
        /// <returns>Возвращает результат работы интерфейса</returns>
        bool? Show();
    }
}
