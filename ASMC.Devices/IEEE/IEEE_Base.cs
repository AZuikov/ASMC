// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Windows.Forms;
using ASMC.Data.Model;
using Ivi.Visa;
using NLog;
using Timer = System.Windows.Forms.Timer;

namespace ASMC.Devices.IEEE
{
    public class IeeeBase : HelpDeviceBase, IDevice
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
        /// Самопроверка 0 - успешно, 1 - пров
        /// ал
        /// </summary>
        public const string QueryTestSelf = "*TST?";

        /// <summary>
        /// Сброс Всех настроек
        /// </summary>
        public const string Reset = "*RST";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private DeviceInfo _info;
        public DeviceInfo Info
        {
            get
            {
                if (_info != null) return _info;

                  var arr  =GetBaseInfoFromDevice();
                string manufacturer = null, serial = null, type = null, fwv = null;
                try
                {
                     manufacturer = arr[0];
                    type = arr[1];
                    serial = arr[2];
                    fwv = arr[3];
                }
                catch (IndexOutOfRangeException e)
                {
                    Logger.Info(e);
                }

                _info = new DeviceInfo(fwv, manufacturer, serial, type);
                return _info;
            }
        }
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
        /// <inheritdoc />
        public string UserType { get; protected set; }

        /// <inheritdoc /> 
        public string StringConnection {
            get => _stringConnection;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _stringConnection = null;
                    return;
                }
                   
                if (value.StartsWith("com", true, CultureInfo.InvariantCulture))
                {
                    var replace = value.ToLower().Replace("com", "ASRL") + "::INSTR";
                    _stringConnection = replace;
                    return;
                }
                _stringConnection = value;
            } }

        /// <summary>
        /// Объкт сессии
        /// </summary>
        protected IMessageBasedSession Session { get; set; }

        private string _stringConnection;

        #endregion

        public IeeeBase()
        {     
            /*50ms задежка*/
            _dealySending = 50;
            Multipliers = new ICommand[]{new Command("N","н", 1E-9),
                new Command("N", "н", 1E-9),
                new Command("U", "мк", 1E-6),
                new Command("M", "м", 1E-3),
                new Command("", "", 1),
                new Command("K", "к", 1E3),
                new Command("MA", "М", 1E6),
                new Command("G", "Г", 1E9)};
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
                Close();
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
            Close();
        }

        /// <inheritdoc />
        public void Close()
        {
            //Session.Clear();
            Session?.Dispose();
        }

        /// <inheritdoc />
        public bool Open()
        {
            
            try
            {
                Session = (IMessageBasedSession) GlobalResourceManager.Open(StringConnection);
                Session.TimeoutMilliseconds = 60000;
                //Session.Clear();
                Thread.Sleep(100);

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
        private string[] GetBaseInfoFromDevice()
        {
            
            if(!Open()) {return null;}
            Session.FormattedIO.WriteLine(QueryIdentificationDevice);
            var res = Session.FormattedIO.ReadLine().Split(',');
             Close();
            return res;

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
                            if (string.CompareOrdinal(Info.Type, UserType) == 0)
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
            Logger.Warn($@"Устройство {UserType} не найдено");
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
                                if (string.CompareOrdinal(Info.Type, deviseType) == 0) return true;
                            }
                            else
                            {
                                foreach (var t in _words)
                                    if (t.Contains(deviseType))
                                        return true;

                                _words = null;
                                connect = null;
                            }

                        Close();
                    }
                    
                }
            Logger.Warn($@"Устройство {UserType} не найдено");
            return false;
        }

        /// <summary>
        /// Получить список всех устройств.
        /// </summary>
        /// <returns></returns>
        public static string[] AllStringConnect
        {
            get
            {
                var arr = GlobalResourceManager.Find().ToList();


                for (var i = 0; i < arr.Count; i++)
                {
                    if (arr[i].Contains("INTFC"))
                    {
                        arr.RemoveAt(i);
                        continue;
                    }

                    if (arr[i].StartsWith("ASRL", true, CultureInfo.InvariantCulture))
                    {
                        arr[i] = "COM" + arr[i].ToUpper().Replace("ASRL", "").Replace("::INSTR", "");
                    }
                }
                //{
                   
                //    if (!arr[i].StartsWith("ASRL", true, CultureInfo.InvariantCulture)) continue;

                //    var replace = "COM" + arr[i].ToUpper().Replace("ASRL", "").Replace("::INSTR", "");
                //    arr[i] = replace;
                //} 
                return arr.ToArray();
            }
        }

          
        public List<string> GetOption()
        {
            WriteLine(QueryConfig);
            return new List<string>(ReadString().Split(','));
        }
        /// <summary>
        /// Считывает строку
        /// </summary>
        /// <returns>Возвращает считанаю строку</returns>
        public string ReadLine()
        {
            if(!Open()) return null;
            string date = null;
            try
            {
                date = Session.FormattedIO.ReadLine();
            }
            catch(TimeoutException e)
            {
                Logger.Error(e);
            }
             Close();
            return date;
        }
        /// <summary>
        /// Считывает текст до пробела
        /// </summary>
        /// <returns>Возвращает считанаю строку</returns>
        public string ReadString()
        {
            if (!Open()) return null;
            string date = null;
            try
            {
                date = Session.FormattedIO.ReadString();
                
            }
            catch (TimeoutException e)
            {  
              Logger.Error(e);  
            } 
            Close();
            return date;
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
        /// <param name = "str">Принимает текст команды в VBS формате</param>
        public void WriteLine(string data )
        {
            
            if(!Open()) return;
            Session.FormattedIO.WriteLine(data);
            Logger.Debug($"На устройство {UserType} по адресу {StringConnection} отправлена команда {data}");
            Thread.Sleep(_dealySending);
            Close();
        }

        public string QueryLine(string inStrData)
        {
            if (!Open())
            {
                Logger.Warn($@"Запись в устройство {Session.ResourceName} данных: {inStrData} не выполнена");
                throw new IOException($"Не удалось получить доступ к утсройству {Session.ResourceName}");

            }

            Session.FormattedIO.WriteLine(inStrData);
            Thread.Sleep(200);

            string answer = Session.FormattedIO.ReadLine().TrimEnd('\n');
            Close();

            if (answer.Length == 0) throw new IOException($"Данные с устройства {Session.ResourceName} считать не удалось.");

            return answer;
        }

        /// <summary>
        /// Отправляет полученную команду в формате vbs 'комантда'
        /// </summary>
        /// <param name = "date">Принимает текст команды в VBS формате</param>
        public void WriteLineVbs(string date)
        {
            if(!Open()) return;
            Session.FormattedIO.WriteLine("vbs '" + date + "'");
            Thread.Sleep(_dealySending);
            Close();
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