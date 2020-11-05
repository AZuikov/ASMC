// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using ASMC.Data.Model;
using NLog;
using System;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace ASMC.Devices.Port
{
    public class ComPort : HelpDeviceBase, IProtocolStringLine
    {
        /// <summary>
        /// Предоставлет перечесление возможного размера данных.
        /// </summary>
        public enum DBit
        {
            Bit4 = 4,
            Bit5 = 5,
            Bit6 = 6,
            Bit7 = 7,
            Bit8 = 8
        }

        /// <summary>
        /// Предоставляет перечисление возможных скоростей.
        /// </summary>
        public enum SpeedRate
        {
            R75 = 75,
            R110 = 110,
            R134 = 134,
            R150 = 150,
            R300 = 300,
            R600 = 600,
            R1200 = 1200,
            R1800 = 1800,
            R2400 = 2400,
            R4800 = 4800,
            R7200 = 7200,
            R9600 = 9600,
            R14400 = 14400,
            R19200 = 19200,
            R28800 = 28800,
            R38400 = 38400,
            R57600 = 57600,
            R115200 = 115200,
            R128000 = 128000
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        private readonly SerialPort _sp;

        #endregion Fields

        #region Property

        /// <summary>
        /// Поддержка сигнала готовности терминала DTR.
        /// </summary>
        public bool IsDtrOn
        {
            get => _sp.DtrEnable;
            set => _sp.DtrEnable = value;
        }

        /// <summary>
        /// Сигнал запроса передачи RTS.
        /// </summary>
        public bool IsRTS
        {
            get => _sp.RtsEnable;
            set => _sp.RtsEnable = value;
        }

        /// <summary>
        /// Состояние линии готовности к приёму.
        /// </summary>
        public bool IsCTS
        {
            get => _sp.CtsHolding;
        }

        public SpeedRate BaudRate
        {
            get => (SpeedRate)_sp.BaudRate;
            set => _sp.BaudRate = (int)value;
        }

        // public DTRMode

        public DBit DataBit
        {
            get => (DBit)_sp.DataBits;
            set => _sp.DataBits = (int)value;
        }

        /// <summary>
        /// Позволяет задать или получить терменал окончания строки.
        /// </summary>
        public string EndLineTerm
        {
            set => _sp.NewLine = value;
            get => _sp.NewLine;
        }

        public Parity Parity
        {
            get => _sp.Parity;
            set => _sp.Parity = value;
        }

        /// <summary>
        /// Установка таймаута, изначально выставлено 500 мс
        /// </summary>
        /// <value>
        /// Значение в милисекундах
        /// </value>
        public int SetTimeout
        {
            set
            {
                _sp.WriteTimeout = value;
                _sp.ReadTimeout = value;
            }
        }

        public StopBits StopBit
        {
            get => _sp.StopBits;
            set => _sp.StopBits = value;
        }

        #endregion Property

        public ComPort()
        {
            _sp = new SerialPort();
        }

        public ComPort(string portName)
        {
            _sp = new SerialPort(portName, (int)SpeedRate.R9600, Parity.None, (int)DBit.Bit8, StopBits.One);
        }

        public ComPort(string portName, SpeedRate bautRate)
        {
            _sp = new SerialPort(portName, (int)bautRate, Parity.None, (int)DBit.Bit8, StopBits.One);
        }

        public ComPort(string portName, SpeedRate bautRate, Parity parity)
        {
            _sp = new SerialPort(portName, (int)bautRate, parity, (int)DBit.Bit8, StopBits.One);
        }

        public ComPort(string portName, SpeedRate bautRate, Parity parity, DBit databit)
        {
            _sp = new SerialPort(portName, (int)bautRate, parity, (int)databit, StopBits.One);
        }

        public ComPort(string portName, SpeedRate bautRate, Parity parity, DBit databit, StopBits stopbits)
        {
            _sp = new SerialPort(portName, (int)bautRate, parity, (int)databit, stopbits);
        }

        #region Methods

        public static string[] GetPortName()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// Записывает в порт строку.
        /// </summary>
        /// <param name = "data"></param>
        public void Write(string data)
        {
            if (!IsOpen)
            {
                Logger.Warn($@"Запись в порт {_sp.PortName}данных:{data} не выполнена");
                return;
            }

            _sp.Write(data);
        }

        protected void DiscardInBuffer()
        {
            if (_sp.IsOpen)
                _sp.DiscardInBuffer();
        }

        protected virtual void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
        }

        public void Write(byte[] sendData, int offset, int length)
        {
            if (!IsOpen)
            {
                Logger.Warn($@"Запись в порт {_sp.PortName}данных:{sendData} не выполнена");
                return;
            }

            _sp.Write(sendData, offset, length);
        }

        #endregion Methods

        /// <summary>
        /// Позволяет получать имя устройства.
        /// </summary>
        public string UserType { get; protected set; }

        public string StringConnection
        {
            get => _sp.PortName;
            set
            {
                if (value == null)
                {
                    _sp.PortName = null;
                    return;
                }

                if (value.StartsWith("ASRL", true, CultureInfo.InvariantCulture))
                {
                    var replace = "COM" + value.ToUpper().Replace("ASRL", "").Replace("::INSTR", "");
                    _sp.PortName = replace;
                    return;
                }

                _sp.PortName = value;
            }
        }

        /// <summary>
        /// Отвечает открыт ли уже порт или нет
        /// </summary>
        /// <returns></returns>
        public bool IsOpen => _sp.IsOpen;

        /// <inheritdoc />
        public virtual bool IsTestConnect => false;

        /// <summary>
        /// Открывает соединение с Com портом.
        /// </summary>
        /// <returns>Возвращает True, если порт открыт, иначе False</returns>
        public void Open()
        {
            if (IsOpen) return;

            try
            {
                _sp.Open();
                _sp.DataReceived += SerialPort_DataReceived;
                Logger.Debug($"Открыли порт {_sp.PortName} и подписались на событие считывания");
            }
            catch (UnauthorizedAccessException e)
            {
                Logger.Error($"Попытка открыть порт: {e}");
            }
        }

        public void Close()
        {
            if (!_sp.IsOpen) return;
            try
            {
                _sp.DataReceived -= SerialPort_DataReceived;
                _sp.Close();
                DiscardInBuffer();
                _sp.Dispose();
                Logger.Debug($"Последовательный порт {_sp.PortName} закрыт и отписались от события считывания.");
            }
            catch (IOException e)
            {
                Logger.Error(e);
            }
        }

        public int ReadByte()
        {
            if (!IsOpen) return 0;
            try
            {
                return _sp.ReadByte();
            }
            catch (TimeoutException e)
            {
                Logger.Error(e);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            finally
            {
                Close();
            }

            return 0;
        }

        /// <summary>
        /// Считывает байты.
        /// </summary>
        /// <param name="buffer">Массив данных (буфер) куда считывать байты.</param>
        /// <param name="offset">Смещение - с какого элемента буфера начать запись.</param>
        /// <param name="count">Число байт для считывания.</param>
        /// <param name="closePort">Закрывать порт после считывания (еси true закрывает порт).</param>
        /// <returns></returns>
        public int ReadByte(byte[] buffer, int offset, int count, bool closePort = true)
        {
            if (!IsOpen) return 0;
            try
            {
                return _sp.Read(buffer, offset, count);
            }
            catch (TimeoutException e)
            {
                Logger.Error(e);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            finally
            {
                if (closePort) Close();
            }

            return 0;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return ReadByte(buffer, offset, count, false);
        }

        /// <summary>
        /// Считывает строку оканчивающуюся терминальнм символом из ComPort.
        /// </summary>
        /// <returns>Возвращает рузультат чтения</returns>
        public string ReadLine()
        {
            if (!IsOpen) Open();
            try
            {
                return _sp.ReadLine();
            }
            catch (TimeoutException e)
            {
                Logger.Error(e);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            finally
            {
                Close();
            }

            return null;
        }

        /// <summary>
        /// Записывает строку оканчивающуюся терминальнм символом в ComPort.
        /// </summary>
        /// <returns>Возвращает рузультат чтения</returns>
        public void WriteLine(string data)
        {
            if (!IsOpen)
                Open();
            //Logger.Warn($@"Запись в порт {_sp.PortName}данных:{data} не выполнена");
            //return;
            _sp.WriteLine(data);
            Logger.Debug($"На устройство {UserType} по адресу {StringConnection} отправлена команда {data}");
            Close();
        }

        public string QueryLine(string inStrData)
        {
            if (!IsOpen)
                Open();
            //Logger.Warn($@"Запись в порт {_sp.PortName} данных: {inStrData} не выполнена");
            //throw new IOException($"Последовательный порт {_sp.PortName} не удалось открыть.");

            _sp.WriteLine(inStrData);
            Thread.Sleep(200);

            var answer = _sp.ReadLine();
            Close();

            if (answer.Length == 0)
                throw new IOException($"Данные с поседовательного порта {_sp.PortName} считать не удалось.");

            return answer;
        }

        public virtual void Dispose()
        {
            _sp.DataReceived -= SerialPort_DataReceived;
            _sp?.Dispose();
        }
    }
}