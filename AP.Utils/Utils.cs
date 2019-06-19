using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace AP.Utils
{
    public static class Utils
    {
        private static string _productId;

        public static string DigitalProductId
        {
            get
            {
                if(_productId == null)
                {
                    var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

                    using(var key = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", false))
                    {
                        var data = (byte[])key.GetValue("DigitalProductId");
                        _productId = Convert.ToBase64String(data);
                    }

                    baseKey.Close();
                }

                return _productId;
            }
        }

        public static string Encrypt(ref string s, string password)
        {
            if(s == null)
                return null;

            using(var des = GetCryptoProvider(ref password))
            {
                using(var stream = new MemoryStream())
                {
                    using(var cryptoStream = new CryptoStream(stream, des.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        var buffer = Encoding.Unicode.GetBytes(s);
                        cryptoStream.Write(buffer, 0, buffer.Length);
                        cryptoStream.FlushFinalBlock();
                        return Convert.ToBase64String(stream.ToArray());
                    }
                }
            }
        }

        public static string Decrypt(string s, string password)
        {
            if(s == null)
                return null;
            if(s.Length == 0)
                return "";

            using(var des = GetCryptoProvider(ref password))
            {
                using(var stream = new MemoryStream(Convert.FromBase64String(s)))
                {
                    using(var cryptoStream = new CryptoStream(stream, des.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        var buffer = new byte[stream.Length];

                        cryptoStream.Read(buffer, 0, buffer.Length);
                        return Encoding.Unicode.GetString(buffer).TrimEnd('\0');
                    }
                }
            }
        }

        /// <summary>
        /// Возвращает уникальную строку для списка
        /// </summary>
        /// <param name="format">Формат строки</param>
        /// <param name="s">Список строк</param>
        /// <param name="ignoreCase">Игнорировать регистр букв</param>
        /// <returns>Строка, являющаяся уникальной для указанного списка</returns>
        public static string GetUniqueString(string format, IEnumerable s, bool ignoreCase = true)
        {
            string str;
            var index = 1;

            var sc = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            var a = s.Cast<object>().ToArray();
            while(true)
            {
                str = string.Format(format, index++);
                if(a.Length == 0 || a.Count(i => string.Compare(i?.ToString(), str, sc) == 0) == 0)
                    break;
            }
            return str;
        }

      

        private static TripleDESCryptoServiceProvider GetCryptoProvider(ref string password)
        {
            var tripleDes = new TripleDESCryptoServiceProvider();
            using (var pdb = new PasswordDeriveBytes(password, Encoding.Unicode.GetBytes(password.Length.ToString())))
            {
                tripleDes.IV = new byte[tripleDes.BlockSize >> 3];
                tripleDes.Key = pdb.CryptDeriveKey("TripleDES", "SHA1", 192, tripleDes.IV);
            }
            return tripleDes;
        }
    }
}
