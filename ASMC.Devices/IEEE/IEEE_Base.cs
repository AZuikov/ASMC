// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Ivi.Visa;
using NLog;
using Timer = System.Windows.Forms.Timer;

namespace ASMC.Devices.IEEE
{
    public class IeeeBase : IDisposable
    {
        /// <summary>
        /// Звуковой сигнал
        /// </summary>
        private const string _BEEP = "SYST:BEEP";

        /// <summary>
        /// Очистка всех регистров состояния
        /// </summary>
        public const string Clear = "*CLS";

        /// <summary>
        /// Запрос индификации прибора
        /// </summary>
        public const string QueryIdentificationDevice = "*IDN?";

        ///Вызывает конфигурацию опций прибора
        public const string QueryConfig = "*OPT?";

        /// <summary>
        /// Запрос на наличие не завершкенных операций
        /// </summary>
        public const string QuerySinchronization = "*OPC?";

        /// <summary>
        /// Самопроверка 0 - успешно, 1 - провал
        /// </summary>
        public const string QueryTestSelf = "*TST?";

        /// <summary>
        /// Сброс Всех настроек
        /// </summary>
        public const string Reset = "*RST";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region  Fields

        /// <summary>
        /// Массив подключенных устройств
        /// </summary>
        private string[] _devace;

        private Timer _time;

        /// <summary>
        /// Массив информации об устройстве
        /// </summary>
        private string[] _words;

        /// <summary>
        /// отвечает за задержку между командами в мс
        /// </summary>
        private readonly int _dealySending;

        protected string DeviseType;

        /// <summary>
        /// Объкт сессии
        /// </summary>
        protected IMessageBasedSession Session;

        /// <summary>
        /// Строка подкючения
        /// </summary>
        public string StringConnection;

        #endregion

        public IeeeBase()
        {   
            _words = null;
            Session = null;
            StringConnection = null;
            /*50ms задежка*/
            _dealySending = 50;
        }

        #region Methods

        /// <summary>
        /// Beeps the start.
        /// </summary>
        /// <param name = "state">True запуск, False стоп</param>
        /// <param name = "delayBeep"></param>
        public void Beep(bool state, int delayBeep = 2000)
        {
            if (state)
            {
                _time = new Timer
                {
                    Interval = delayBeep
                };
                Open();
                WriteLine(_BEEP);
                _time.Tick += (sender, args) => { WriteLine(_BEEP); };
                _time.Start();
            }
            else
            {
                _time.Stop();
                Close();
            }
        }

        public void Dispose()
        {
          _time?.Dispose();
            Session?.Dispose();
        }

        /// <summary>
        /// Закрывает соединение
        /// </summary>
        public void Close()
        {
            Session.Clear();
            Session?.Dispose();
        }

        /// <summary>
        /// Соеденение по уканой строке подключения
        /// </summary>
        /// <returns>Возвращает True в случае если соединение установлено</returns>
        public bool Open()
        {
            //ConnectionClosed();
            try
            {
                Session = (IMessageBasedSession) GlobalResourceManager.Open(StringConnection);
                Session.TimeoutMilliseconds = 60000;
                Session.Clear();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                Session.Clear();
                return false;
            }


            return true;
        }


        /// <summary>
        /// Метод обращается к прибору и заполняет массив для информации о нём
        /// </summary>
        private void GetBaseInfoFromDevice()
        {
            Session.FormattedIO.WriteLine(QueryIdentificationDevice);
            
            if ((Session.HardwareInterfaceType != HardwareInterfaceType.Custom) || (Session.HardwareInterfaceType == HardwareInterfaceType.Serial))
            {
                _words = new[] { Session.FormattedIO.ReadLine() };
            }
            else
            {
                _words = Session.FormattedIO.ReadLine().Split(',');
                if (_words.Length < 2) _words = null;
            }
            
        }


        /// <summary>
        /// Опрашивает все подключеннные устройства и находит необходимый(по типу прибора), устанавливая строку подключения
        /// </summary>
        /// <returns>Если устройство с указаным типом было найдено возвращает True. В противном случае False</returns>
        public bool Devace()
        {
            _words = null;
            var allDevace = GlobalResourceManager.Find();
            if (allDevace != null)
            {
                var onDevace = allDevace.GetEnumerator();
                _devace = new string[allDevace.Count()];
                for (var i = 0; i < allDevace.Count(); i++)
                {
                    onDevace.MoveNext();
                    _devace[i] = onDevace.Current;
                    StringConnection = _devace[i];
                    if (!Open(true))
                    {
                        //если прибор не подключился, т.е. не знает команд scpi, тогда на запрос *idn? он ничего не ответит
                        //в этом случае мы должны его отпустить
                        Session.Dispose();
                        continue;
                    }

                    if (_words != null)
                        if (_words.Length > 2)
                        {
                            if (string.CompareOrdinal(GetDeviceType, DeviseType) == 0)
                            {
                                Close();
                                return true;
                            }
                        }
                        else
                        {
                            _words = null;
                            StringConnection = null;
                        }

                    Close();
                }
            }
            Logger.Warn($@"Устройство {DeviseType} не найдено");
            return false;
        }

        public bool Devace(string deviseType, out string connect)
        {
            connect = null;
            _words = null;
            var allDevace = GlobalResourceManager.Find();
            var onDevace = allDevace.GetEnumerator();
            _devace = new string[allDevace.Count()];
            if (deviseType != "")
                for (var i = 0; i < allDevace.Count(); i++)
                {
                    onDevace.MoveNext();
                    _devace[i] = onDevace.Current;
                    connect = onDevace.Current;
                    StringConnection = onDevace.Current;
                    if (!Open(false))
                    {
                        if (_words != null)
                            if (_words.Length > 2)
                            {
                                if (string.Compare(GetDeviceType, deviseType) == 0) return true;
                            }
                            else
                            {
                                for (var allI = 0; allI < _words.Length; allI++)
                                    if (_words[allI].Contains(deviseType))
                                        return true;
                                _words = null;
                                connect = null;
                            }

                        Close();
                    }
                    
                }
            Logger.Warn($@"Устройство {DeviseType} не найдено");
            return false;
        }

        /// <summary>
        /// Получить список всех устройств
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllDevace
        {
            get { var arr = new List<string>();
            arr = (List<string>) GlobalResourceManager.Find();
            return arr;}
            
        }

        /// <summary>
        /// Позволяет получить фирму-производитель устройства от текущей сессии
        /// </summary>
        /// <returns>Возвращает фирму-производитель устройства</returns>
        public string GetDeviceFirm
        {
            get{
                if (_words == null) GetBaseInfoFromDevice();
                if(_words != null) return _words[0];
                return "NO TYPE";
               }
            
        }

        /// <summary>
        /// Позволяет получить зоводской номер устройства от текущей сессии
        /// </summary>
        /// <returns>Возвращает заводской номер устройства</returns>
        public string GetDeviceSerialNumber
        {
            get
            {
                if (_words == null) GetBaseInfoFromDevice();
                if (_words != null) return _words[2];
                return "NO Device Serial Number";
            }
            
        }

        /// <summary>
        /// Позволяет получить идентификационный номер ПО
        /// </summary>
        /// <returns>Возвращает идентификационный номер ПО устройства</returns>
        public string GetDeviceFirmwareVersion
        {
            get
            {
                if (_words == null) GetBaseInfoFromDevice();
                if (_words != null)
                {
                    var arr = _words[3].Split('-');
                    return arr[0];
                }

                return "NO Firmware Version";
            }
           
        }

        /// <summary>
        /// Позволяет получить тип устройства от текущей сессии
        /// </summary>
        /// <returns>Возвращает тип устройства</returns>
        public string GetDeviceType
        {
            get
            {
                if (_words == null) GetBaseInfoFromDevice();
                if (_words != null) return _words[2];
                return "NO Device Type";
            }
            
        }

        public List<string> GetOption()
        {
            
                WriteLine(QueryConfig);
            return new List<string>(ReadString().Split(','));
           
            
        }

        /// <summary>
        /// Возвращает объект сесии
        /// </summary>
        /// <returns>Возвращет объект сессии</returns>
        public IMessageBasedSession GetSession()
        {
            return Session;
        }

        /// <summary>
        /// Считывает строку
        /// </summary>
        /// <returns>Возвращает считанаю строку</returns>
        public string ReadLine()
        {
            Session = GetSession();
            var date = Session.FormattedIO.ReadLine();
            return date;
        }

        /// <summary>
        /// Считывает текст до пробела
        /// </summary>
        /// <returns>Возвращает считанаю строку</returns>
        public string ReadString()
        {
            Session = GetSession();
            var date = Session.FormattedIO.ReadString();
            return date;
        }

        public void SetStringconection(string conection)
        {
            StringConnection = conection;
        }

        /// <summary>
        /// Синхронизация с прибором
        /// </summary>
        public void Sinchronization()
        {
            WriteLine(QuerySinchronization);
            var val = "";
            do
            {
                try
                {
                    val = ReadString();
                    break;
                }
                catch (TimeoutException e)
                {
                    Logger.Error(e);
                }
            } while (val != "1");
        }

        /// <summary>
        /// Самопроверка
        /// </summary>
        /// <returns> 1 - успешно, 0 - провал</returns>
        public bool TestSelf()
        {
            WriteLine(QueryTestSelf);
            if (int.TryParse(ReadString(), out var result))
                if (result == 0)
                    return true;
                else
                    return false;
            return false;
        }

        /// <summary>
        /// Отправляет полученную команду, без изменений
        /// </summary>
        /// <param name = "date">Принимает текст команды в VBS формате</param>
        public void WriteLine(string date)
        {
            Session = GetSession();
            Session.FormattedIO.WriteLine(date);
            Thread.Sleep(_dealySending);
        }

        /// <summary>
        /// Отправляет полученную команду в формате vbs 'комантда'
        /// </summary>
        /// <param name = "date">Принимает текст команды в VBS формате</param>
        public void WriteLineVbs(string date)
        {
            Session = GetSession();
            Session.FormattedIO.WriteLine("vbs '" + date + "'");
            Thread.Sleep(_dealySending);
        }

        private bool Open(bool error)
        {
            try
            {
                Session = (IMessageBasedSession) GlobalResourceManager.Open(StringConnection);
                Session.TimeoutMilliseconds = 100;
                Session.Clear();
            }
            catch (Exception e)
            {
                if(error) Logger.Error(e);
                return false;
            }

            try
            {
                Session.FormattedIO.WriteLine("*IDN?");
               
                if (Session.HardwareInterfaceType == HardwareInterfaceType.Serial)
                {
                    _words = new[] {Session.FormattedIO.ReadLine()};
                }
                else
                {
                    _words = Session.FormattedIO.ReadLine().Split(',');
                    if (_words.Length < 2) _words = null;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                Session.Clear();
                return false;
            }
            

            return true;
        }

        #endregion
    }
}