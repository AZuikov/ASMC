// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Ivi.Visa;

namespace ASMC.Devises.IEEE
{
    public class Main
    {

        public Main()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            words = null;
            session = null;
            Stringconection = null;
            /*50ms задежка*/
            DealySending = 50;
        }
        ///Вызывает конфигурацию опций прибора
        public const string QueryConfig = "*OPT?";
        /// <summary>
        /// Запрос на наличие не завершкенных операций
        /// </summary>
        public const string QuerySinchronization = "*OPC?";
        /// <summary>
        /// Очистка всех регистров состояния
        /// </summary>
        public const string Clear = "*CLS";
        /// <summary>
        /// Сброс Всех настроек
        /// </summary>
        public const string RESET = "*RST";
        /// <summary>
        /// Самопроверка 0 - успешно, 1 - провал
        /// </summary>
        public const string QueryTestSelf = "*TST?";
        /// <summary>
        /// Самопроверка
        /// </summary>
        /// <returns> 1 - успешно, 0 - провал</returns>
        public bool TestSelf()
        {
            WriteLine(QueryTestSelf);
            if (int.TryParse(ReadString(), out int result))
            {
                if (result == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        private System.Windows.Forms.Timer Time;
        /// <summary>
        /// Beeps the start.
        /// </summary>
        /// <param name="state">True запуск, False стоп</param>
        public void BEEP(bool state, int delay_beep = 2000)
        {
            if (state)
            {
                Time = new System.Windows.Forms.Timer()
                {
                    Interval = delay_beep
                };
                Connection();
                WriteLine(_BEEP);
                Time.Tick += new EventHandler(Time_Tick);
                Time.Start();
            }
            else
            {
                Time.Stop();
                Close();
            }
        }
        private void Time_Tick(object sender, EventArgs e)
        {
            WriteLine(_BEEP);
        }
        /// <summary>
        /// Звуковой сигнал
        /// </summary>
        private const string _BEEP = "SYST:BEEP";
        /// <summary>
        /// Запрос индификации прибора
        /// </summary>
        public const string IdentificationDevice = "*IDN?";
        /// <summary>
        /// Строка подкючения
        /// </summary>
        public string Stringconection;
        /// <summary>
        /// Объкт сессии
        /// </summary>
        protected IMessageBasedSession session;
        /// <summary>
        /// Массив информации об устройстве
        /// </summary>
        private string[] words;
        /// <summary>
        /// Массив подключенных устройств
        /// </summary>
        private string[] devace;
        protected string DeviseType;
        /// <summary>
        /// отвечает за задержку между командами в мс
        /// </summary>
        private int DealySending;
        /// <summary>
        /// Синхронизация с прибором
        /// </summary>
        public void Sinchronization()
        {
            WriteLine(QuerySinchronization);
            string val="";
            do
            {
                try
                {
                    val = ReadString();
                    break;
                }
                catch (TimeoutException)
                {

                }
            } while (val != "1");
        }
        public List<string> GetOption()
        {
            WriteLine(QueryConfig);
             return new List<string>(ReadString().Split(','));

        }
        /// <summary>
        /// Получить список всех устройств
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllDevace()
        {
            List<string> arr = new List<string>();
            arr = (List<string>)GlobalResourceManager.Find();
            return arr;
        }
        /// <summary>
        /// Опрашивает все подключеннные устройства и находит необходимый(по типу прибора), установкивая строку подключения
        /// </summary>        
        /// <returns>Если устройство с указаным типом было найдено возвращает True</returns>
        public bool Devace()
        {
            words = null;
            IEnumerable<string> AllDevace = GlobalResourceManager.Find();
            var OnDevace = AllDevace.GetEnumerator();
            devace = new string[AllDevace.Count()];
            for (int i = 0; i < AllDevace.Count(); i++)
            {
                OnDevace.MoveNext();
                devace[i] = OnDevace.Current;
                Stringconection = OnDevace.Current;
                if (!Connection(false))
                {
                    if (words != null)
                    {
                        if (words.Length>2)
                        {
                            if (string.Compare(GetDeviceType(), DeviseType) == 0)
                            {
                                return true;
                            }
                        }                       
                        else
                        {
                            words = null;
                            Stringconection = null;
                        }
                    }
                    Close();
                }
            }
            MessageBox.Show("Устройство " + DeviseType + " не найдено");
            return false;

        }
        public bool Devace(string DeviseType, out string connect)
        {
            connect = null;
            words = null;
            IEnumerable<string> AllDevace = GlobalResourceManager.Find();
            var OnDevace = AllDevace.GetEnumerator();
            devace = new string[AllDevace.Count()];
            if (DeviseType!="")
            {
                for (int i = 0; i < AllDevace.Count(); i++)
                {
                    OnDevace.MoveNext();
                    devace[i] = OnDevace.Current;
                    connect = OnDevace.Current;
                    Stringconection = OnDevace.Current;
                    if (!Connection(false))
                    {
                        if (words != null)
                        {
                            if (words.Length > 2)
                            {
                                if (string.Compare(GetDeviceType(), DeviseType) == 0)
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                for (int All_i = 0; All_i < words.Length; All_i++)
                                {
                                    if (words[All_i].Contains(DeviseType))
                                    {
                                        return true;
                                    }
                                }
                                words = null;
                                connect = null;
                            }
                        }
                        Close();
                    }
                }
            }      
            MessageBox.Show("Устройство " + DeviseType + " не найдено");
            return false;
        }
        public void SetStringconection(string conection)
        {
            Stringconection = conection;
        }
        /// <summary>
        /// Соеденение по уканой строке подключения
        /// </summary>
        /// <returns>Возвращает True в случае если соединение установлено</returns>
        public bool Connection()
        {
            //ConnectionClosed();
            try
            {
                session = (IMessageBasedSession)GlobalResourceManager.Open(Stringconection);
                session.TimeoutMilliseconds = 60000;
                session.Clear();
            }
            catch (Exception)
            {
                MessageBox.Show("Соединение c " + DeviseType + " не устаеновленно");

                return false;
            }

            return true;
        }
        private bool Connection(bool error)
        {
            try
            {
                session = (IMessageBasedSession)GlobalResourceManager.Open(Stringconection);
                session.TimeoutMilliseconds = 100;
                session.Clear();
            }
            catch (Exception)
            {
                if (error)
                {
                    MessageBox.Show(@"Соединение не устаеновленно");
                }
                return true;
            }
            try
            {

                session.FormattedIO.WriteLine("*IDN?");
                if (Stringconection.Contains("ASRL"))
                {
                    words =new string[] { session.FormattedIO.ReadLine() } ;
                }
                else
                {
                    words = session.FormattedIO.ReadLine().Split(new char[] { ',' });
                    if (words.Length < 2)
                    {
                        words = null;
                    }
                }
               
            }
            catch (Exception)
            {

            }
            return false;
        }
        /// <summary>
        /// Возвращает объект сесии
        /// </summary>
        /// <returns>Возвращет объект сессии</returns>
        public IMessageBasedSession GetSession()
        {
            return session;
        }
        /// <summary>
        /// Позволяет получить зоводской номер устройства от текущей сессии
        /// </summary>
        /// <returns>Возвращает заводской номер устройства</returns>
        public string GetDeviceNumber()
        {
            return words[2];
        }
        /// <summary>
        /// Позволяет получить тип устройства от текущей сессии
        /// </summary>
        /// <returns>Возвращает тип устройства</returns>
        public string GetDeviceType()
        {
            return words[1];
        }
        /// <summary>
        /// Позволяет получить фирму-производитель устройства от текущей сессии
        /// </summary>
        /// <returns>Возвращает фирму-производитель устройства</returns>
        public string GetDeviceFirm()
        {
            return words[0];
        }
        /// <summary>
        /// Позволяет получить идентификационный номер ПО
        /// </summary>
        /// <returns>Возвращает идентификационный номер ПО устройства</returns>
        public string GetDevicePO()
        {
            string[] arr = words[3].Split(new char[] { '-' });
            return arr[0];
        }
        /// <summary>
        /// Закрывает соединение
        /// </summary>
        public void Close()
        {
            if (session!=null)
            {
                session.Dispose();
            }
        }
        /// <summary>
        /// Отправляет полученную команду, без изменений
        /// </summary>
        /// <param string="date">Принимает текст команды</param>
        public void WriteLine(string date)
        {
            session = GetSession();
            session.FormattedIO.WriteLine(date);
            Thread.Sleep(DealySending);
        }
        /// <summary>
        /// Считывает текст до пробела
        /// </summary>
        /// <returns>Возвращает считанаю строку</returns>
        public string ReadString()
        {
            string date;
            session = GetSession();
            date = session.FormattedIO.ReadString();
            return date;
        }
        /// <summary>
        /// Считывает строку
        /// </summary>
        /// <returns>Возвращает считанаю строку</returns>
        public string ReadLine()
        {
            string date;
            session = GetSession();
            date = session.FormattedIO.ReadLine();
            return date;
        }
        /// <summary>
        /// Отправляет полученную команду в формате vbs 'комантда'
        /// </summary>
        /// <param string="date">Принимает текст команды в VBS формате</param>
        public void WriteLineVBS(string date)
        {
            session = GetSession();
            session.FormattedIO.WriteLine("vbs '" + date + "'");
            Thread.Sleep(DealySending);
        }
    }
}
