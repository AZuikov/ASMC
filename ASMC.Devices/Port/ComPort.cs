// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using NLog;

namespace ASMC.Devices.Port
{
    public class ComPort
    {
        /// <summary>
        /// Позволяет получать имя устройства.
        /// </summary>
        public string DeviceType { get; protected set; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected SerialPort Sp;
        /// <summary>
        /// Установка таймаута, изначально выставлено 500 мс
        /// </summary>
        /// <value>
        /// Значение в милисекундах
        /// </value>
        public int SetTimeout {
            set
            {
                Sp.WriteTimeout = value;
                Sp.ReadTimeout = value;
            }
        }
        public ComPort()
        {
        }
        public ComPort(string portName)
        {
            Sp = new SerialPort(portName, (int)SpeedRate.R9600,Parity.None, (int)DataBit.Bit8, StopBits.One);            
        }
        public ComPort(string portName, SpeedRate bautRate)
        {
            Sp = new SerialPort(portName, (int)bautRate, Parity.None, (int)DataBit.Bit8, StopBits.One);
        }
        public ComPort(string portName, SpeedRate bautRate, Parity parity)
        {
            Sp = new SerialPort(portName, (int)bautRate, parity, (int)DataBit.Bit8, StopBits.One);
        }
        public ComPort(string portName, SpeedRate bautRate, Parity parity, DataBit databit)
        {
            Sp = new SerialPort(portName, (int)bautRate, parity, (int)databit, StopBits.One);
        }
        public ComPort(string portName, SpeedRate bautRate, Parity parity, DataBit databit, StopBits stopbits)
        {
            Sp = new SerialPort(portName, (int)bautRate, parity, (int)databit, stopbits);
        }
        /// <summary>
        /// Позволяет задать или получить терменал окончания строки.
        /// </summary>
        public string EndLineTerm {
            set { Sp.NewLine = value; }
            get { return Sp.NewLine; }
        }
        /// <summary>
        /// Открывает соединение с Com портом.
        /// </summary>
        /// <returns>Возвращает True, если порт открыт, иначе False</returns>
        public bool Open()
        {
            if (Sp.IsOpen) return true;
            try
            {
                Sp.Open();
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
            if (!Sp.IsOpen) return;
            try
            {  
                Sp.Close();
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
            if (!Sp.IsOpen) return null;
            try
            {
                return Sp.ReadLine();
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
        /// <summary>
        /// Записывает в порт строку.
        /// </summary>
        /// <param name="data"></param>
        public void Write(string data)
        {
            if(!Sp.IsOpen)
            {
                Logger.Warn($@"Запись в порт {Sp.PortName}данных:{data} не выполнена");
                return;
            }
            Sp.Write(data);
        }
        /// <summary>
        /// Записывает строку оканчивающуюся терминальнм символом в ComPort.
        /// </summary>
        /// <returns>Возвращает рузультат чтения</returns>
        public void WriteLine(string data)
        {
            if (!Sp.IsOpen)
            {
                Logger.Warn($@"Запись в порт {Sp.PortName}данных:{data} не выполнена");
                return;
            }
            Sp.WriteLine(data);
        }
        public static string[] GetPortName()
        {
            return SerialPort.GetPortNames();
        }
        /// <summary>
        /// Предоставлет перечесление возможного размера данных.
        /// </summary>
        public enum DataBit
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
