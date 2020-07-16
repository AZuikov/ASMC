// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.IO;
using System.IO.Ports;
using NLog;

namespace ASMC.Devices.Port
{
    public class ComPort : IDisposable
    {
        /// <summary>
        /// Позволяет получать имя устройства.
        /// </summary>
        public string DeviceType { get; protected set; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly SerialPort _sp;
        protected virtual void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            

        }
        public string PortName
        { get => _sp.PortName;
            set => _sp.PortName = value;
        }
        public SpeedRate BaudRate
        {
            get => (SpeedRate) _sp.BaudRate;
            set => _sp.BaudRate=(int)value;
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
        public int SetTimeout {
            set
            {
                _sp.WriteTimeout = value;
                _sp.ReadTimeout = value;
            }
        }
        public DBit DataBit
        {
            get => (DBit) _sp.DataBits;
            set => _sp.DataBits = (int) value;
        }
        public ComPort()
        {
            _sp= new SerialPort();
          
        }
        public ComPort(string portName)
        {
            _sp = new SerialPort(portName, (int)SpeedRate.R9600,Parity.None, (int)DBit.Bit8, StopBits.One);            
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
        public StopBits StopBit
        {
            get => _sp.StopBits;
            set => _sp.StopBits = value;
        }
        /// <summary>
        /// Позволяет задать или получить терменал окончания строки.
        /// </summary>
        public string EndLineTerm {
            set { _sp.NewLine = value; }
            get { return _sp.NewLine; }
        }
        /// <summary>
        /// Открывает соединение с Com портом.
        /// </summary>
        /// <returns>Возвращает True, если порт открыт, иначе False</returns>
        public bool Open()
        {
            if (_sp.IsOpen)
            {
                Logger.Error($"Порт {_sp.PortName} уже открыт.");
                return true;
            }
            try
            {
                _sp.Open();
                _sp.DataReceived += SerialPort_DataReceived;
            }
            catch (UnauthorizedAccessException e)
            {
                Logger.Error(e);
                return false;
            }              
            return true;
        }
        public void Close()
        {
            try
            {
                if (!_sp.IsOpen)
                {
                Logger.Debug($"Порт {_sp.PortName} уже закрыт.");
                return;
                 }

                _sp.DataReceived -= SerialPort_DataReceived;
                _sp.Close();
            }
            catch(IOException e)
            {
                Logger.Error(e);
            }
        }


        /// <summary>
        /// Считывает строку оканчивающуюся терминальнм символом из ComPort.
        /// </summary>
        /// <returns>Возвращает рузультат чтения</returns>
        public string ReadLine()
        {
            if (!_sp.IsOpen) return null;
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
            return null;
        }
        protected void Write(byte[] sendData, int v, int length)
        {
            if (!_sp.IsOpen)
            {
                Logger.Warn($@"Запись в порт {_sp.PortName}данных:{sendData} не выполнена");
                return;
            }
            _sp.Write(sendData, v, length);
        }

        protected void DiscardInBuffer()
        {
            _sp.DiscardInBuffer();
        }
        /// <summary>
        /// Записывает в порт строку.
        /// </summary>
        /// <param name="data"></param>
        public void Write(string data)
        {
            if(!_sp.IsOpen)
            {
                Logger.Warn($@"Запись в порт {_sp.PortName}данных:{data} не выполнена");
                return;
            }
            _sp.Write(data);
        }
        /// <summary>
        /// Записывает строку оканчивающуюся терминальнм символом в ComPort.
        /// </summary>
        /// <returns>Возвращает рузультат чтения</returns>
        public void WriteLine(string data)
        {
            if (!_sp.IsOpen)
            {
                Logger.Warn($@"Запись в порт {_sp.PortName}данных:{data} не выполнена");
                return;
            }
            _sp.WriteLine(data);
        }
        public static string[] GetPortName()
        {
            return SerialPort.GetPortNames();
        }

        public void Dispose()
        {
            _sp.DataReceived -= SerialPort_DataReceived;
            _sp?.Dispose();
        }

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
            R110= 110,
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
            R38400 = 38400,
            R57600 = 57600,
            R115200 = 115200,
            R128000 = 128000
        }
    }
}
