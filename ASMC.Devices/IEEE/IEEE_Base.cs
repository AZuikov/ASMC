// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ASMC.Data.Model;
using Ivi.Visa;
using MathNet.Numerics.Financial;
using NLog;
using Timer = System.Windows.Forms.Timer;

namespace ASMC.Devices.IEEE
{
    public class IeeeBase : HelpDeviceBase, IProtocolStringLine
    {
        /// <summary>
        /// Звуковой сигнал
        /// </summary>
        private const string _BEEP = "SYST:BEEP";

        /// <summary>
        /// Очистка всех регистров состояния
        /// </summary>
        public const string Clear = "*CLS";

        ///Вызывает конфигурацию опций прибора
        public const string QueryConfig = "*OPT?";

        /// <summary>
        /// Запрос индификации прибора
        /// </summary>
        public const string QueryIdentificationDevice = "*IDN?";

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

        #region Fields

        /// <summary>
        /// отвечает за задержку между командами в мс
        /// </summary>
        protected  int DealySending { get; set; }

        /// <summary>
        /// Массив подключенных устройств
        /// </summary>
        private string[] _devace;

        private DeviceInfo _info;

        private string _stringConnection;

        private Timer _time;

        /// <summary>
        /// Массив информации об устройстве
        /// </summary>
        private string[] _words;

        #endregion

        #region Property

        /// <summary>
        /// Получить список всех устройств.
        /// </summary>
        /// <returns></returns>
        public static string[] AllStringConnect
        {
            //  \HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\USB  чистить реестр тут
            get
            {
                var arr = GlobalResourceManager.Find().ToList();
                var ResultList = new List<string>();

                Parallel.For(0, arr.Count, i =>
                {
                    if (!arr[i].Contains("INTFC"))
                    {
                        var MyReEx = @"(com|lpt)\d+";
                        Match portNameMatch;

                        var MyUsbRegEx = @"USB\d+::0x\d+::0x\d+::\w+::INSTR";
                        Match usbDeviceMatch;

                        var MyGpibRegEx = @"GPIB\d+::\d+::INSTR";
                        Match gpibDeviceMatch;

                        //нужна регулярка для устройств подключенных через LAN

                        try
                        {
                            
                           usbDeviceMatch = Regex.Match(arr[i], MyUsbRegEx, RegexOptions.IgnoreCase);
                           gpibDeviceMatch = Regex.Match(arr[i], MyGpibRegEx, RegexOptions.IgnoreCase);
                           if (usbDeviceMatch.Success || gpibDeviceMatch.Success) ResultList.Add(arr[i]);
                           else
                           {
                                var devObj = GlobalResourceManager.Open(arr[i]);
                                portNameMatch = Regex.Match(devObj.HardwareInterfaceName, MyReEx, RegexOptions.IgnoreCase);
                                if (portNameMatch.Success) ResultList.Add(portNameMatch.Value);
                           }
                        }
                        catch (NativeVisaException e)
                        {
                        }
                    }
                });

                return ResultList.ToArray();
            }
        }

        public DeviceInfo Info
        {
            get
            {
                if (_info != null) return _info;

                var arr = GetBaseInfoFromDevice();
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

        /// <summary>
        /// Объкт сессии
        /// </summary>
        protected IMessageBasedSession Session { get; set; }

        #endregion

        public IeeeBase()
        {
            /*50ms задежка*/
            DealySending = 50;
            Multipliers = new ICommand[]
            {
                new Command("N", "н", 1E-9),
                new Command("N", "н", 1E-9),
                new Command("U", "мк", 1E-6),
                new Command("M", "м", 1E-3),
                new Command("", "", 1),
                new Command("K", "к", 1E3),
                new Command("MA", "М", 1E6),
                new Command("G", "Г", 1E9)
            };
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

        public List<string> GetOption()
        {
            WriteLine(QueryConfig);
            return new List<string>(ReadString().Split(','));
        }

        /// <summary>
        /// Считывает текст до пробела
        /// </summary>
        /// <returns>Возвращает считанаю строку</returns>
        public string ReadString()
        {
            Open();
            string date = null;
            try
            {
                date = Session.FormattedIO.ReadString();
            }
            catch (IOTimeoutException e)
            {
                Logger.Error(e);
            }
            finally
            {
                Close();
            }

            return date;
        }

        /// <summary>
        /// Синхронизация с прибором
        /// </summary>
        public void Sinchronization()
        {
            var timer = new System.Timers.Timer {Interval = 30000};
            timer.Elapsed += Timer_Elapsed;

            void Timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                timer.Elapsed -= Timer_Elapsed;
                timer.Dispose();
                throw new TimeoutException($@"Превышено время ожидания синхронизации в {UserType}");
            }

            // ReSharper disable once EmptyEmbeddedStatement
            while (!QueryLine(QuerySinchronization).Equals("1")) ;
            timer.Elapsed -= Timer_Elapsed;
            timer.Dispose();
        }

        /// <summary>
        /// Самопроверка
        /// </summary>
        /// <returns> True - успешно, иначе провал</returns>
        public bool TestSelf()
        {
            return QueryLine(QueryTestSelf).Equals("1");
        }

        /// <summary>
        /// Отправляет полученную команду в формате vbs 'комантда'
        /// </summary>
        /// <param name = "date">Принимает текст команды в VBS формате</param>
        public void WriteLineVbs(string date)
        {
            Open();
            Session.FormattedIO.WriteLine("vbs '" + date + "'");
            Thread.Sleep(DealySending);
            Close();
        }

        /// <summary>
        /// Метод обращается к прибору и заполняет массив для информации о нём
        /// </summary>
        private string[] GetBaseInfoFromDevice()
        {
            Open();
            Session.FormattedIO.WriteLine(QueryIdentificationDevice);
            var res = Session.FormattedIO.ReadLine().Split(',');
            Close();
            return res;
        }

        private bool Open(bool error)
        {
            try
            {
                Session = (IMessageBasedSession) GlobalResourceManager.Open(StringConnection);
                Session.TimeoutMilliseconds = 100;
                // !!! очистка сессии здесь не нужна. на некоторых машинах она вызывает наслоение команд. В результате часть отправленых команд не выпонятся.
                // !!! Очистка сессии может отменять ранее запущенные операции на устройстве.
            }
            catch (Exception e)
            {
                if (error) Logger.Error(e);
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

        /// <inheritdoc />
        public string UserType { get; protected set; }

        /// <inheritdoc />
        public string StringConnection
        {
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
            if (IsOpen)
            {
                Session?.Dispose();
            }
            IsOpen = false;
        }

        /// <inheritdoc />
        public void Open()
        {
            try
            {
                if (IsOpen) return;
                Session = (IMessageBasedSession) GlobalResourceManager.Open(StringConnection);
                Session.TimeoutMilliseconds = 60000;
                Thread.Sleep(100);
            }
            catch (Exception e)
            {
                Logger.Error(e, $@"Порт не удалось открыть для устроства {UserType}.");
                Session.Clear();
                IsOpen = false;
                throw;
            }

            IsOpen = true;
        }

        /// <inheritdoc />
        public bool IsOpen { get; protected set; }

        /// <summary>
        /// Считывает строку
        /// </summary>
        /// <returns>Возвращает считанаю строку</returns>
        public string ReadLine()
        {
            Open();
            string date = null;
            try
            {
                date = Session.FormattedIO.ReadLine();
            }
            catch (Exception e)
            {
                Logger.Error(e, $@"Не удалось считать данные с устройства {UserType}");
                throw;
            }
            finally
            {
                Close();
            }

            return date;
        }

        /// <summary>
        /// Отправляет полученную команду как строку
        /// </summary>
        /// <param name = "data">Текст комманды.</param>
        public void WriteLine(string data)
        {
            Open();
            try
            {
                Session.FormattedIO.WriteLine(data);
            }
            catch (Exception e)
            {
                Logger.Error(e, $@"Не удалось отправить комманду {data} на устройство {UserType}");
                throw;
            }
            finally
            {
                Close();
            }

            Logger.Debug($"На устройство {UserType} по адресу {StringConnection} отправлена команда {data}");
            Thread.Sleep(DealySending);
        }

        public string QueryLine(string inStrData)
        {
            Open();
            Session.FormattedIO.WriteLine(inStrData);
            Logger.Debug($"На устройство {UserType} отправлена команда {inStrData}");
            
            string answer;
            try
            {
                answer = Session.FormattedIO.ReadLine().TrimEnd('\n');
                Logger.Debug($"Устройство {UserType} ответило {answer}");
            }
            catch (Exception e)
            {
                Logger.Error(e, $@"Считать результат на запрос {inStrData} c устройства {UserType} не удалось.");
                throw;
            }
            finally
            {
                Close();
            }

            if (!answer.Any()) throw new IOException($"Данные с устройства {UserType} считать не удалось.");

            return answer;
        }
    }
}