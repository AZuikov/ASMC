using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace ASMC.Devices.Port.ZipNu4Pribor
{
    public class Km300P : ComPort
    {
        /// <summary>
        /// Входы компаратора.
        /// </summary>
        public enum MeasureInputVolt
        {
            InputU1 = 0x00,
            InputU2 = 0x08
        }

        [Flags]
        public enum MeasureMode
        {
            /// <summary>
            /// Предел измерения 100мв.
            /// </summary>
            Range100mV = 0x0,

            /// <summary>
            /// Предел измерения 1в.
            /// </summary>
            Range1V = 0x01,

            /// <summary>
            /// Предел измерения 10в.
            /// </summary>
            Range10V = 0x02,

            /// <summary>
            /// Предел измерения 100в.
            /// </summary>
            Range100V = 0x03,

            /// <summary>
            /// Предел измерения 1000в.
            /// </summary>
            Range1000V = 0x04,

            /// <summary>
            /// Режим измерения разности напряжений между входом U1 и U2.
            /// </summary>
            IsDeltaVoltMeasure = 0x10,

            //[DoubleValue(4)] DeltaVoltMeasureOff = 0x00,
            /// <summary>
            /// Автоматический режим измерения.
            /// </summary>
            IsAutoMeasureMode = 0x20,

            /// <summary>
            /// Ручной  режим измерения.
            /// </summary>
            //[DoubleValue(5)] ManualMeasureMode = 0x0,
            /// <summary>
            /// Прибор может ответить что значение выше предела измерения.
            /// </summary>
            Overload = 0x80
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly AutoResetEvent WaitEvent = new AutoResetEvent(false);

        #region Fields

        //таймер
        private readonly Timer _wait;

        /// <summary>
        /// Адрес устройства по умолчанию.
        /// </summary>
        private readonly byte adress = 1;

        private bool _flagTimeout;

        #endregion Fields

        //SpeedRate baudRate = SpeedRate.R57600;
        //Parity parity = Parity.None;
        //DBit dataBit = DBit.Bit8;
        //StopBits stopBit = StopBits.One;

        public Km300P()
        {
            UserType = "КМ300Р";
            BaudRate = SpeedRate.R57600;

            _wait = new Timer();
            _wait.Interval = 35000;
            _wait.Elapsed += TWait_Elapsed;
            _wait.AutoReset = false;
        }

        #region Methods

        public override void Dispose()
        {
            _wait?.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Получить измеренное значение.
        /// </summary>
        public void GetMeasureValue()
        {
            
            
            Sincronization();// спамим тест-обмен
            SendQuery(new byte[] { 0x03, 0x44, 0x03, 0x21 });//получаем измеренное значение
            
        }

        /// <summary>
        /// Включение режима измерения.
        /// </summary>
        public void MeasureModeOn()
        {
            WriteData(new byte[] { 0x03, 0x44, 0x03, 0x20 }, false);//включаем режим измерения
            
        }

        /// <summary>
        /// Сброс прибора.
        /// </summary>
        public void Reset()
        {
            WriteData(new byte[] { 0x03, 0x44, 0x03, 0x01 });
        }

        /// <summary>
        /// Посылает запрос. Программа ждет ответ.
        /// </summary>
        /// <param name="sendThisArr"></param>
        public void SendQuery(byte[] sendThisArr)
        {
            var resultCRC = CRCUtilsKM300.CalcCRCforKm300(sendThisArr);
            sendThisArr = new[] { adress }.Concat(sendThisArr).ToArray();
            sendThisArr = sendThisArr.Concat(new[] { resultCRC }).ToArray();

            
            Open();
            //отправили команду запроса
            Write(sendThisArr, 0, sendThisArr.Length);
            _wait.Start();
            

            WaitEvent.WaitOne();
            if (_flagTimeout)
            {
                _flagTimeout = false;
                var errorMessage = $"{UserType} не отвечает.";
                Logger.Debug(errorMessage);
                throw new TimeoutException(errorMessage);
            }
        }

        /// <summary>
        /// Устанавливает напряжение на выходе калибратора.
        /// </summary>
        /// <param name = "inVolt">Напряжение в вольтах на выходе калибратора.</param>
        public void SetOutVoltOnCalibrator(decimal inVolt)
        {
            var valueArr = CRCUtilsKM300.ConvertValueToBcdCode((double)Math.Abs(inVolt));
            byte[] resultByteArr = { 0x0A, 0x44, 0x03, 0x10 };
            resultByteArr = resultByteArr.Concat(valueArr).ToArray();

            if (inVolt > 0)
                resultByteArr = resultByteArr.Concat(new byte[] { 0x02, 0x03, 0xe8 }).ToArray();
            else
                //Если значение должно быть со знаком минус, тогда берем 0x07
                resultByteArr = resultByteArr.Concat(new byte[] { 0x07, 0x03, 0xe8 }).ToArray();

            WriteData(resultByteArr);
        }

        /// <summary>
        /// Задает текущий вход для измерения (вход U1 или U2).
        /// </summary>
        public void SetVoltInput(MeasureInputVolt input)
        {
            byte[] resultByteArr = { 0x04, 0x44, 0x03, 0x24 };
            if (input == MeasureInputVolt.InputU1)
                resultByteArr = resultByteArr.Concat(new byte[] { 0x20 }).ToArray();
            else
                resultByteArr = resultByteArr.Concat(new byte[] { 0x28 }).ToArray();

            WriteData(resultByteArr);
        }

        private List<byte> _readData = new List<byte>();

        /// <summary>
        /// Метод выполняет функцию "Тест-Обмен"а из руководства на калибратор.
        /// </summary>
        protected void Sincronization()
        {
            byte[] byteToWrite = new byte[] {0x03, 0x44, 0x03, 0x08};
            var resultCRC = CRCUtilsKM300.CalcCRCforKm300(byteToWrite);
            byteToWrite = new[] { adress }.Concat(byteToWrite).ToArray();
            byteToWrite = byteToWrite.Concat(new[] { resultCRC }).ToArray();

            
            while (true)
            {
                Open();
                Write(byteToWrite,0,byteToWrite.Length);
                Thread.Sleep(500);
                if (_readData.Count > 3) 
                    break;
            }
            Close();
            byte[] readBytes = _readData.ToArray();
        }

        /// <summary>
        /// Запись команды в виде массива байт.
        /// </summary>
        /// <param name="byteToWrite"></param>
        /// /// <param name="closePort">Закрывает порт после записи, если true.</param>
        public void WriteData(byte[] byteToWrite, bool closePort =true)
        {
            var resultCRC = CRCUtilsKM300.CalcCRCforKm300(byteToWrite);
            byteToWrite = new[] { adress }.Concat(byteToWrite).ToArray();
            byteToWrite = byteToWrite.Concat(new[] { resultCRC }).ToArray();

            Open();
            Write(byteToWrite, 0, byteToWrite.Length);
            if (closePort)  Close();
        }

        protected override void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _readData.Clear();
            try
            {
                //будем делать
                for (int j = 0; j<50; j++)
                {
                    var buffer1 = new byte[32];
                    ReadByte(buffer1, 0, buffer1.Length, false);

                    
                    //byte[] fullByteBlock = buffer1.Concat(buff2).ToArray();
                    ////проверка сходимости контрольной суммы
                    //byte[] buf3 = new byte[buff2.Length - 1];//создаем массив что бы отбросить CRC  сумму
                    //Array.Copy(buff2, buf3, buff2.Length - 1); //отбрасываем CRC сумму
                    //if (fullByteBlock[fullByteBlock.Length - 1] == CRCUtilsKM300.CalcCRCforKm300(buf3))
                    //{//если контрольная сумма совпадает то все хорошо
                    //    _readData.AddRange(fullByteBlock);
                    //    _wait.Stop();
                    //    WaitEvent.Set();

                    //    DiscardInBuffer();
                    //    Close();
                    //    return;
                    //}

                   
                }

                throw new DataException($"{UserType}: После 10 попыток считывания данные не поступили.");

            }
            catch (Exception a)
            {
                Logger.Error(a);
            }
            //finally
            //{
            //    Close();
            //}
        }

        private void TWait_Elapsed(object sender, ElapsedEventArgs e)
        {
            _flagTimeout = true;
            WaitEvent.Set();
        }

        #endregion Methods
    }

    internal static class CRCUtilsKM300
    {
        #region Methods

        /// <summary>
        /// Преобразует число в массив байт, согласно принципу BCD кода.
        /// </summary>
        /// <param name = "inVal">Число для преобразования.</param>
        /// <returns></returns>
        public static byte[] ConvertValueToBcdCode(double inVal)
        {
            //округляем до 6 знаков после запятой
            inVal = Math.Round(inVal, 6);

            var Valstr = inVal.ToString();
            Valstr = Valstr.Replace(',', '.');
            var splitStr = Valstr.Split('.');
            //массив может быть с одним элементом!!!!
            if (splitStr[0].Length < 2) splitStr[0] = "0" + splitStr[0];

            if (splitStr.Length == 2)
                Valstr = splitStr[0] + splitStr[1];
            else
                Valstr = splitStr[0];

            if (Valstr.IndexOf('.') > 0) Valstr = Valstr.Remove(Valstr.IndexOf('.'), 1);

            if (Valstr.Length < 8)
                do
                {
                    Valstr += "0";
                } while (Valstr.Length != 8);

            //переводим число в bcd
            int value;
            int.TryParse(Valstr, out value);
            var bcd = 0;
            //переводим в bcd каждую цифру
            for (var i = 0; i < Valstr.Length; i++)
            {
                var nibble = value % 10;
                bcd |= nibble << (i * 4);
                value /= 10;
            }

            return new[]
            {
                (byte) ((bcd >> 24) & 0xff), (byte) ((bcd >> 16) & 0xff), (byte) ((bcd >> 8) & 0xff),
                (byte) (bcd & 0xff)
            };
        }

        /// <summary>
        /// Вычисление CRC суммы для КМ300.
        /// </summary>
        /// <param name = "inArr">массив байт для обработки.</param>
        /// <returns></returns>
        public static byte CalcCRCforKm300(byte[] inArr)
        {
            byte CRC = 0;
            byte parity = 0;

            for (var i = 0; i < inArr.Length; i++)
            {
                CRC = CrCTable((byte)(CRC ^ inArr[i]));
                if (!DetectByteParity(inArr[i])) parity = (byte)(parity ^ 0x80);
            }

            CRC = (byte)(CRC >> 1);
            CRC = (byte)(CRC | parity);
            CRC = (byte)~CRC;

            return CRC;
        }

        /// <summary>
        /// Функция определения четности бита.
        /// </summary>
        /// <param name = "inByte">Байт для проверки.</param>
        /// <returns></returns>
        public static bool DetectByteParity(byte inByte)
        {
            var bitArr = new BitArray(new[] { inByte });

            byte m = 0;
            foreach (bool bit in bitArr)
                if (bit)
                    m++;

            return m % 2 == 0;
        }

        public static byte CrCTable(byte inByte)
        {
            switch (inByte)
            {
                case 0x00: return 0x00;
                case 0x01: return 0xca;
                case 0x02: return 0x5e;
                case 0x03: return 0x94;

                case 0x04: return 0xbc;
                case 0x05: return 0x76;
                case 0x06: return 0xe2;
                case 0x07: return 0x28;

                case 0x08: return 0xb2;
                case 0x09: return 0x78;
                case 0x0a: return 0xec;
                case 0x0b: return 0x26;

                case 0x0c: return 0x0e;
                case 0x0d: return 0xc4;
                case 0x0e: return 0x50;
                case 0x0f: return 0x9a;

                case 0x10: return 0xae;
                case 0x11: return 0x64;
                case 0x12: return 0xf0;
                case 0x13: return 0x3a;

                case 0x14: return 0x12;
                case 0x15: return 0xd8;
                case 0x16: return 0x4c;
                case 0x17: return 0x86;

                case 0x18: return 0x1c;
                case 0x19: return 0xd6;
                case 0x1a: return 0x42;
                case 0x1b: return 0x88;

                case 0x1c: return 0xa0;
                case 0x1d: return 0x6a;
                case 0x1e: return 0xfe;
                case 0x1f: return 0x34;

                case 0x20: return 0x96;
                case 0x21: return 0x5c;
                case 0x22: return 0xc8;
                case 0x23: return 0x02;

                case 0x24: return 0x2a;
                case 0x25: return 0xe0;
                case 0x26: return 0x74;
                case 0x27: return 0xbe;

                case 0x28: return 0x24;
                case 0x29: return 0xee;
                case 0x2a: return 0x7a;
                case 0x2b: return 0xb0;

                case 0x2c: return 0x98;
                case 0x2d: return 0x52;
                case 0x2e: return 0xc6;
                case 0x2f: return 0x0c;

                case 0x30: return 0x38;
                case 0x31: return 0xf2;
                case 0x32: return 0x66;
                case 0x33: return 0xac;

                case 0x34: return 0x84;
                case 0x35: return 0x4e;
                case 0x36: return 0xda;
                case 0x37: return 0x10;

                case 0x38: return 0x8a;
                case 0x39: return 0x40;
                case 0x3a: return 0xd4;
                case 0x3b: return 0x1e;

                case 0x3c: return 0x36;
                case 0x3d: return 0xfc;
                case 0x3e: return 0x68;
                case 0x3f: return 0xa2;

                case 0x40: return 0xe6;
                case 0x41: return 0x2c;
                case 0x42: return 0xb8;
                case 0x43: return 0x72;

                case 0x44: return 0x5a;
                case 0x45: return 0x90;
                case 0x46: return 0x04;
                case 0x47: return 0xce;

                case 0x48: return 0x54;
                case 0x49: return 0x9e;
                case 0x4a: return 0x0a;
                case 0x4b: return 0xc0;

                case 0x4c: return 0xe8;
                case 0x4d: return 0x22;
                case 0x4e: return 0xb6;
                case 0x4f: return 0x7c;

                case 0x50: return 0x48;
                case 0x51: return 0x82;
                case 0x52: return 0x16;
                case 0x53: return 0xdc;

                case 0x54: return 0xf4;
                case 0x55: return 0x3e;
                case 0x56: return 0xaa;
                case 0x57: return 0x60;

                case 0x58: return 0xfa;
                case 0x59: return 0x30;
                case 0x5a: return 0xa4;
                case 0x5b: return 0x6e;

                case 0x5c: return 0x46;
                case 0x5d: return 0x8c;
                case 0x5e: return 0x18;
                case 0x5f: return 0xd2;

                case 0x60: return 0x70;
                case 0x61: return 0xba;
                case 0x62: return 0x2e;
                case 0x63: return 0xe4;

                case 0x64: return 0xcc;
                case 0x65: return 0x06;
                case 0x66: return 0x92;
                case 0x67: return 0x58;

                case 0x68: return 0xc2;
                case 0x69: return 0x08;
                case 0x6a: return 0x9c;
                case 0x6b: return 0x56;

                case 0x6c: return 0x7e;
                case 0x6d: return 0xb4;
                case 0x6e: return 0x20;
                case 0x6f: return 0xea;

                case 0x70: return 0xde;
                case 0x71: return 0x14;
                case 0x72: return 0x80;
                case 0x73: return 0x4a;

                case 0x74: return 0x62;
                case 0x75: return 0xa8;
                case 0x76: return 0x3c;
                case 0x77: return 0xf6;

                case 0x78: return 0x6c;
                case 0x79: return 0xa6;
                case 0x7a: return 0x32;
                case 0x7b: return 0xf8;

                case 0x7c: return 0xd0;
                case 0x7d: return 0x1a;
                case 0x7e: return 0x8e;
                case 0x7f: return 0x44;

                case 0x80: return 0x06;
                case 0x81: return 0xcc;
                case 0x82: return 0x58;
                case 0x83: return 0x92;

                case 0x84: return 0xba;
                case 0x85: return 0x70;
                case 0x86: return 0xe4;
                case 0x87: return 0x2e;

                case 0x88: return 0xb4;
                case 0x89: return 0x7e;
                case 0x8a: return 0xea;
                case 0x8b: return 0x20;

                case 0x8c: return 0x08;
                case 0x8d: return 0xc2;
                case 0x8e: return 0x56;
                case 0x8f: return 0x9c;

                case 0x90: return 0xa8;
                case 0x91: return 0x62;
                case 0x92: return 0xf6;
                case 0x93: return 0x3c;

                case 0x94: return 0x14;
                case 0x95: return 0xde;
                case 0x96: return 0x4a;
                case 0x97: return 0x80;

                case 0x98: return 0x1a;
                case 0x99: return 0xd0;
                case 0x9a: return 0x44;
                case 0x9b: return 0x8e;

                case 0x9c: return 0xa6;
                case 0x9d: return 0x6c;
                case 0x9e: return 0xf8;
                case 0x9f: return 0x32;

                case 0xa0: return 0x90;
                case 0xa1: return 0x5a;
                case 0xa2: return 0xce;
                case 0xa3: return 0x04;

                case 0xa4: return 0x2c;
                case 0xa5: return 0xe6;
                case 0xa6: return 0x72;
                case 0xa7: return 0xb8;

                case 0xa8: return 0x22;
                case 0xa9: return 0xe8;
                case 0xaa: return 0x7c;
                case 0xab: return 0xb6;

                case 0xac: return 0x9e;
                case 0xad: return 0x54;
                case 0xae: return 0xc0;
                case 0xaf: return 0x0a;

                case 0xb0: return 0x3e;
                case 0xb1: return 0xf4;
                case 0xb2: return 0x60;
                case 0xb3: return 0xaa;

                case 0xb4: return 0x82;
                case 0xb5: return 0x48;
                case 0xb6: return 0xdc;
                case 0xb7: return 0x16;

                case 0xb8: return 0x8c;
                case 0xb9: return 0x46;
                case 0xba: return 0xd2;
                case 0xbb: return 0x18;

                case 0xbc: return 0x30;
                case 0xbd: return 0xfa;
                case 0xbe: return 0x6e;
                case 0xbf: return 0xa4;

                case 0xc0: return 0xe0;
                case 0xc1: return 0x2a;
                case 0xc2: return 0xbe;
                case 0xc3: return 0x74;

                case 0xc4: return 0x5c;
                case 0xc5: return 0x96;
                case 0xc6: return 0x02;
                case 0xc7: return 0xc8;

                case 0xc8: return 0x52;
                case 0xc9: return 0x98;
                case 0xca: return 0x0c;
                case 0xcb: return 0xc6;

                case 0xcc: return 0xee;
                case 0xcd: return 0x24;
                case 0xce: return 0xb0;
                case 0xcf: return 0x7a;

                case 0xd0: return 0x4e;
                case 0xd1: return 0x84;
                case 0xd2: return 0x10;
                case 0xd3: return 0xda;

                case 0xd4: return 0xf2;
                case 0xd5: return 0x38;
                case 0xd6: return 0xac;
                case 0xd7: return 0x66;

                case 0xd8: return 0xfc;
                case 0xd9: return 0x36;
                case 0xda: return 0xa2;
                case 0xdb: return 0x68;

                case 0xdc: return 0x40;
                case 0xdd: return 0x8a;
                case 0xde: return 0x1e;
                case 0xdf: return 0xd4;

                case 0xe0: return 0x76;
                case 0xe1: return 0xbc;
                case 0xe2: return 0x28;
                case 0xe3: return 0xe2;

                case 0xe4: return 0xca;
                case 0xe5: return 0x00;
                case 0xe6: return 0x94;
                case 0xe7: return 0x5e;

                case 0xe8: return 0xc4;
                case 0xe9: return 0x0e;
                case 0xea: return 0x9a;
                case 0xeb: return 0x50;

                case 0xec: return 0x78;
                case 0xed: return 0xb2;
                case 0xee: return 0x26;
                case 0xef: return 0xec;

                case 0xf0: return 0xd8;
                case 0xf1: return 0x12;
                case 0xf2: return 0x86;
                case 0xf3: return 0x4c;

                case 0xf4: return 0x64;
                case 0xf5: return 0xae;
                case 0xf6: return 0x3a;
                case 0xf7: return 0xf0;

                case 0xf8: return 0x6a;
                case 0xf9: return 0xa0;
                case 0xfa: return 0x34;
                case 0xfb: return 0xfe;

                case 0xfc: return 0xd6;
                case 0xfd: return 0x1c;
                case 0xfe: return 0x88;
                case 0xff: return 0x42;
            }

            return 0;
        }

        #endregion Methods
    }
}