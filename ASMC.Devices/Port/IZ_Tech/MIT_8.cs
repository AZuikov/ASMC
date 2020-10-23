using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Devices.Port.IZ_Tech
{
   public class MIT_8 : ComPort
    {

        public MIT_8()
        {
            UserType = "МИТ 8";
            BaudRate = SpeedRate.R9600;
            StopBit = StopBits.Two;
            Parity = Parity.None;
            DataBit = DBit.Bit8;
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

        public decimal ReadDataFromChanel(int chanel)
        {
            return -5;
        }

        /// <summary>
        /// считываем данные с прибора.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

        }
    }
}
