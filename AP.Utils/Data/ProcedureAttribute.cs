using System;

namespace AP.Utils.Data
{
    /// <summary>
    /// Задает операцию на базе данных,
    /// которую выполняет хранимая
    /// процедура.
    /// </summary>
    public enum StoredProcedureOp
    {
        /// <summary>
        /// Задает операцию вставки записи.
        /// </summary>
        Insert,
        /// <summary>
        /// Задает операцию обновления записи.
        /// </summary>
        Update,
        /// <summary>
        /// Задает операцию удаления записи.
        /// </summary>
        Delete,
        /// <summary>
        /// Задает операцию выборки записи.
        /// </summary>
        Select,
        /// <summary>
        /// Задает операцию выборки множества записей.
        /// </summary>
        SelectMany
    }

    /// <summary>
    /// Представляет хранимую процедуру базы данных,
    /// используемую для выполнения действий с данными
    /// проецируемой сущности.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class StoredProcedureAttribute : Attribute
    {
        /// <summary>
        /// Возвращает имя хранимой процедуры.
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// Возвращает или задает операцию,
        /// выполняемую хранимой процедурой.
        /// </summary>
        public StoredProcedureOp Operation { get; set; } = StoredProcedureOp.Select;
        public bool IsStoredProcedure { get; set; } = true;
        /// <summary>
        /// Возвращает или задает имя параметра
        /// хранимой процедуры, принимающего
        /// уникальное идентификационное
        /// свойство сущности.
        /// </summary>
        public string KeyName
        {
            get; set;
        }

        /// <summary>
        /// Возвращает или задает строку
        /// форматирования для параметра,
        /// передающего значение уникального
        /// идентификационного свойства
        /// сущности.
        /// </summary>
        public string KeyFormat
        {
            get; set;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса
        /// <see cref="StoredProcedureAttribute"/>.
        /// </summary>
        /// <param name="procedureName">Имя хранимой
        /// процедуры в базе данных.</param>
        public StoredProcedureAttribute(string procedureName)
        {
            Name = procedureName ?? throw new ArgumentNullException(nameof(procedureName));
        }
    }
}
