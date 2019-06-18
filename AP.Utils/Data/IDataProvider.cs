using System;
using System.Data;
using System.Data.Common;

namespace AP.Utils.Data
{
    /// <summary>
    /// Представляет общий интерфейс для
    /// доступа к источнику данных.
    /// </summary>
    public interface IDataProvider : ICloneable
    {
        /// <summary>
        /// Возвращает или задает сетевой адрес
        /// экземпляра источника данных, с
        /// которым устанавливается соединение.
        /// </summary>
        string DataSource
        {
            get; set;
        }

        /// <summary>
        /// Возвращает или задает имя базы
        /// на источнике данных, с которой
        /// устанавливается соединение.
        /// </summary>
        string Catalog
        {
            get; set;
        }

        /// <summary>
        /// Возвращает или задает имя входа,
        /// используемое при авторизации на
        /// источнике данных.
        /// </summary>
        string User
        {
            get; set;
        }

        /// <summary>
        /// Возвращает или задает пароль,
        /// используемый при авторизации
        /// на источнике данных.
        /// </summary>
        string Password
        {
            get; set;
        }

        /// <summary>
        /// Возвращает или задает использование
        /// проверки подлинности Windows при
        /// авторизации на источнике данных.
        /// </summary>
        bool IntegratedSecurity
        {
            get; set;
        }

        /// <summary>
        /// Возвращает форматированную строку
        /// подключения для используемого
        /// источника данных.
        /// </summary>
        string ConnectionString
        {
            get;
        }

        /// <summary>
        /// Устанавливает новое соединение
        /// с текущим источником данных.
        /// </summary>
        /// <returns>Возвращает подключение
        /// с источником данных.</returns>
        IDbConnection OpenConnection();

        /// <summary>
        /// Разрывает активное соединение
        /// с источником данных.
        /// </summary>
        void CloseConnection();

        /// <summary>
        /// Выполняет попытку подключения
        /// с текущими параметрами источника
        /// данных.
        /// </summary>
        /// <param name="throwOnException">Задает состояние
        /// вызоваисключения при ошибке.</param>
        /// <returns>Возвращает истинно, если соединение может
        /// быть установлено; иначе ложно.</returns>
        bool TestConnection(bool throwOnException);

        /// <summary>
        /// Выполняет запрос к источнику данных.
        /// Результат запроса не ожидается.
        /// </summary>
        /// <param name="commandText">Строка, содержащая текст команды запроса.</param>
        /// <param name="storedProcedure">Истинно, если запрос представляет вызов хранимой процедуры; иначе ложно.</param>
        /// <param name="parameters">Параметры, которые передаются при запросе.</param>
        /// <returns>Возвращает количество записей,
        /// на которые оказал воздействие запрос.</returns>
        int ExecuteNonQuery(string commandText, bool storedProcedure, params DbParameter[] parameters);

        /// <summary>
        /// Выполняет запрос к источнику данных.
        /// Результатом запроса ожидается объект.
        /// </summary>
        /// <param name="commandText">Строка, содержащая текст команды запроса.</param>
        /// <param name="storedProcedure">Истинно, если запрос представляет вызов хранимой процедуры; иначе ложно.</param>
        /// <param name="parameters">Параметры, которые передаются при запросе.</param>
        /// <returns>Возвращает экземпляр <see cref="object"/>.</returns>
        object ExecuteScalar(string commandText, bool storedProcedure, params DbParameter[] parameters);

        /// <summary>
        /// Выполняет запрос к источнику данных.
        /// Результатом запроса ожидается
        /// таблица данных.
        /// </summary>
        /// <param name="commandText">Строка, содержащая текст команды запроса.</param>
        /// <param name="storedProcedure">Истинно, если запрос представляет вызов хранимой процедуры; иначе ложно.</param>
        /// <param name="parameters">Параметры, которые передаются при запросе.</param>
        /// <returns>Возвращает экземпляр <see cref="DataTable"/>.</returns>
        DataTable Execute(string commandText, bool storedProcedure, params DbParameter[] parameters);

        /// <summary>
        /// Создает новый параметр для передачи
        /// в запрос к источнику данных.
        /// </summary>
        /// <param name="name">Название параметра.</param>
        /// <param name="value">Значение параметра.</param>
        /// <returns>Возвращает новый экземплряр
        /// <see cref="DbParameter"/>.</returns>
        DbParameter GetParameter(string name, object value);
    }
}
