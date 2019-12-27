namespace AP.Reports.Interface
{
    /// <summary>
    /// Интерфейс обеспечивающий основыные функции для создания отчетов Word, Excel
    /// </summary>
    public interface IMsOfficeReport : ITextGraphicsReport
    {
        /// <summary>
        ///   Втавка текста в выделенный диапазон в колонтитуле
        /// </summary>
        /// <param name="code"></param>
        void InsertFieldInHeader(string code);
        /// <summary>
        ///  Втавка паоля в выделенный диапазон
        /// </summary>
        /// <param name="code"></param>
        void InsertField(string code);
        /// <summary>
        ///  Заменяет все вхождения в колонтитуле на указанный код поля
        /// </summary>
        /// <param name="sFind">Найти</param>
        /// <param name="sCode">Код поля</param>
        void FindStringInHeaderAndAllReplaceField(string sFind, string sCode);
        /// <summary>
        /// Заменяет все вхождения в колонтитуле на указанную строку
        /// </summary>
        /// <param name="sFind">Найти</param>
        /// <param name="sReplace">Заменить</param>
        void FindStringInHeaderAndAllReplace(string sFind, string sReplace);
    }
}
