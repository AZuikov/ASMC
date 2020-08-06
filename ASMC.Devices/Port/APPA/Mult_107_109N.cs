﻿// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using NLog;

namespace ASMC.Devices.Port.APPA
{
    // ReSharper disable once InconsistentNaming
    public class Mult107_109N : ComPort
    {
        private readonly System.Timers.Timer _wait;
        private bool _flagTimeout;
        private List<byte> _data;
        //размер посылаемых данных от прибора
        private const byte Cadr = 19;
        //хранит считанные с прибора данные
        private readonly List<byte> _readingBuffer;
        //данные для начало обмена информацией с прибором
        private readonly byte[] _sendData = { 0x55, 0x55, 0x00, 0x00, 0xAA };
        private static readonly AutoResetEvent WaitEvent = new AutoResetEvent(false);

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _wait?.Dispose();
            }
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Измеренное значение на основном экране
        /// </summary>
        public double GetGeneralValue
        {
            get
            {
                SendQuery();
                double value;
                WaitEvent.WaitOne();
                if (_flagTimeout)
                {
                    _flagTimeout = false;
                    throw new TimeoutException();
                }
                switch (_data[11] & 0x07)
                {
                    case (int)Point.None:
                        value = (_data[10] << 8) | (_data[9] << 8) | _data[8];
                        break;
                    case (int)Point.Point1:
                        value = ((_data[10] << 8) | (_data[9] << 8) | _data[8]) / 10.0;
                        break;
                    case (int)Point.Point2:
                        value = ((_data[10] << 8) | (_data[9] << 8) | _data[8]) / 100.0;
                        break;
                    case (int)Point.Point3:
                        value = ((_data[10] << 8) | (_data[9] << 8) | _data[8]) / 1000.0;
                        break;
                    case (int)Point.Point4:
                        value = ((_data[10] << 8) | (_data[9] << 8) | _data[8]) / 10000.0;
                        break;
                    default:
                        return 0;
                }
                Logger.Info(value);
                return value; 
            }
        }

        /// <summary>
        /// Значение со второй строки экрана (верхняя с мелким шрифтом)
        /// </summary>
        public double GetSubValue
        {
            get
            {
                SendQuery();
                double value;
                WaitEvent.WaitOne();
                if (_flagTimeout)
                {
                    _flagTimeout = false;
                    throw new TimeoutException();
                }
                switch (_data[16] & 0x07)
                {
                    case (int)Point.None:
                        value = (_data[15] << 8) | (_data[14] << 8) | _data[13];
                        break;
                    case (int)Point.Point1:
                        value = ((_data[15] << 8) | (_data[14] << 8) | _data[13]) / 10.0;
                        break;
                    case (int)Point.Point2:
                        value = ((_data[15] << 8) | (_data[14] << 8) | _data[13]) / 100.0;
                        break;
                    case (int)Point.Point3:
                        value = ((_data[15] << 8) | (_data[14] << 8) | _data[13]) / 1000.0;
                        break;
                    case (int)Point.Point4:
                        value = ((_data[15] << 8) | (_data[14] << 8) | _data[13]) / 10000.0;
                        break;
                    default:
                        return 0;
                }
                Logger.Info(value);
                return value;
            }
        }

        public Mult107_109N() 
        {
            _wait = new System.Timers.Timer();
            _readingBuffer = new List<byte>();
            _data = new List<byte>();
            _flagTimeout = false;
            _wait.Interval = 35000;
            _wait.Elapsed += TWait_Elapsed;
            _wait.AutoReset = false;
        }
        private void TWait_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _flagTimeout = true;
            WaitEvent.Set();
        }
        public void SendQuery()
        {

            if (Open())
            {
                Write(_sendData, 0, _sendData.Length);
                _wait.Start();
            }
            else Close();

        }

      
        /// <summary>
        /// Положение переключатиля
        /// </summary>
        public enum Rotor
        {
            OFF = 0x00,
            V = 0x01,
            MV = 0x02,
            Ohm = 0x03,
            Diode = 0x04,
            MA = 0x05,
            A = 0x06,
            Cap = 0x07,
            Hz = 0x08,
            Temp = 0x09
        }
        public enum Point
        {
            None = 0,
            Point1 = 0x01,
            Point2 = 0x02,
            Point3 = 0x03,
            Point4 = 0x04
        }
        public enum Range
        {
            Range1Manual = 0x80,
            Range2Manual = 0x81,
            Range3Manual = 0x82,
            Range4Manual = 0x83,
            Range5Manual = 0x84,
            Range6Manual = 0x85,
            Range7Manual = 0x86,
            Range8Manual = 0x87,
            Range1Auto = 0,
            Range2Auto = 0x01,
            Range3Auto = 0x02,
            Range4Auto = 0x03,
            Range5Auto = 0x04,
            Range6Auto = 0x05,
            Range7Auto = 0x06,
            Range8Auto = 0x07

        }

        public enum Function
        {
            None = 0x00,
            InputReading = 0x01,
            Freq = 0x02,
            Period = 0x03,
            DutyFactor = 0x04,
            StampStoreRecallLoginLogout = 0x08,
            Store = 0x09,
            Recall = 0x0A,
            AutoHold = 0x0C,
            Max = 0x0D,
            Min = 0x0E,
            PeakHoldMax = 0x10,
            PeakHoldMin = 0x11,
            Delta = 0x17,
            Ref = 0x19,
            dBm = 0x1A,
            dB = 0x1B,
            Avg = 0x25,
            ProbECharacter = 0x26,
            ErCharacter = 0x27,
            FuseCharacter = 0x28,
            PausCharacter = 0x29,
            LogoutMaxData = 0x2A,
            LogoutMinData = 0x2B,
            LogoutMaxTurningPoint = 0x2C,
            LogoutMinTurningPoint = 0x2D,
            LogoutData = 0x2E,
            PeriodTime = 0x2F,
            FullCharacter = 0x30,
            EpErCharacter = 0x31,
            EepromCharacter = 0x32,
            LoginStamp = 0x33
        }

        public enum BlueState
        {
            NoPress = 0,
            OnPress = 0x01,
            DoublePress = 0x02
        }



        /// <summary>
        /// Получает данные с прибора, записывает их в буфер для проверки контрольной суммы.
        /// </summary>
        /// <param name="sender">Устройство передающее данные</param>
        /// <param name="e"></param>
        protected override void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _readingBuffer.Clear();
            var port = (SerialPort)sender;
            try
            {
                byte bt = (byte)port.ReadByte();
                _readingBuffer.Add(bt);
                while (_readingBuffer.Count < Cadr)
                {
                    if (0x55 == _readingBuffer[0])
                    {
                        _readingBuffer.Add((byte)port.ReadByte());
                    }
                }
                DiscardInBuffer();
                CheckControlSumm();
                //Sp.DataReceived -= SerialPort_DataReceived;
            }
            catch (Exception a)
            {
                Logger.Error(a);
            }

        }

        /// <summary>
        /// Проверка контрольной суммы полученных данных
        /// </summary>
        /// <returns></returns>
        private void CheckControlSumm()
        {
            if (_readingBuffer.Count != Cadr) return ;
            var chechSum = 0;
            for (var i = 0; i < _readingBuffer.Count - 1; i++)
            {
                chechSum = chechSum + _readingBuffer[i];
            }
            if (_readingBuffer[_readingBuffer.Count - 1] != (chechSum & 0xFF))
            {
                SendQuery();
                WaitEvent.Reset();
                return ;
            }
            //если сумма совпадает тогда считанные данные из буфера пишем в поле
            _data = _readingBuffer;
            _wait.Stop();
            WaitEvent.Set();

        }
        

        public Range GetRange
        {
            get
            {
                SendQuery();
                WaitEvent.WaitOne();
                if (_flagTimeout)
                {
                    _flagTimeout = false;
                    throw new TimeoutException();
                }
                Logger.Info(((Range)_data[7]).ToString());
                return (Range)_data[7];
            }
        }

        public Function GetSubFunction
        {
            get
            {
                SendQuery();
                WaitEvent.WaitOne();
                if (_flagTimeout)
                {
                    _flagTimeout = false;
                    throw new TimeoutException();
                }
                Logger.Info(((Function)_data[17]).ToString());
                return (Function)_data[17];
            }
        }

        public Function GetGeneralFunction
        {
            get
            {
                SendQuery();
                WaitEvent.WaitOne();
                if (_flagTimeout)
                {
                    _flagTimeout = false;
                    throw new TimeoutException();
                }
                Logger.Info(((Function)_data[12]).ToString());
                return (Function)_data[12];
            }
        }

        public Rotor GetRotor
        {
            get
            {
                SendQuery();
                WaitEvent.WaitOne();
                if (_flagTimeout)
                {
                    _flagTimeout = false;
                    throw new TimeoutException();
                }
                Logger.Info(((Rotor)_data[4]).ToString());
                return (Rotor)_data[4];
            }
        }
        public BlueState GetBlueState
        {
            get
            {
                SendQuery();
                WaitEvent.WaitOne();
                if (_flagTimeout)
                {
                    _flagTimeout = false;
                    throw new TimeoutException();
                }
                Logger.Info(((BlueState)_data[5]).ToString());
                return (BlueState)_data[5];
            }
        }
    }
}
