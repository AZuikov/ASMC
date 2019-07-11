using System.Data;
using System.Drawing;
using AP.Reports.AutoDocumets;
using AP.Reports.Utils;

namespace AP.Reports.Interface
{
    /// <summary>
    /// Интерфейс обеспечивающий основыные функции для создания отчетов графический текстовых редакторов
    /// </summary>
    public interface ITextGraphicsReport : IReport
    {

        /// <summary>
        /// Заменяет в документе все вхождения на указанное изображение
        /// </summary>
        /// <param name="sFind">Заменяемая строка</param>
        /// <param name="image">Изображение</param>
        /// <param name="scale">Масштаб изображения</param>
        void FindStringAndAllReplaceImage(string sFind, Bitmap image, float scale =1);

        /// <summary>
        /// Заменяет первое вхождение на указанное изображение
        /// </summary>
        /// <param name="sFind">Заменяемая строка</param>
        /// <param name="image">Изображение</param>
        /// <param name="scale">Масштаб изображения</param>
        void FindStringAndReplaceImage(string sFind, Bitmap image, float scale = 1);

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
        /// <param name="cf">Условия форматирования</param>
        void FillTableToBookmark(string bm, DataTable dt,  bool del = false, Document.ConditionalFormatting cf = default(Document.ConditionalFormatting));
        /// <summary>
        /// Создает новую таблицу на указанной закладке
        /// </summary>
        /// <param name="bm">Наименование закладки</param>
        /// <param name="dt">Таблица с данными</param>
        /// <param name="cf">Условия форматирования</param>
        void InsertNewTableToBookmark(string bm, DataTable dt,  Document.ConditionalFormatting cf = default(Document.ConditionalFormatting));
        /// <summary>
        /// Вставляет текст на закладку
        /// </summary>
        /// <param name="bm">Наименование закладки</param>
        /// <param name="text">Текст</param>
        void InsertTextToBookmark( string bm,string text);

        /// <summary>
        /// Вставка картинки на закладку
        /// </summary>
        /// <param name="bm">Имя закладки</param>
        /// <param name="image">Изображение</param>
        /// <param name="scale">Масштаб</param>
        void InsertImageToBookmark(string bm, Bitmap image, float scale = 1);

        /// <summary>
        /// Вставка картинки в выделенный диапазон
        /// </summary>
        /// <param name="image">Изображение</param>
        /// <param name="scale">Масштаб</param>
        void InsertImage(Bitmap image, float scale = 1);

        /// <summary>
        /// Создает и вставляет таблицу
        /// </summary>
        /// <param name="dt">Таблица для вставки</param>
        /// <param name="cf">Условия форматирования</param>
        void InsertTable(DataTable dt, Document.ConditionalFormatting cf = default(Document.ConditionalFormatting));
    }
}
