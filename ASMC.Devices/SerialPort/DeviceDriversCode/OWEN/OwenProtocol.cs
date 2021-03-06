﻿using ASMC.Devices.Port;
using NLog;
using OwenioNet;
using OwenioNet.DataConverter.Converter;
using OwenioNet.IO;
using OwenioNet.Types;
using System;
using System.IO.Ports;

namespace ASMC.Devices.OWEN
{
    public class OwenProtocol : ComPort, IStreamResource
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        public Enum Parametr;

        #endregion Fields

        #region Property

        

        #endregion Property

        /// <summary>
        /// Инициализирует объект.
        /// </summary>
        /// <param name = "Addres">Адрес устройства ТРМ202. Его нужно знать заранее.</param>
        public OwenProtocol()
        {
            BaudRate = SpeedRate.R115200;
            Parity = Parity.None;
            DataBit = DBit.Bit8;
            StopBit = StopBits.One;
        }

        #region Methods

        /// <summary>
        /// Считывает значение параметра с устройства.
        /// </summary>
        /// <param name = "PortNumber">Номер последовательного порта.</param>
        /// <param name = "addresDevice">Удрес устройства.</param>
        /// <param name = "ParametrName">Имя параметра, который необходимо считать.</param>
        /// <returns>Массив байт, требующий конверткации.</returns>
        public virtual byte[] OwenReadParam(string ParametrName, int addres, ushort? Register = null)
        {
            try
            {
                if (IsOpen != true) Open();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            var owenProtocol = OwenProtocolMaster.Create(this);

            if (IsOpen != true) Logger.Error("Ошибка открытия порта: {0}", StringConnection);

            byte[] dataFromDevice = { 0x00 };

            try
            {
                dataFromDevice = owenProtocol.OwenRead(addres, AddressLengthType.Bits8, ParametrName, Register);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка чтения ОВЕН: " + ex);
            }

            Close();

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
        public void OwenWriteParam(int addresDevice,AddressLengthType addressLengthType, string ParametrName, byte[] writeDataBytes, ushort? Register = null)
        {
            try
            {
                if (IsOpen != true) Open();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            var owenProtocol = OwenProtocolMaster.Create(this);

            if (IsOpened != true) Logger.Error("Ошибка открытия порта: {0}", StringConnection);
            try
            {
                owenProtocol.OwenWrite(addresDevice, addressLengthType, ParametrName, writeDataBytes, Register);
            }
            finally
            {
                Close();
            } 
            
            
            
        }

        /// <summary>
        /// Функция для считывания параметра типа float.
        /// </summary>
        /// <param name = "addresDevice">Адрес устройства.</param>
        /// <param name = "addressLengthType">Длина сетевого адреса устройства.</param>
        /// <param name = "ParametrName">Наименование параметра устройства.</param>
        /// <param name = "size">Размер ожидаемого числа в байтах.</param>
        /// <param name = "Register">Индекс параметра (если есть).</param>
        /// <returns>Считанное значение параметра.</returns>
        public float ReadFloatParam(int addresDevice, AddressLengthType addressLengthType,
            string ParametrName, int size, ushort? Register = null)
        {
            var answerDevice = OwenReadParam(ParametrName, addresDevice, Register);
            var converter = new ConverterFloat(size);
            var value = converter.ConvertBack(answerDevice);
            return value;
           
        }

        public void WriteFloatParam(int addresDevice, AddressLengthType addressLengthType, string ParametrName, float ParVal, ushort? Register = null)
        {
            var bitBuffer = BitConverter.GetBytes(ParVal);
            OwenWriteParam( addresDevice,  addressLengthType, ParametrName, bitBuffer ,Register);
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
        public int ReadShortIntParam( int addresDevice, AddressLengthType addressLengthType,
            string ParametrName, int size, ushort? Register = null)
        {
            var answerDevice = OwenReadParam(ParametrName, addresDevice, Register);
            var converter = new ConverterI(size);
            var value = converter.ConvertBack(answerDevice);
            return value;
        }
        public void WriteIntParam(int addresDevice, AddressLengthType addressLengthType, string ParametrName, int ParVal, ushort? Register = null)
        {
            var bitBuffer = BitConverter.GetBytes(ParVal);
            OwenWriteParam(addresDevice, addressLengthType, ParametrName, bitBuffer, Register);
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
            var answerDevice = OwenReadParam(ParametrName, addresDevice,  Register);
            var converter = new ConverterAscii(answerDevice.Length);
            var value = converter.ConvertBack(answerDevice);
            return value;
        }

        #endregion Methods

        public new void DiscardInBuffer()
        {
            base.DiscardInBuffer();
        }

        public bool IsOpened => IsOpen;
        public int ReadTimeout { get; set; }
        public int WriteTimeout { get; set; }
    }
}