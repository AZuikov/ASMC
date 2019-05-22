using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Core
{
    // <summary>
    /// Представляет поставщика данных
    /// </summary>
    public class DataProvider
    {
        #region Fields

        private string _server = Environment.MachineName;
        private string _source;
        private string _user;
        private string _sourceName;
        private bool _integratedSecurity;
        private string _dataLinkFileName;
        private bool _useDataLinkFile;
        private DbConnection _connection;
        private static int _commandTimeout = 300;

        private readonly SecureString _password = new SecureString();

        #endregion

        #region Properties

        /// <summary>
        /// Возвращает или задает сервер
        /// </summary>
        public string Server
        {
            get { return _server; }
            set
            {
                if (_server != value)
                {
                    _server = value;
                    ClearCache();
                }
            }
        }
        /// <summary>
        /// Возвращает или задает имя входа
        /// </summary>
        public string User
        {
            get { return _user; }
            set
            {
                if (_user != value)
                {
                    _user = value;
                    ClearCache();
                }
            }
        }

        /// <summary>
        /// Возвращает или задает путь к файлу UDL
        /// </summary>
        public string DataLinkFileName
        {
            get { return _dataLinkFileName; }
            set
            {
                if (_dataLinkFileName != value)
                {
                    _dataLinkFileName = value;
                    ClearCache();
                }
            }
        }

        public bool UseDataLinkFile
        {
            get => _useDataLinkFile;
            set
            {
                if (_useDataLinkFile != value)
                {
                    _useDataLinkFile = value;
                    ClearCache();
                }
            }
        }

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
                    ClearCache();
                }
            }
        }

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

        public string ConnectionString
        {
            get
            {
                if (_useDataLinkFile)
                    return $"File Name = {_dataLinkFileName}";

                var iss = _integratedSecurity ? ";Integrated Security=SSPI" : null;

                return $"Provider=sqloledb;Data Source={_server};Initial Catalog={_source}{iss};User Id={_user};Password={Password}";
            }
        }

        #endregion

        public DataProvider()
        {
            IntegratedSecurity = true;
        }

        #region Methods

        protected DbConnection GetConnection()
        {
            var str = ConnectionString;
            return (str == null) ? null : new OleDbConnection(str);
        }

        public void ClearCache()
        {
            _connection?.Close();

            _connection = null;
            _sourceName = null;
        }

        public string GetSourceName()
        {
            if (_sourceName != null)
                return _sourceName;

            var connectionOpened = false;

            try
            {
                connectionOpened = EnsureConnection(ref _connection);

                var name = _connection.Database;
                if (string.IsNullOrEmpty(name))
                {
                    var source = _connection.DataSource;
                    if (source != null)
                        name = source.Substring(source.LastIndexOf('\\') + 1);
                }
                _sourceName = name;
            }
            catch
            {
                _sourceName = null;
                throw;
            }
            finally
            {
                if (_connection != null && !connectionOpened)
                    _connection.Close();
            }
            return _sourceName;
        }

        public DbTransaction BeginTransaction()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
                throw new InvalidOperationException();

            return _connection.BeginTransaction();
        }

        public DbCommand CreateCommand(string commandText, DbTransaction transaction, IEnumerable parameters = null)
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
                throw new InvalidOperationException();

            var command = _connection.CreateCommand();
            command.CommandTimeout = _commandTimeout;
            command.CommandText = commandText;
            command.Transaction = transaction;

            if (parameters != null)
                foreach (var param in parameters)
                {
                    if (command is OleDbCommand)
                        command.Parameters.Add(new OleDbParameter { Value = param });
                    else if (command is OdbcCommand)
                        command.Parameters.Add(new OdbcParameter { Value = param });
                }
            return command;
        }

        public int ExecuteNonQuery(string commandText)
        {
            var connectionOpened = false;
            try
            {
                connectionOpened = EnsureConnection(ref _connection);

                using (var cmd = CreateCommand(commandText, null))
                    return cmd.ExecuteNonQuery();
            }
            finally
            {
                if (_connection != null && !connectionOpened)
                    _connection.Close();
            }
        }

        public object ExecuteScalar(string commandText)
        {
            var connectionOpened = false;
            try
            {
                connectionOpened = EnsureConnection(ref _connection);

                using (var cmd = CreateCommand(commandText, null))
                    return cmd.ExecuteScalar();
            }
            finally
            {
                if (_connection != null && !connectionOpened)
                    _connection.Close();
            }
        }

        public int ExecuteProcedureNonQuery(string storedProcedure, params IDbDataParameter[] parameters)
        {
            var connectionOpened = false;
            try
            {
                connectionOpened = EnsureConnection(ref _connection);

                using (var cmd = CreateCommand(storedProcedure, null))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (parameters.Any())
                        cmd.Parameters.AddRange(parameters);

                    return cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (_connection != null && !connectionOpened)
                    _connection.Close();
            }
        }

        public DataTable GetData(string selectCommandText)
        {
            var connectionOpened = false;
            try
            {
                connectionOpened = EnsureConnection(ref _connection);

                var dataSet = new DataSet();
                var adapter = GetDataAdapter(selectCommandText, _connection);

                adapter.Fill(dataSet);
                return dataSet.Tables.Count > 0 ? dataSet.Tables[0] : null;
            }
            finally
            {
                if (_connection != null && !connectionOpened)
                    _connection.Close();
            }
        }

        public DataTable GetDataAsync(string selectQuery)
        {
            var connectionOpened = false;
            try
            {
                connectionOpened = EnsureConnection(ref _connection);

                var dataSet = new DataSet();
                var adapter = GetDataAdapter(selectQuery, _connection);

                adapter.Fill(dataSet);
                return dataSet.Tables.Count > 0 ? dataSet.Tables[0] : null;
            }
            finally
            {
                if (_connection != null && !connectionOpened)
                    _connection.Close();
            }
        }

        public DataTable ExecuteProcedure(string storedProcedure, params IDbDataParameter[] parameters)
        {
            var connectionOpened = false;
            try
            {
                connectionOpened = EnsureConnection(ref _connection);

                using (var command = CreateCommand(storedProcedure, null))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    if (parameters.Any())
                        command.Parameters.AddRange(parameters);

                    var dataSet = new DataSet();
                    var adapter = GetDataAdapter(command);

                    adapter.Fill(dataSet);
                    return dataSet.Tables.Count > 0 ? dataSet.Tables[0] : null;
                }
            }
            finally
            {
                if (_connection != null && !connectionOpened)
                    _connection.Close();
            }
        }

        private static IDataAdapter GetDataAdapter(string selectCommandText, DbConnection connection)
        {
            IDbDataAdapter adapter = null;
            if (connection is OleDbConnection dbConnection)
                adapter = new OleDbDataAdapter(selectCommandText, dbConnection);
            if (connection is OdbcConnection odbcConnection)
                adapter = new OdbcDataAdapter(selectCommandText, odbcConnection);

            if (adapter != null)
                adapter.SelectCommand.CommandTimeout = _commandTimeout;

            return adapter;
        }

        private static IDataAdapter GetDataAdapter(IDbCommand command)
        {
            if (command is OleDbCommand oleDbCommand)
                return new OleDbDataAdapter(oleDbCommand);
            if (command is OdbcCommand odbcCommand)
                return new OdbcDataAdapter(odbcCommand);

            return null;
        }

        public void UpdateData(string tableName, DataTable dataTable)
        {
            if (dataTable == null)
                throw new ArgumentNullException(nameof(dataTable));

            var connectionOpened = false;
            try
            {
                connectionOpened = EnsureConnection(ref _connection);
                var adapter = GetDataAdapter("select * from [" + tableName + "]", _connection);

                if (dataTable.PrimaryKey.Any())
                {
                    if (adapter is OleDbDataAdapter)
                    {
                        var cb = new OleDbCommandBuilder((OleDbDataAdapter)adapter);
                        ((OleDbDataAdapter)adapter).UpdateCommand = cb.GetUpdateCommand();
                    }
                    else if (adapter is OdbcDataAdapter)
                    {
                        var cb = new OdbcCommandBuilder((OdbcDataAdapter)adapter);
                        ((OdbcDataAdapter)adapter).UpdateCommand = cb.GetUpdateCommand();
                    }
                }
                else
                    throw new InvalidOperationException(string.Format("Обновление данных невозможно. Таблица с названием '{0}' не содержит первичного ключа.", tableName));
                adapter.Update(dataTable.DataSet);
            }
            finally
            {
                if (_connection != null && !connectionOpened)
                    _connection.Close();
            }
        }
        public void OpenConnection()
        {
            if (_connection == null)
                _connection = GetConnection();

            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();
            }
            catch (Exception e)
            {
                throw new ApplicationException("Ошибка доступа к источнику данных.", e);
            }
        }

        public void CloseConnection()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
                _connection.Close();
        }

        private bool EnsureConnection(ref DbConnection connection)
        {
            if (connection == null)
                connection = GetConnection();

            if (connection.State != ConnectionState.Open)
            {
                try
                {
                    connection.Open();
                }
                catch (Exception e)
                {
                    throw new ApplicationException("Ошибка доступа к источнику данных.", e);
                }
            }
            else
                return true;

            return false;
        }

        public override string ToString()
        {
            return $"Сервер={_server}, Источник={_source}";
        }

        public static object EscapeQuotes(object value)
        {
            if (value is string s)
                return s.Replace("'", "''").Replace("\"", "\"\"");

            return value;
        }

        #endregion
    }
}
