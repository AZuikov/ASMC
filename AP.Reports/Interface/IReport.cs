using System.Data;
using System.Drawing;
using AP.Reports.Utils;

namespace AP.Reports.Interface
{
    /// <summary>
    /// Интерфейс обеспечивающий основыные функции для создания отчетов Word, Excel
    /// </summary>
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
        /// Заменяет в документе все вхождения на указанное изображение
        /// </summary>
        void FindStringAndAllReplacImage(string sFind, Bitmap image);
        /// <summary>
        /// Заменяет первое вхождение на указанную строку
        /// </summary>
        /// <param name="sFind">Найти</param>
        /// <param name="sReplac">Заменить</param>
        /// <param name="invert">Указывает направление вхождения. По умолчания поиск с начала документа</param>
        void FindStringAndReplac(string sFind, string sReplac, bool invert = true);
        /// <summary>
        /// Заменяет первое вхождение на указанное изображение
        /// </summary>
        /// <param name="sFind">Найти</param>
        /// <param name="image">Заменить</param>
        /// <param name="invert">Указывает направление</param>
        void FindStringAndReplacImage(string sFind, string image, bool invert = true);

        /// <summary>
        /// Создает новый документ
        /// </summary>
        void NewDocument();

        /// <summary>
        /// Создает новый документ по указанному шаблону
        /// </summary>
        /// <param name="templatePath">Путь к шаблону</param>
        void NewDocumentTemp(string templatePath);
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
        /// Вставляет таблицу текущий диапазон
        /// </summary>
        /// <param name="dt">Талица для вставки</param>
        void InsertTable(DataTable dt);
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
        /// Вставка картинки на закладку
        /// </summary>
        /// <param name="image">Картинка</param>
        /// <param name="bm">Имя закладки</param>
        void InsertImageToBookmark(Bitmap image, string bm);
        /// <summary>
        /// Вставка картинки в выделенный диапазон
        /// </summary>
        void InsertImage(Bitmap image);
        /// <summary>
        /// Объеденяет документы
        /// </summary>
        /// <param name="pathdoc">Путь к объеденяемому документу</param>
        void MergeDocuments(string pathdoc);
    }
}
