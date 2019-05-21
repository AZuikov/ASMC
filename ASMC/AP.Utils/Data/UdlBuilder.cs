using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AP.Utils.Data
{
    public class UdlBuilder
    {
        private static readonly string Default = "[oledb]" + Environment.NewLine +
            "; Everything after this line is an OLE DB initstring" + Environment.NewLine +
            "Provider=SQLOLEDB.1;";

        #region Properties

        public string Catalog
        {
            get
            {
                var rex = new Regex("Initial Catalog=([^;]*)", RegexOptions.IgnoreCase);
                var m = rex.Match(ConnectionString);

                return m.Success ? m.Groups[1].ToString() : string.Empty;
            }
        }

        public string DataSource
        {
            get
            {
                var rex = new Regex(@"Data Source=([^\r\n;]*)", RegexOptions.IgnoreCase);
                var m = rex.Match(ConnectionString);

                return m.Success ? m.Groups[1].ToString() : string.Empty;
            }
        }

        public string Password
        {
            get
            {
                var rex = new Regex(@"Password=([^\r\n;]*)", RegexOptions.IgnoreCase);
                var m = rex.Match(ConnectionString);
                if (m.Success)
                {
                    var password = m.Groups[1].ToString();

                    if (password.StartsWith("\""))
                    {
                        rex = new Regex("Password=\"(.*)\"");
                        m = rex.Match(ConnectionString);
                        if (m.Success)
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

        public string Provider { get; set; }

        public string Username
        {
            get
            {
                var rex = new Regex(@"User ID=([^\r\n;]*)", RegexOptions.IgnoreCase);

                var m = rex.Match(ConnectionString);
                if (m.Success)
                {
                    var userId = m.Groups[1].ToString();
                    if (userId.StartsWith("\""))
                    {
                        rex = new Regex("\"(.*)\"");
                        m = rex.Match(userId);
                        if (m.Success)
                            userId = m.Groups[1].ToString().Replace("\"\"", "\"");

                    }

                    return userId;
                }
                return string.Empty;
            }
        }

        public bool IntegratedSecurity
        {
            get
            {
                var rex = new Regex(@"Integrated Security=([^\r\n;]*)", RegexOptions.IgnoreCase);
                var m = rex.Match(ConnectionString);

                return m.Success && m.Groups[1].Value.Trim().Equals("sspi", StringComparison.CurrentCultureIgnoreCase);
            }
        }

        public bool PersistSecurityInfo
        {
            get
            {
                var rex = new Regex(@"Persist Security Info=([^\r\n;]*)", RegexOptions.IgnoreCase);
                var m = rex.Match(ConnectionString);

                return m.Success && m.Groups[1].Value.Trim().Equals("true", StringComparison.CurrentCultureIgnoreCase);
            }
        }

        private string ConnectionString { get; set; }

        #endregion

        #region Methods

        public SqlConnectionStringBuilder ToSqlConnectionStringBuilder()
        {
            var csb = new SqlConnectionStringBuilder
            {
                UserID = Username,
                Password = Password,
                DataSource = DataSource,
                InitialCatalog = Catalog,
                IntegratedSecurity = IntegratedSecurity,
                PersistSecurityInfo = PersistSecurityInfo
            };
            return csb;
        }

        public static Stream CreateDefaultFile()
        {
            var stream = new MemoryStream();
            var sw = new StreamWriter(stream, Encoding.Unicode);
            sw.Write(Default);

            stream.Position = 0;
            return stream;
        }

        public static UdlBuilder ParseFile(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            var str = File.ReadAllText(fileName, Encoding.Unicode);

            var regex = new Regex("(Provider[^;]*);(.*)", RegexOptions.Multiline);
            var udl = new UdlBuilder();

            var match = regex.Match(str);
            if (match.Success)
            {
                udl.Provider = match.Groups[1].ToString();
                udl.ConnectionString = match.Groups[2].ToString();
            }
            return udl;
        }

        #endregion
    }
}
