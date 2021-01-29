using System;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;

//using Timer = System.Windows.Forms.Timer;
using Timer = System.Timers.Timer;

namespace ASMC.Devices.Port.IZ_Tech
{
    public class MIT_8 : ComPort
    {
        private byte[] buffer = new byte[1024];
        private string readData;
        private bool newDataRead = false;

        private readonly Timer _wait;
        private static readonly AutoResetEvent WaitEvent = new AutoResetEvent(false);
        private bool _flagTimeout;

        public MIT_8()
        {
            UserType = "МИТ 8";
            BaudRate = SpeedRate.R9600;
            StopBit = StopBits.One;
            Parity = Parity.None;
            DataBit = DBit.Bit8;
            EndLineTerm = " ";
            _wait = new Timer();
            _wait.Interval = 35000;
            _wait.Elapsed += TWait_Elapsed;
            _wait.AutoReset = false;
        }

        private void TWait_Elapsed(object sender, ElapsedEventArgs e)
        {
            _flagTimeout = true;
            WaitEvent.Set();
        }

        /// <summary>
        /// Начало опроса прибора
        /// </summary>
        public void startMonitoring()
        {
            Open();
        }

        /// <summary>
        /// Окончить опрос прибора.
        /// </summary>
        public void stopMonitoring()
        {
            Close();
        }

        public decimal ReadDataFromChanel(int inChanel)
        {
            _wait.Start();
            decimal result;
            while (true)
            {
                //Regex chanelNumbeRegex = new Regex($"{inChanel}:");
                //Regex regexForMeasVal = new Regex(pattern: @"(?<=\d:)(\S+)(?=\w)"); //регулярка для получения измеренного значения
                Regex regexForMeasVal1 = new Regex(pattern: $@"(?<={inChanel}:)(\S+)(?=\w)"); //регулярка для получения измеренного значения
                string str = ReadLine();

                var match = regexForMeasVal1.Match(str ?? String.Empty).Value;
                if (!string.IsNullOrWhiteSpace(match))
                {
                    return (decimal)StrToDouble(match);
                }

                // return (decimal)StrToDoubleMindMind(.Value);
            }
        }

        /// <summary>
        /// считываем данные с прибора.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //protected override void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    int byteCount = ReadByte(buffer, 0, buffer.Length, false);
        //    if (byteCount > 0)

        //    {
        //        char[] chars = new char[byteCount];
        //        for (int i = 0; i < byteCount; i++)
        //            chars[i] = Convert.ToChar(buffer[i]);
        //        readData = new string(chars);
        //        _wait.Stop();
        //        WaitEvent.Set();
        //        newDataRead = true;
        //    }
        //}
    }
}