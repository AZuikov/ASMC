// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using AP.Utils.Data;
using NLog;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;
using ASMC.Devices;

namespace ASMC.Devices.Port.APPA
{
    // ReSharper disable once InconsistentNaming
    public class Mult107_109N : ComPort
    {
        /// <summary>
        /// Единицы измерения мультиметра
        /// </summary>
        public enum Units
        {
            [DoubleValue(1)][StringValue("")] None,
            [DoubleValue(1)][StringValue("В")] V,
            [DoubleValue(1E-3)][StringValue("мВ")] mV,
            [DoubleValue(1)][StringValue("А")] A,
            [DoubleValue(1E-3)][StringValue("мА")] mA,
            [DoubleValue(1)][StringValue("дБ")] dB,
            [DoubleValue(1)][StringValue("дБм")] dBm,
            [DoubleValue(1E-9)][StringValue("нФ")] nF,
            [DoubleValue(1E-6)][StringValue("мкФ")] uF,
            [DoubleValue(1E-3)][StringValue("мФ")] mF,
            [DoubleValue(1)][StringValue("Ом")] Ohm,
            [DoubleValue(1E3)][StringValue("кОм")] KOhm,
            [DoubleValue(1E6)][StringValue("МОм")] MOhm,
            [DoubleValue(1E9)][StringValue("ГОм")] GOhm,
            [DoubleValue(1)][StringValue("%")] Percent,
            [DoubleValue(1)][StringValue("Гц")] Hz,
            [DoubleValue(1E3)][StringValue("кГц")] KHz,
            [DoubleValue(1E6)][StringValue("МГц")] MHz,
            [DoubleValue(1)][StringValue("℃")] CelciumGrad,
            [DoubleValue(1)][StringValue("℉")] FaringeitGrad,
            [DoubleValue(1)][StringValue("сек")] Sec,
            [DoubleValue(1E-3)][StringValue("мсек")] mSec,
            [DoubleValue(1E-9)][StringValue("нсек")] nSec,
            [DoubleValue(1)][StringValue("В")] Volt,
            [DoubleValue(1E-3)][StringValue("мВ")] mVolt,
            [DoubleValue(1)][StringValue("А")] Amp,
            [DoubleValue(1E-3)][StringValue("мА")] mAmp,
            [DoubleValue(1)][StringValue("Ом")] Ohm2,
            [DoubleValue(1E3)][StringValue("кОм")] KOhm2,
            [DoubleValue(1E6)] [StringValue("МОм")] MOhm3
        }

        public enum BlueState
        {
            NoPress = 0,
            OnPress = 0x01,
            DoublePress = 0x02
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

        /// <summary>
        /// Положение переключатиля
        /// </summary>
        public enum Rotor
        {
            OFF = 0x00,
            [StringValue("В")] V = 0x01,
            [StringValue("мВ")] mV = 0x02,
            [StringValue("Ом")] Ohm = 0x03,
            [StringValue("В")] Diode = 0x04,
            [StringValue("мА")] mA = 0x05,
            [StringValue("А")] A = 0x06,
            [StringValue("Ф")] Cap = 0x07,
            [StringValue("Гц")] Hz = 0x08,
            Temp = 0x09
        }

        //размер посылаемых данных от прибора
        private const byte Cadr = 19;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly AutoResetEvent WaitEvent = new AutoResetEvent(false);

        #region Fields

        //хранит считанные с прибора данные
        private readonly List<byte> _readingBuffer;

        //данные для начало обмена информацией с прибором
        private readonly byte[] _sendData = { 0x55, 0x55, 0x00, 0x00, 0xAA };

        private readonly Timer _wait;
        private List<byte> _data;
        private bool _flagTimeout;

        #endregion Fields

        #region Property

        /// <summary>
        /// Позволяет получить информацию о текущих единицах измерения с основного экрана.
        /// </summary>
        public Units GetGeneralMeasureUnit
        {
            get
            {
                return (Units)(_data[11] >> 3);
            }
        }

        /// <summary>
        /// Возвращает единицы измерения со второго экрана.
        /// </summary>
        public Units GeSubMeasureUnit
        {
            get
            {
                return (Units)(_data[16] >> 3);
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

        /// <summary>
        /// Возвращает измеренное значение с прибора, приведенное к единицам в соответствии с множителем mult
        /// </summary>
        /// <param name="mult">Множитель единицы измерения.</param>
        /// <param name="generalDsiplay">Если флаг true, тогда возвращаются показания с основного экрана прибора. Иначе с второстепенного.</param>
        /// <returns></returns>
        public double GetValue(Multipliers mult = Devices.Multipliers.None, bool generalDsiplay = true)
        {
            SendQuery();
            double value;
            WaitEvent.WaitOne();
            if (_flagTimeout)
            {
                _flagTimeout = false;
                throw new TimeoutException();
            }
            // по умолчанию запрашиваем показания с главного экрана
            if (generalDsiplay)
            {
                if (_data[10] == 255)
                    value = ~((0xff - _data[10] << 16) | (0xff - _data[9] << 8) | (0xff - _data[8])) - 1;
                else value = ((_data[10] << 16) | (_data[9] << 8) | _data[8]);
                value= GetPointInfo(value, _data[11]);
             
            }
            //если запрашиваются показания со второго экрана
            else
            {
                if (_data[15] == 255)
                    value = ~((0xff - _data[15] << 16) | (0xff - _data[14] << 8) | (0xff - _data[13])) - 1;
                else value = ((_data[15] << 16) | (_data[14] << 8) | _data[13]);
                value = GetPointInfo(value, _data[16]);
            }

            double GetPointInfo(double val,byte addres)
            {
                switch (addres & 0x07)
                {
                    case (int)Point.Point1:
                        return val /= 10.0;
                        

                    case (int)Point.Point2:
                        return val /= 100.0;
                        

                    case (int)Point.Point3:
                        return val /= 1000.0;
                       
                    case (int)Point.Point4:
                      return val /= 10000.0;
                    default:
                        return val;

                }
            }


            Logger.Info(value);

            value *= GetGeneralMeasureUnit.GetDoubleValue();

            return DoubleToDoubleMind(value, mult);
        }

        ///// <summary>
        ///// Измеренное значение на основном экране
        ///// </summary>
        //public double GetGeneralValue
        //{
        //    get
        //    {
        //        SendQuery();
        //        double value;
        //        WaitEvent.WaitOne();
        //        if (_flagTimeout)
        //        {
        //            _flagTimeout = false;
        //            throw new TimeoutException();
        //        }

        //        if (_data[10] == 255)
        //            value = ~((0xff - _data[10] << 16) | (0xff - _data[9] << 8) | (0xff - _data[8])) - 1;
        //        else value = ((_data[10] << 16) | (_data[9] << 8) | _data[8]);

        //        switch (_data[11] & 0x07)
        //        {
        //            case (int)Point.None:
        //                break;

        //            case (int)Point.Point1:
        //                value /= 10.0;
        //                break;

        //            case (int)Point.Point2:
        //                value /= 100.0;
        //                break;

        //            case (int)Point.Point3:
        //                value /= 1000.0;
        //                break;

        //            case (int)Point.Point4:
        //                value /= 10000.0;
        //                break;

        //            default:
        //                return 0;
        //        }

        //        Logger.Info(value);
        //        return value;
        //    }
        //}

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

        ///// <summary>
        ///// Значение со второй строки экрана (верхняя с мелким шрифтом)
        ///// </summary>
        //public double GetSubValue
        //{
        //    get
        //    {
        //        SendQuery();
        //        double value;
        //        WaitEvent.WaitOne();
        //        if (_flagTimeout)
        //        {
        //            _flagTimeout = false;
        //            throw new TimeoutException();
        //        }

        //        switch (_data[16] & 0x07)
        //        {
        //            case (int)Point.None:
        //                value = (_data[15] << 8) | (_data[14] << 8) | _data[13];
        //                break;

        //            case (int)Point.Point1:
        //                value = ((_data[15] << 8) | (_data[14] << 8) | _data[13]) / 10.0;
        //                break;

        //            case (int)Point.Point2:
        //                value = ((_data[15] << 8) | (_data[14] << 8) | _data[13]) / 100.0;
        //                break;

        //            case (int)Point.Point3:
        //                value = ((_data[15] << 8) | (_data[14] << 8) | _data[13]) / 1000.0;
        //                break;

        //            case (int)Point.Point4:
        //                value = ((_data[15] << 8) | (_data[14] << 8) | _data[13]) / 10000.0;
        //                break;

        //            default:
        //                return 0;
        //        }

        //        Logger.Info(value);
        //        return value;
        //    }
        //}

        #endregion Property

        public Mult107_109N()
        {
            _wait = new Timer();
            _readingBuffer = new List<byte>();
            _data = new List<byte>();
            _flagTimeout = false;
            _wait.Interval = 35000;
            _wait.Elapsed += TWait_Elapsed;
            _wait.AutoReset = false;
        }

        #region Methods

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void SendQuery()
        {
            if (Open())
            {
                Write(_sendData, 0, _sendData.Length);
                _wait.Start();
            }
            else
            {
                Close();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) _wait?.Dispose();
        }

        /// <summary>
        /// Получает данные с прибора, записывает их в буфер для проверки контрольной суммы.
        /// </summary>
        /// <param name = "sender">Устройство передающее данные</param>
        /// <param name = "e"></param>
        protected override void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _readingBuffer.Clear();
            var port = (SerialPort)sender;
            try
            {
                var bt = (byte)port.ReadByte();
                _readingBuffer.Add(bt);
                while (_readingBuffer.Count < Cadr)
                    if (0x55 == _readingBuffer[0])
                        _readingBuffer.Add((byte)port.ReadByte());
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
            if (_readingBuffer.Count != Cadr) return;
            var chechSum = 0;
            for (var i = 0; i < _readingBuffer.Count - 1; i++) chechSum = chechSum + _readingBuffer[i];
            if (_readingBuffer[_readingBuffer.Count - 1] != (chechSum & 0xFF))
            {
                SendQuery();
                WaitEvent.Reset();
                return;
            }

            //если сумма совпадает тогда считанные данные из буфера пишем в поле
            _data = _readingBuffer;
            _wait.Stop();
            WaitEvent.Set();
        }

        private void TWait_Elapsed(object sender, ElapsedEventArgs e)
        {
            _flagTimeout = true;
            WaitEvent.Set();
        }

        #endregion Methods
    }
}