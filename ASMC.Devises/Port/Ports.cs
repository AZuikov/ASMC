// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Globalization;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace ASMC.Devises.Port
{
    public class Ports
    {
        protected SerialPort SP;
        /// <summary>
        /// Установка таймаута, изначально выставлено 500 мс
        /// </summary>
        /// <value>
        /// Значение в милисекундах
        /// </value>
        public int SetTimeout {
            set
            {
                SP.WriteTimeout = value;
                SP.ReadTimeout = value;
            }
        }
        public Ports(string PortName)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            SP = new SerialPort(PortName, (int)SpeedRate.R9600,Parity.None, (int)DataBit.bit8, StopBits.One);            
        }
        public Ports(string PortName, SpeedRate bautRate)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            SP = new SerialPort(PortName, (int)bautRate, Parity.None, (int)DataBit.bit8, StopBits.One);
        }
        public Ports(string PortName, SpeedRate bautRate, Parity parity)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            SP = new SerialPort(PortName, (int)bautRate, parity, (int)DataBit.bit8, StopBits.One);
        }
        public Ports(string PortName, SpeedRate bautRate, Parity parity, DataBit databit)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            SP = new SerialPort(PortName, (int)bautRate, parity, (int)databit, StopBits.One);
        }
        public Ports(string PortName, SpeedRate bautRate, Parity parity, DataBit databit, StopBits stopbits)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            SP = new SerialPort(PortName, (int)bautRate, parity, (int)databit, stopbits);
        }

        public string EndLineTerm {
            set { SP.NewLine = value; }
            get { return SP.NewLine; }
        }

        public bool Open()
        {
            if (!SP.IsOpen)
            {
                try
                {
                    SP.Open();
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox                .Show("Не удаеться открыть COM port " + SP.PortName, "Ошибка COM порта", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }              
                return true;
            }
            else
            {
                MessageBox.Show("Не удаеться открыть COM port " + SP.PortName, "Ошибка COM порта", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        public void Close()
        {
            if (SP.IsOpen)
            {
                SP.Close();
            }
        }

      

        public string ReadLine()
        {
            if (SP.IsOpen)
            {
                try
                {
                    return SP.ReadLine();
                }
                catch (TimeoutException)
                {
                    
                }
                catch (Exception)
                {

                }
            }
            return "";
        }
        public bool Write(string data)
        {
            if (SP.IsOpen)
            {
                SP.Write(data);
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool WriteLine(string data)
        {
            if (SP.IsOpen)
            {
                SP.WriteLine(data);
                return true;
            }
            else
            {
                return false;
            }
        }
        public static string[] GetPortName()
        {
            return SerialPort.GetPortNames();
        }
        public enum DataBit
        {
            bit4 = 4,
            bit5 = 5,
            bit6 = 6,
            bit7 = 7,
            bit8 = 8
        }
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
