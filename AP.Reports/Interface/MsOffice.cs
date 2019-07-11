namespace AP.Reports.Interface
{
    /// <summary>
    /// Интерфейс обеспечивающий основыные функции для создания отчетов Word, Excel
    /// </summary>
    public interface IMsOffice : ITextGraphicsReport
    {
        /// <summary>
        ///   Втавка текста в выделенный диапазон в колонтитуле
        /// </summary>
        /// <param name="code"></param>
        void InsertFiledInHeader(string code);
        /// <summary>
        ///  Втавка паоля в выделенный диапазон
        /// </summary>
        /// <param name="code"></param>
        void InsertFiled(string code);
        /// <summary>
        ///  Заменяет все вхождения в колонтитуле на указанный код поля
        /// </summary>
        /// <param name="sFind">Найти</param>
        /// <param name="sCode">Код поля</param>
        void FindStringInHeaderAndAllReplaceFiled(string sFind, string sCode);
        /// <summary>
        /// Заменяет все вхождения в колонтитуле на указанную строку
        /// </summary>
        /// <param name="sFind">Найти</param>
        /// <param name="sReplace">Заменить</param>
        void FindStringInHeaderAndAllReplace(string sFind, string sReplace);
    }
}
