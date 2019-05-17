using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AP.Reports.Interface
{
    public interface IReport
    {
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
        void FindStringAndAllReplac(string sFind, string sReplac);
        /// <summary>
        /// Заменяет первое вхождение на указанную строку
        /// </summary>
        /// <param name="sFind">Найти</param>
        /// <param name="sReplac">Заменить</param>
        /// <param name="invert">Указывает направление вхождения. По умолчания поиск с начала документа</param>
        void FindStringAndReplac(string sFind, string sReplac, bool invert = true);

        /// <summary>
        /// Создает новый документ
        /// </summary>
        void NewDocument();
        /// <summary>
        /// Открыть существующий документ
        /// </summary>
        /// <param name="sPath">Путь к открываемому документу</param>
        void OpenDocument(string sPath);
        /// <summary>
        /// Сохраняет текущий документ
        /// </summary>
        void Save();
        /// <summary>
        /// Вставляет таблицу на текущий диапазон
        /// </summary>
        /// <param name="dt">Талица для вставки</param>
        /// <param name="cf">Перечень условий для форматирования(цветовыделения)</param>
        void InsertTable(DataTable dt, IEnumerable<ConditionalFormatting> cf = default(IEnumerable<ConditionalFormatting>);
        /// <summary>
        /// Заполняет таблицу на закладку(Именованный диапазон)
        /// </summary>
        /// <param name="dt">Таблица с данными</param>
        /// <param name="bm">Наименование закладки</param>
        /// <param name="del">Признак удаления таблицы, если нет данных</param>
        void FillsTableToBookmark(DataTable dt, string bm, bool del = false, ConditionalFormatting cf = default(ConditionalFormatting));
        /// <summary>
        /// Создает новую таблицу на указанной закладке
        /// </summary>
        /// <param name="dt">Таблица с данными</param>
        /// <param name="bm">Наименование закладки</param>
        /// <param name="cf">Условия форматирования</param>
        void InsertNewTableToBookmark(DataTable dt, string bm, ConditionalFormatting cf = default(ConditionalFormatting));
        /// <summary>
        /// Вставляет текст на закладку
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="bm">Наименование закладки</param>
        void InsertTextToBookmark(string text, string bm);
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
    }
}
