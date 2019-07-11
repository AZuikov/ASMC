using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace AP.Utils.Data
{
    /// <summary>
    /// Представляет контейнер для работы с файлами
    /// универсальной связи с данными (UDL).
    /// </summary>
    public class UdlBuilder
    {
        private static readonly string Default = "[oledb]" + Environment.NewLine +
            "; Everything after this line is an OLE DB initstring" + Environment.NewLine +
            "Provider=SQLOLEDB.1;";

        #region Properties

        /// <summary>
        /// Возвращает имя базы
        /// на источнике данных, с которой
        /// устанавливается соединение.
        /// </summary>
        public string Catalog
        {
            get
            {
                var rex = new Regex("Initial Catalog=([^;]*)", RegexOptions.IgnoreCase);
                var m = rex.Match(ConnectionString);

                return m.Success ? m.Groups[1].ToString() : string.Empty;
            }
        }

        /// <summary>
        /// Возвращает сетевой адрес
        /// экземпляра источника данных, с
        /// которым устанавливается соединение.
        /// </summary>
        public string DataSource
        {
            get
            {
                var rex = new Regex(@"Data Source=([^\r\n;]*)", RegexOptions.IgnoreCase);
                var m = rex.Match(ConnectionString);

                return m.Success ? m.Groups[1].ToString() : string.Empty;
            }
        }

        /// <summary>
        /// Возвращает пароль,
        /// используемый при авторизации
        /// на источнике данных.
        /// </summary>
        public string Password
        {
            get
            {
                var rex = new Regex(@"Password=([^\r\n;]*)", RegexOptions.IgnoreCase);
                var m = rex.Match(ConnectionString);
                if(m.Success)
                {
                    var password = m.Groups[1].ToString();

                    if(password.StartsWith("\""))
                    {
                        rex = new Regex("Password=\"(.*)\"");
                        m = rex.Match(ConnectionString);
                        if(m.Success)
                        {
                            password = m.Groups[1].ToString();
                            password = password.Replace("\"\"", "\"");
                        }
                    }

                    return password;
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Возвращает выбранного поставщика OLE DB.
        /// </summary>
        public string Provider
        {
            get; set;
        }

        /// <summary>
        /// Возвращает имя входа,
        /// используемое при авторизации на
        /// источнике данных.
        /// </summary>
        public string Username
        {
            get
            {
                var rex = new Regex(@"User ID=([^\r\n;]*)", RegexOptions.IgnoreCase);

                var m = rex.Match(ConnectionString);
                if(m.Success)
                {
                    var userId = m.Groups[1].ToString();
                    if(userId.StartsWith("\""))
                    {
                        rex = new Regex("\"(.*)\"");
                        m = rex.Match(userId);
                        if(m.Success)
                            userId = m.Groups[1].ToString().Replace("\"\"", "\"");

                    }

                    return userId;
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Возвращает использование
        /// проверки подлинности Windows при
        /// авторизации на источнике данных.
        /// </summary>
        public bool IntegratedSecurity
        {
            get
            {
                var rex = new Regex(@"Integrated Security=([^\r\n;]*)", RegexOptions.IgnoreCase);
                var m = rex.Match(ConnectionString);

                if(!m.Success)
                    return false;

                var value = m.Groups[1].Value.Trim();
                return value.Equals("sspi", StringComparison.OrdinalIgnoreCase) ||
                       value.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Возвращает значение, которое определяет,
        /// возвращаются ли сведения, связанные с
        /// безопасностью, как часть подключения,
        /// если оно открыто или когда-либо
        /// находилось в открытом состоянии.
        /// </summary>
        public bool PersistSecurityInfo
        {
            get
            {
                var rex = new Regex(@"Persist Security Info=([^\r\n;]*)", RegexOptions.IgnoreCase);
                var m = rex.Match(ConnectionString);

                return m.Success && m.Groups[1].Value.Trim().Equals("true", StringComparison.CurrentCultureIgnoreCase);
            }
        }

        /// <summary>
        /// Возвращает время ожидания
        /// соединения с источником данных.
        /// </summary>
        public int ConnectionTimeout
        {
            get
            {
                var rex = new Regex(@"Connection timeout=([^\r\n;]*)", RegexOptions.IgnoreCase);
                var m = rex.Match(ConnectionString);

                return m.Success ? Convert.ToInt32(m.Groups[1].ToString()) : 1;
            }
        }

        private string ConnectionString
        {
            get; set;
        }

        #endregion

        /// <summary>
        /// Инициализирует новый экземпляр
        /// класса <see cref="UdlBuilder"/>.
        /// </summary>
        public UdlBuilder()
            : this(null)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр
        /// класса <see cref="UdlBuilder"/>.
        /// </summary>
        /// <param name="connectionString">Строка подключения для
        /// инициализации.</param>
        public UdlBuilder(string connectionString)
        {
            ConnectionString = connectionString ?? string.Empty;
        }

        #region Methods

        public void Save(Stream stream)
        {
            using(var sw = new StreamWriter(stream, Encoding.Unicode, 1, true))
                sw.Write(Default + ConnectionString);
        }

        public void SaveFile(string fileName)
        {
            using(var stream = new MemoryStream())
            {
                Save(stream);

                using(var file = File.Create(fileName))
                {
                    stream.Position = 0;
                    stream.CopyTo(file);
                }
            }
        }

        public SqlConnectionStringBuilder ToSqlConnectionStringBuilder()
        {
            var csb = new SqlConnectionStringBuilder
            {
                UserID = Username,
                Password = Password,
                DataSource = DataSource,
                InitialCatalog = Catalog,
                IntegratedSecurity = IntegratedSecurity,
                PersistSecurityInfo = PersistSecurityInfo,
                ConnectTimeout = ConnectionTimeout
            };
            return csb;
        }

        public static void CreateDefaultFile(string fileName)
        {
            using(var stream = new MemoryStream())
            {
                using(var sw = new StreamWriter(stream, Encoding.Unicode))
                {
                    sw.Write(Default);
                    sw.Flush();
                    stream.Position = 0;

                    using(var file = File.Create(fileName))
                        stream.CopyTo(file);
                }
            }
        }

        public static UdlBuilder ParseFile(string fileName)
        {
            if(fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            var str = File.ReadAllText(fileName, Encoding.Unicode);

            var regex = new Regex("(Provider[^;]*);(.*)", RegexOptions.Multiline);
            var udl = new UdlBuilder();

            var match = regex.Match(str);
            if(match.Success)
            {
                udl.Provider = match.Groups[1].ToString();
                udl.ConnectionString = match.Groups[2].ToString();
            }
            return udl;
        }

        #endregion
    }

}
