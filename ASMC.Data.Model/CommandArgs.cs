namespace ASMC.Data.Model
{
    /// <summary>
    /// Аргументы передаваемые через IDA
    /// </summary>
    public class CommandArgs
    {
        /// <summary>
        ///     ID элемента
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     Из какого окна была запущена программа?
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        ///     Запрос к БД
        /// </summary>
        public string Query { get; set; }
    }
}