using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;

namespace AP.Utils.Data
{
    /// <summary>
    /// Представляет поставщика данных
    /// из системы управления базами
    /// данных  Microsoft SQL Server.
    /// </summary>
    public class SqlDataProvider : IDataProvider, IDisposable
    {
        #region Fields

        private string _dataSource = Environment.MachineName;
        private string _catalog;
        private string _user;
        private bool _integratedSecurity;
        private DbConnection _connection;
        private static int _commandTimeout = 300;

        private readonly SecureString _password = new SecureString();

        #endregion

        #region Properties

        /// <inheritdoc />
        public string DataSource
        {
            get => _dataSource;
            set
            {
                if (_dataSource != value)
                {
                    _dataSource = value;
                    ClearConnection();
                }
            }
        }

        /// <inheritdoc />
        public string Catalog
        {
            get => _catalog;
            set
            {
                if (_catalog != value)
                {
                    _catalog = value;
                    ClearConnection();
                }
            }
        }

        /// <inheritdoc />
        public string User
        {
            get => _user;
            set
            {
                if (_user != value)
                {
                    _user = value;
                    ClearConnection();
                }
            }
        }

        /// <inheritdoc />
        public bool IntegratedSecurity
        {
            get => _integratedSecurity;
            set
            {
                if (_integratedSecurity != value)
                {
                    _integratedSecurity = value;
                    if (_integratedSecurity)
                    {
                        User = $"{Environment.UserDomainName}\\{Environment.UserName}";
                        Password = "";
                    }

                    ClearConnection();
                }
            }
        }

        /// <inheritdoc />
        public string Password
        {
            get
            {
                var ptr = IntPtr.Zero;
                try
                {
                    ptr = Marshal.SecureStringToGlobalAllocUnicode(_password);
                    return Marshal.PtrToStringUni(ptr);
                }
                finally
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(ptr);
                }
            }
            set
            {
                if (Equals(Password, value))
                    return;

                _password.Clear();
                if (!string.IsNullOrEmpty(value))
                    foreach (var c in value.ToCharArray())
                        _password.AppendChar(c);
            }
        }

        /// <inheritdoc />
        public string ConnectionString
        {
            get
            {
                var iss = _integratedSecurity ? ";Integrated Security=SSPI" : null;
                return $"Data Source={_dataSource};Initial Catalog={_catalog}{iss};User Id={_user};Password={Password}";
            }
        }

        #endregion

        /// <summary>
        /// Инициализирует новый экземпляр класса
        /// <see cref="SqlDataProvider"/>.
        /// </summary>
        public SqlDataProvider()
        {
            IntegratedSecurity = true;
        }

        #region Methods

        #region IDataProvider

        /// <inheritdoc />
        public bool TestConnection(bool throwOnException)
        {
            DbConnection connection = null;
            try
            {
                connection = GetConnection();
                connection.Open();

                return true;
            }
            catch (Exception)
            {
                if (!throwOnException)
                    return false;

                throw;
            }
            finally
            {
                connection?.Close();
            }
        }

        /// <inheritdoc />
        public void OpenConnection()
        {
            if (_connection == null)
                _connection = GetConnection();

            if (_connection.State != ConnectionState.Open)
                _connection.Open();
        }

        /// <inheritdoc />
        public void CloseConnection()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
                _connection.Close();
        }
        /// <inheritdoc />
        public int ExecuteNonQuery(string commandText, bool storedProcedure = false, params DbParameter[] parameters)
        {
            var connectionOpened = false;
            try
            {
                connectionOpened = EnsureConnection(ref _connection);

                using (var cmd = CreateCommand(commandText, null,
                    storedProcedure ? CommandType.StoredProcedure : CommandType.Text, parameters))
                    return cmd.ExecuteNonQuery();
            }
            finally
            {
                if (_connection != null && !connectionOpened)
                    _connection.Close();
            }
        }

        /// <inheritdoc />
        public object ExecuteScalar(string commandText, bool storedProcedure = false, params DbParameter[] parameters)
        {
            var connectionOpened = false;
            try
            {
                connectionOpened = EnsureConnection(ref _connection);

                using (var cmd = CreateCommand(commandText, null,
                    storedProcedure ? CommandType.StoredProcedure : CommandType.Text, parameters))
                    return cmd.ExecuteScalar();
            }
            finally
            {
                if (_connection != null && !connectionOpened)
                    _connection.Close();
            }
        }

        /// <inheritdoc />
        public DataTable Execute(string commandText, bool storedProcedure = false, params DbParameter[] parameters)
        {
            var connectionOpened = false;
            try
            {
                connectionOpened = EnsureConnection(ref _connection);

                var dataSet = new DataSet();
                var adapter = GetDataAdapter(commandText,
                    storedProcedure ? CommandType.StoredProcedure : CommandType.Text, parameters);

                adapter.Fill(dataSet);
                return dataSet.Tables.Count > 0 ? dataSet.Tables[0] : null;
            }
            finally
            {
                if (_connection != null && !connectionOpened)
                    _connection.Close();
            }
        }

        /// <inheritdoc />
        public DbParameter GetParameter(string name, object value)
        {
            var param = new SqlParameter(name, value ?? DBNull.Value);
            if (value is Array array)
                param.Size = Buffer.ByteLength(array);

            return param;
        }

        #endregion

        #region IClonable

        /// <summary>
        /// Создает полную копию поставщика данных.
        /// </summary>
        /// <returns>Возвращает новый экземпляр
        /// <see cref="SqlDataProvider"/>.</returns>
        public object Clone()
        {
            return new SqlDataProvider
            {
                DataSource = _dataSource,
                Catalog = _catalog,
                User = _user,
                IntegratedSecurity = _integratedSecurity,
                Password = Password
            };
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _password?.Dispose();
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"DataSource={_dataSource}, Catalog={_catalog}";
        }

        /// <summary>
        /// Создает новое подключение к
        /// источнику данных.
        /// </summary>
        /// <returns>Возвращает <see cref="DbConnection"/>.</returns>
        protected DbConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        /// <summary>
        /// Создает команду для обращения
        /// к источнику данных.
        /// </summary>
        /// <param name="commandText">Строка,содержащая текст команды.</param>
        /// <param name="transaction">Активная транзакция, в которой
        /// выполняется команда.</param>
        /// <param name="commandType">Тип команды.</param>
        /// <param name="parameters">Параметры, передаваемые с командой.</param>
        /// <returns>Возвращает экземпляр <see cref="DbCommand"/>.</returns>
        protected DbCommand CreateCommand(string commandText, DbTransaction transaction,
            CommandType commandType = CommandType.Text, params DbParameter[] parameters)
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
                throw new InvalidOperationException();

            var command = _connection.CreateCommand();

            command.CommandTimeout = _commandTimeout;
            command.CommandText = commandText;
            command.CommandType = commandType;
            command.Transaction = transaction;

            if (parameters.Any())
                command.Parameters.AddRange(parameters);

            return command;
        }

        /// <summary>
        /// Сбрасывает активное соединение
        /// с источником данных.
        /// </summary>
        protected void ClearConnection()
        {
            CloseConnection();
            _connection = null;
        }

        private IDataAdapter GetDataAdapter(string selectCommandText, CommandType commandType = CommandType.Text,
            params DbParameter[] parameters)
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
                throw new InvalidOperationException();

            using (var cmd = CreateCommand(selectCommandText, null, commandType, parameters))
            {
                return new SqlDataAdapter((SqlCommand)cmd);
            }
        }

        private bool EnsureConnection(ref DbConnection connection)
        {
            if (connection == null)
                connection = GetConnection();

            if (connection.State != ConnectionState.Open)
                connection.Open();
            else
                return true;

            return false;
        }

        #endregion
    }
}
