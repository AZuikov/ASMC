using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OwenioNet;
using OwenioNet.IO;
using OwenioNet.Types;

namespace ASMC.Devices.OWEN
{
    class OwenProtocol
    {
        /// <summary>
        /// Считывает значение параметра с устройства.
        /// </summary>
        /// <param name="PortNumber">Номер последовательного порта.</param>
        /// <param name="addresDevice">Удрес устройства.</param>
        /// <param name="ParametrName">Имя параметра, который необходимо считать.</param>
        /// <returns>Массив байт, требующий конверткации.</returns>
        public byte[] OwenReadParam(int PortNumber, int addresDevice, string ParametrName, ushort? ParIndex = null)
        {
            SerialPortAdapter port = new SerialPortAdapter(PortNumber, 115200, Parity.None, 8, StopBits.One);

            try
            {
                if (port.IsOpened != true)
                {
                    port.Open();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var owenProtocol = OwenProtocolMaster.Create(port);

            if (port.IsOpened != true)
            {
                Console.WriteLine("Ошибка открытия порта: {0}", port.ToString());
            }


            byte[] dataFromDevice = new byte[] { 0x00 };

            try
            {
                dataFromDevice = owenProtocol.OwenRead(addresDevice, AddressLengthType.Bits8, ParametrName, ParIndex);
            }
            catch (Exception)
            {
                Console.WriteLine("Ничего не прочитали. Что-то пошло не так...");
            }


            port.Close();

            return dataFromDevice;
        }

        /// <summary>
        /// Запись значения параметра в устройство.
        /// </summary>
        /// <param name="PortNumber">Номер последовательного порта.</param>
        /// <param name="addresDevice">Адрес устройства.</param>
        /// <param name="addressLengthType">Длина сетевого адреса устройства.</param>
        /// <param name="ParametrName">Наименование параметра устройства.</param>
        /// <param name="writeDataBytes">Массив байт дял записи в устройство (значение параметра).</param>
        /// <param name="ParIndex">Индекс параметра (если есть).</param>
        public void OwenWriteParam(int PortNumber, int addresDevice, AddressLengthType addressLengthType, string ParametrName, byte[] writeDataBytes, ushort? ParIndex = null)
        {
            SerialPortAdapter port = new SerialPortAdapter(PortNumber, 115200, Parity.None, 8, StopBits.One);

            try
            {
                if (port.IsOpened != true)
                {
                    port.Open();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var owenProtocol = OwenProtocolMaster.Create(port);

            if (port.IsOpened != true)
            {
                Console.WriteLine("Ошибка открытия порта: {0}", port.ToString());
            }


            byte[] dataFromDevice = new byte[] { 0x00 };

            try
            {
                //dataFromDevice = owenProtocol.OwenRead(addresDevice, AddressLengthType.Bits8, ParametrName, ParIndex);
                owenProtocol.OwenWrite(addresDevice, AddressLengthType.Bits8, ParametrName, writeDataBytes, ParIndex);
            }
            catch (Exception)
            {
                Console.WriteLine("Ничего не прочитали. Что-то пошло не так...");
            }


            port.Close();


        }
    }
}
