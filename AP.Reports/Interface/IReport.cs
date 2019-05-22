using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AP.Reports.Interface
{
    public interface IReport
    {
        /// <summary>
        /// Путь к файлу
        /// </summary>
        string Path { get; }
        /// <summary>
        /// Сохраняет текущий документ по указаному пути
        /// </summary>
        void SaveAs(string pathToSave);
        /// <summary>
        /// Закрывает текущий документ
        /// </summary>
        void Close();
        /// <summary>
        /// Заменяет в документе все вхождения на указаную строку
        /// </summary>
        void FindStringAndAllReplace(string sFind, string sReplace);
        /// <summary>
        /// Заменяет первое вхождение на указанную строку
        /// </summary>
        /// <param name="sFind">Найти</param>
        /// <param name="sReplace">Заменить</param>
        void FindStringAndReplace(string sFind, string sReplace);

        /// <summary>
        /// Создает новый документ
        /// </summary>
        void NewDocument();
        /// <summary>
        /// Открыть существующий документ
        /// </summary>
        /// <param name="sPath">Путь к открываемому документу</param>
        /// <exception cref="System.IO.IOException">Не удается открыть файл</exception>
        void OpenDocument(string sPath);
        /// <summary>
        /// Сохраняет текущий документ
        /// </summary>
        void Save();
        
        /// <summary>
        /// Втавка текста в выделенный диапазон
        /// </summary>
        /// <param name="text">Текст</param>
        void InsertText(string text);

        /// <summary>
        /// Объеденяет документы
        /// </summary>
        /// <param name="pathdoc">Путь к объеденяемому документу</param>
        void MergeDocuments(string pathdoc);
        /// <summary>
        /// Обеденяет множество документов
        /// </summary>
        /// <param name="pathdoc">Путь к объеденяемому документу</param>
        void MergeDocuments(IEnumerable<string> pathdoc);
        /// <summary>
        /// Установка курсора в конец документа
        /// </summary>
        void MoveEnd();
        /// <summary>
        /// Установка курсора в начало документа
        /// </summary>
        void MoveHome();
    }
}
