using System;
using System.IO.Ports;
using NLog;
using OwenioNet;
using OwenioNet.DataConverter.Converter;
using OwenioNet.IO;
using OwenioNet.Types;

namespace ASMC.Devices.OWEN
{
    internal class OwenProtocol
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Methods

        /// <summary>
        /// Считывает значение параметра с устройства.
        /// </summary>
        /// <param name = "PortNumber">Номер последовательного порта.</param>
        /// <param name = "addresDevice">Удрес устройства.</param>
        /// <param name = "ParametrName">Имя параметра, который необходимо считать.</param>
        /// <returns>Массив байт, требующий конверткации.</returns>
        public byte[] OwenReadParam(int PortNumber, int addresDevice, string ParametrName, ushort? Register = null)
        {
            var port = new SerialPortAdapter(PortNumber, 115200, Parity.None, 8, StopBits.One);

            try
            {
                if (port.IsOpened != true) port.Open();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            var owenProtocol = OwenProtocolMaster.Create(port);

            if (port.IsOpened != true) Logger.Error("Ошибка открытия порта: {0}", port.ToString());

            byte[] dataFromDevice = {0x00};

            try
            {
                dataFromDevice = owenProtocol.OwenRead(addresDevice, AddressLengthType.Bits8, ParametrName, Register);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка чтения ОВЕН: " + ex);
            }

            port.Close();

            return dataFromDevice;
        }

        /// <summary>
        /// Запись значения параметра в устройство.
        /// </summary>
        /// <param name = "PortNumber">Номер последовательного порта.</param>
        /// <param name = "addresDevice">Адрес устройства.</param>
        /// <param name = "addressLengthType">Длина сетевого адреса устройства.</param>
        /// <param name = "ParametrName">Наименование параметра устройства.</param>
        /// <param name = "writeDataBytes">Массив байт дял записи в устройство (значение параметра).</param>
        /// <param name = "Register">Индекс параметра (если есть).</param>
        public void OwenWriteParam(int PortNumber, int addresDevice, AddressLengthType addressLengthType,
            string ParametrName, byte[] writeDataBytes, ushort? Register = null)
        {
            var port = new SerialPortAdapter(PortNumber, 115200, Parity.None, 8, StopBits.One);

            try
            {
                if (port.IsOpened != true) port.Open();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            var owenProtocol = OwenProtocolMaster.Create(port);

            if (port.IsOpened != true) Logger.Error("Ошибка открытия порта: {0}", port.ToString());

            byte[] dataFromDevice = {0x00};

            try
            {
                //dataFromDevice = owenProtocol.OwenRead(addresDevice, AddressLengthType.Bits8, ParametrName, ParIndex);
                owenProtocol.OwenWrite(addresDevice, AddressLengthType.Bits8, ParametrName, writeDataBytes, Register);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка записи ОВЕН: " + ex);
            }

            port.Close();
        }

        /// <summary>
        /// Функция для считывания параметра типа float.
        /// </summary>
        /// <param name = "PortNumber">Номер последовательного порта.</param>
        /// <param name = "addresDevice">Адрес устройства.</param>
        /// <param name = "addressLengthType">Длина сетевого адреса устройства.</param>
        /// <param name = "ParametrName">Наименование параметра устройства.</param>
        /// <param name = "size">Размер ожидаемого числа в байтах.</param>
        /// <param name = "Register">Индекс параметра (если есть).</param>
        /// <returns>Считанное значение параметра.</returns>
        public float ReadFloatParam(int PortNumber, int addresDevice, AddressLengthType addressLengthType,
            string ParametrName, int size, ushort? Register = null)
        {
            var answerDevice = OwenReadParam(PortNumber, addresDevice, ParametrName, Register);
            var converter = new ConverterFloat(size);
            var value = converter.ConvertBack(answerDevice);
            return value;
        }

        /// <summary>
        /// Функция для считывания целочисленного параметра (unsigned short int).
        /// </summary>
        /// <param name = "PortNumber">Номер последовательного порта.</param>
        /// <param name = "addresDevice">Адрес устройства.</param>
        /// <param name = "addressLengthType">Длина сетевого адреса устройства.</param>
        /// <param name = "ParametrName">Наименование параметра устройства.</param>
        /// <param name = "size">Размер ожидаемого числа в байтах.</param>
        /// <param name = "Register">Индекс параметра (если есть).</param>
        /// <returns>Считанное значение параметра.</returns>
        public int ReadShortIntParam(int PortNumber, int addresDevice, AddressLengthType addressLengthType,
            string ParametrName, int size, ushort? Register = null)
        {
            var answerDevice = OwenReadParam(PortNumber, addresDevice, ParametrName, Register);
            var converter = new ConverterI(size);
            var value = converter.ConvertBack(answerDevice);
            return value;
        }

        /// <summary>
        /// Функция для считывания параметра типа string.
        /// </summary>
        /// <param name = "PortNumber">Номер последовательного порта.</param>
        /// <param name = "addresDevice">Адрес устройства.</param>
        /// <param name = "addressLengthType">Длина сетевого адреса устройства.</param>
        /// <param name = "ParametrName">Наименование параметра устройства.</param>
        /// <param name = "Register">Индекс параметра (если есть).</param>
        /// <returns>Считанное значение параметра.</returns>
        public string ReadStringAscii(int PortNumber, int addresDevice, AddressLengthType addressLengthType,
            string ParametrName, ushort? Register = null)
        {
            var answerDevice = OwenReadParam(PortNumber, addresDevice, ParametrName, Register);
            var converter = new ConverterAscii(answerDevice.Length);
            var value = converter.ConvertBack(answerDevice);
            return value;
        }

        #endregion
    }
}