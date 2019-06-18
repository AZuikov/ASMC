using System;
using System.Data;
using System.Data.Common;
using System.Security.Cryptography;

namespace AP.Utils.Data
{
    /// <summary>
    /// Методы расширения для интерфейса
    /// <see cref="IDataProvider"/>.
    /// </summary>
    public static class DataProviderExtensions
    {
        /// <summary>
        /// Производит шифрование пароля, используемого
        /// для авторизации на источнике данных.
        /// </summary>
        /// <param name="dataProvider">Экземпляр <see cref="IDataProvider"/>.</param>
        /// <returns>Возвращает строку, содержащую зашифрованный пароль.</returns>
        public static string GetSecurePassword(this IDataProvider dataProvider)
        {
            var key = Utils.DigitalProductId;
            var pwd = dataProvider.Password;

            return Utils.Encrypt(ref pwd, key);
        }

        /// <summary>
        /// Производит расшифровку пароля, используемого
        /// для авторизации на источнике данных.
        /// </summary>
        /// <param name="dataProvider">Экземпляр <see cref="IDataProvider"/>.</param>
        /// <param name="encriptedString">Строка, содержащая содержащая зашифрованный пароль.</param>
        public static void SetSecurePassword(this IDataProvider dataProvider, string encriptedString)
        {
            var key = Utils.DigitalProductId;
            try
            {
                dataProvider.Password = Utils.Decrypt(encriptedString, key);
            }
            catch(CryptographicException)
            {
                dataProvider.Password = null;
            }
        }

        /// <summary>
        /// Выполняет запрос к источнику данных.
        /// Результат запроса не ожидается.
        /// </summary>
        /// <param name="dataProvider">Экземпляр <see cref="IDataProvider"/>.</param>
        /// <param name="commandText">Строка, содержащая текст команды запроса.</param>
        /// <param name="parameters">Параметры, которые передаются при запросе.</param>
        /// <returns>Возвращает количество записей,
        /// на которые оказал воздействие запрос.</returns>
        public static int ExecuteNonQuery(this IDataProvider dataProvider, string commandText,
            params DbParameter[] parameters)
        {
            return dataProvider.ExecuteNonQuery(commandText, false, parameters);
        }

        /// <summary>
        /// Выполняет запрос к источнику данных.
        /// Результатом запроса ожидается объект.
        /// </summary>
        /// <param name="dataProvider">Экземпляр <see cref="IDataProvider"/>.</param>
        /// <param name="commandText">Строка, содержащая текст команды запроса.</param>
        /// <param name="parameters">Параметры, которые передаются при запросе.</param>
        /// <returns>Возвращает экземпляр <see cref="object"/>.</returns>
        public static object ExecuteScalar(this IDataProvider dataProvider, string commandText,
            params DbParameter[] parameters)
        {
            return dataProvider.ExecuteScalar(commandText, false, parameters);
        }

        /// <summary>
        /// Выполняет запрос к источнику данных.
        /// Результатом запроса ожидается
        /// таблица данных.
        /// </summary>
        /// <param name="dataProvider">Экземпляр <see cref="IDataProvider"/>.</param>
        /// <param name="commandText">Строка, содержащая текст команды запроса.</param>
        /// <param name="parameters">Параметры, которые передаются при запросе.</param>
        /// <returns>Возвращает экземпляр <see cref="DataTable"/>.</returns>
        public static DataTable Execute(this IDataProvider dataProvider, string commandText,
            params DbParameter[] parameters)
        {
            return dataProvider.Execute(commandText, false, parameters);
        }

        /// <summary>
        /// Создает новый параметр для передачи
        /// в запрос к источнику данных.
        /// </summary>
        /// <param name="dataProvider">Экземпляр <see cref="IDataProvider"/>.</param>
        /// <param name="name">Название параметра.</param>
        /// <param name="type">Тип параметра</param>
        /// <param name="value">Значение параметра.</param>
        /// <returns>Возвращает новый экземплряр
        /// <see cref="DbParameter"/>.</returns>
        public static DbParameter GetParameter(this IDataProvider dataProvider, string name, DbType type, object value)
        {
            var param = dataProvider.GetParameter(name, value);
            param.DbType = type;

            return param;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataProvider">Экземпляр <see cref="IDataProvider"/>.</param>
        /// <param name="value"></param>
        public static object EscapeQuotes(this IDataProvider dataProvider, object value)
        {
            if(value is string s)
                return s.Replace("'", "''").Replace("\"", "\"\"");
            return value;
        }

        /// <summary>
        /// Загружает параметры подключения
        /// к источнику данных из файла
        /// универсальной связи данных UDL.
        /// </summary>
        /// <param name="dataProvider">Экземпляр <see cref="IDataProvider"/>.</param>
        /// <param name="fileName">Строка, содержащая путь к файлу UDL.</param>
        public static void LoadFromUdl(this IDataProvider dataProvider, string fileName)
        {
            if(fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            var udl = UdlBuilder.ParseFile(fileName);

            dataProvider.DataSource = udl.DataSource;
            dataProvider.Catalog = udl.Catalog;
            dataProvider.User = udl.Username;
            dataProvider.Password = udl.Password;
            dataProvider.IntegratedSecurity = udl.IntegratedSecurity;
        }

        /// <summary>
        /// Возвращает контекст доступа к
        /// объектным сущностям источника
        /// данных.
        /// </summary>
        /// <param name="dataProvider">Экземпляр <see cref="IDataProvider"/>.</param>
        public static IEntityContext GetEntityContext(this IDataProvider dataProvider)
        {
            return new EntityContext(dataProvider);
        }
    }
}
