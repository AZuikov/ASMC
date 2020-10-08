using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OwenioNet;
using OwenioNet.DataConverter.Converter;
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
        public byte[] OwenReadParam(int PortNumber, int addresDevice, string ParametrName, ushort? Register = null)
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
                dataFromDevice = owenProtocol.OwenRead(addresDevice, AddressLengthType.Bits8, ParametrName, Register);
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
        /// <param name="Register">Индекс параметра (если есть).</param>
        public void OwenWriteParam(int PortNumber, int addresDevice, AddressLengthType addressLengthType, string ParametrName, byte[] writeDataBytes, ushort? Register = null)
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
                owenProtocol.OwenWrite(addresDevice, AddressLengthType.Bits8, ParametrName, writeDataBytes, Register);
            }
            catch (Exception)
            {
                Console.WriteLine("Ничего не прочитали. Что-то пошло не так...");
            }


            port.Close();


        }

        /// <summary>
        /// Функция для считывания параметра типа string.
        /// </summary>
        /// <param name="PortNumber">Номер последовательного порта.</param>
        /// <param name="addresDevice">Адрес устройства.</param>
        /// <param name="addressLengthType">Длина сетевого адреса устройства.</param>
        /// <param name="ParametrName">Наименование параметра устройства.</param>
        /// <param name="Register">Индекс параметра (если есть).</param>
        /// <returns>Считанное значение параметра.</returns>
        public string ReadStringAscii(int PortNumber, int addresDevice, AddressLengthType addressLengthType, string ParametrName, ushort? Register = null)
        {
            byte[] answerDevice = OwenReadParam(PortNumber, addresDevice, ParametrName,  Register);
            var converter = new ConverterAscii(answerDevice.Length);
            string value = converter.ConvertBack(answerDevice);
            return value;

        }

        /// <summary>
        /// Функция для считывания целочисленного параметра (unsigned short int).
        /// </summary>
        /// <param name="PortNumber">Номер последовательного порта.</param>
        /// <param name="addresDevice">Адрес устройства.</param>
        /// <param name="addressLengthType">Длина сетевого адреса устройства.</param>
        /// <param name="ParametrName">Наименование параметра устройства.</param>
        /// <param name="size">Размер ожидаемого числа в байтах.</param>
        /// <param name="Register">Индекс параметра (если есть).</param>
        /// <returns>Считанное значение параметра.</returns>
        public int ReadShortIntParam(int PortNumber, int addresDevice, AddressLengthType addressLengthType, string ParametrName, int size, ushort? Register = null)
        {
            byte[] answerDevice = OwenReadParam(PortNumber, addresDevice, ParametrName, Register);
            var converter = new ConverterI(size);
            int value = converter.ConvertBack(answerDevice);
            return value;

        }

        /// <summary>
        /// Функция для считывания параметра типа float.
        /// </summary>
        /// <param name="PortNumber">Номер последовательного порта.</param>
        /// <param name="addresDevice">Адрес устройства.</param>
        /// <param name="addressLengthType">Длина сетевого адреса устройства.</param>
        /// <param name="ParametrName">Наименование параметра устройства.</param>
        /// <param name="size">Размер ожидаемого числа в байтах.</param>
        /// <param name="Register">Индекс параметра (если есть).</param>
        /// <returns>Считанное значение параметра.</returns>
        public float ReadFloatParam(int PortNumber, int addresDevice, AddressLengthType addressLengthType, string ParametrName, int size, ushort? Register = null)
        {
            byte[] answerDevice = OwenReadParam(PortNumber, addresDevice, ParametrName, Register);
            var converter = new ConverterFloat(size);
            float value = converter.ConvertBack(answerDevice);
            return value;

        }
    }
}
