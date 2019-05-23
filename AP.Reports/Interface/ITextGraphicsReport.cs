using System.Data;
using System.Drawing;
using AP.Reports.Utils;

namespace AP.Reports.Interface
{
    /// <summary>
    /// Интерфейс обеспечивающий основыные функции для создания отчетов Word, Excel
    /// </summary>
    public interface ITextGraphicsReport : IReport
    {

        /// <summary>
        /// Заменяет в документе все вхождения на указанное изображение
        /// </summary>
        void FindStringAndAllReplaceImage(string sFind, Bitmap image);

        /// <summary>
        /// Заменяет первое вхождение на указанное изображение
        /// </summary>
        /// <param name="sFind">Найти</param>
        /// <param name="image">Заменить</param>
        void FindStringAndReplaceImage(string sFind, Bitmap image);

        /// <summary>
        /// Создает новый документ по указанному шаблону
        /// </summary>
        /// <param name="templatePath">Путь к шаблону</param>
        void NewDocumentTemp(string templatePath);

        /// <summary>
        /// Заполняет таблицу на закладку(Именованный диапазон)
        /// </summary>
        /// <param name="bm">Наименование закладки</param>
        /// <param name="dt">Таблица с данными</param>
        /// <param name="del">Признак удаления таблицы, если нет данных</param>
        void FillsTableToBookmark(string bm, DataTable dt, bool del = false, ConditionalFormatting cf = default(ConditionalFormatting));

        /// <summary>
        /// Создает новую таблицу на указанной закладке
        /// </summary>
        /// <param name="bm">Наименование закладки</param>
        /// <param name="dt">Таблица с данными</param>
        /// <param name="cf">Условия форматирования</param>
        void InsertNewTableToBookmark(string bm, DataTable dt, ConditionalFormatting cf = default(ConditionalFormatting));

        /// <summary>
        /// Вставляет текст на закладку
        /// </summary>
        /// <param name="bm">Наименование закладки</param>
        /// <param name="text">Текст</param>
        /// <param name="bm">Наименование закладки</param>
        void InsertTextToBookmark(string bm, string text);
        
        /// <summary>
        /// Вставка картинки на закладку
        /// </summary>
        /// <param name="bm">Имя закладки</param>
        /// <param name="image">Картинка</param>
        /// <param name="bm">Имя закладки</param>
        void InsertImageToBookmark(string bm, Bitmap image);

        /// <summary>
        /// Вставка картинки в выделенный диапазон
        /// </summary>
        void InsertImage(Bitmap image);

        /// <summary>
        /// Создает и вставляет таблицу
        /// </summary>
        /// <param name="dt">Таблица для вставки</param>
        void InsertTable(DataTable dt, ConditionalFormatting cf = default(ConditionalFormatting));
    }
}
