// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using AP.Utils.Data;
using NLog;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Timers;
using System.Windows.Forms.VisualStyles;
using AP.Utils.Helps;
using ASMC.Data.Model;
using Timer = System.Timers.Timer;


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
            [DoubleValue(1)][StringValue("⁰C")] CelciumGrad,
            [DoubleValue(1)][StringValue("⁰F")] FaringeitGrad,
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

          private enum Point
        {
            None = 0,
            Point1 = 0x01,
            Point2 = 0x02,
            Point3 = 0x03,
            Point4 = 0x04
        }

        /// <summary>
        /// Режимы переключения пределов ручной/авто.
        /// </summary>
          public enum RangeSwitchMode
          {
              Manual=0x80,
              Auto=0x00
          }

          /// <summary>
          /// Позволяет получить режим переключения пределов Auto/Manual
          /// </summary>
          public RangeSwitchMode GetRangeSwitchMode
          {
              get
              {
                  
                  if (((int)GetRangeCode & (int)RangeSwitchMode.Manual) == (int)RangeSwitchMode.Manual) 
                      return RangeSwitchMode.Manual;
                  return RangeSwitchMode.Auto;
              }
          }

          /// <summary>
          /// Коды переключатля прибора
          /// </summary>
          public enum RangeCode
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
          /// Допустимые номиналы пределов измерения
          /// </summary>
          public enum RangeNominal
          {
              [StringValue("20 мВ")]Range20mV,
              [StringValue("200 мВ")]Range200mV,
              [StringValue("2 В")]Range2V, 
              [StringValue("20 В ")]Range20V, 
              [StringValue("200 В")]Range200V,
              [StringValue("750 В")]Range750V, 
              [StringValue("1000 В")]Range1000V,
              [StringValue("20 мА")]Range20mA,
              [StringValue("200 мА")]Range200mA,
              [StringValue("400 мА")]Range400mA,
              [StringValue("2 А")]Range2A,
              [StringValue("10 А")]Range10A,
              [StringValue("200 Ом")]Range200Ohm,
              [StringValue("2 кОм")]Range2kOhm,
              [StringValue("20 кОм")]Range20kOhm,
              [StringValue("200 кОм")]Range200kOhm,
              [StringValue("2 МОм")]Range2Mohm,
              [StringValue("20 МОм")]Range20Mohm,
              [StringValue("200 МОм")]Range200Mohm,
              [StringValue("2 ГОм")]Range2Gohm,
              [StringValue("4 нФ")]Range4nF,
              [StringValue("40 нФ")]Range40nF,
              [StringValue("400 нФ")]Range400nF,
              [StringValue("4 мкФ")]Range4uF,
              [StringValue("40 мкФ")]Range40uF,
              [StringValue("400 мкФ")]Range400uF,
              [StringValue("4 мФ")]Range4mF,
              [StringValue("40 мФ")]Range40mF,
              [StringValue("20 Гц")]Range20Hz,
              [StringValue("200 Гц ")]Range200Hz,
              [StringValue("2 кГц")]Range2kHz,
              [StringValue("20 кГц")]Range20kHz,
              [StringValue("200 кГц")]Range200kHz,
              [StringValue("1 МГц")]Range1MHz,
              [StringValue("400 ℃")]Range400degC, 
              [StringValue("400 ℉")]Range400degF,
              [StringValue("1200 ℃")]Range1200degC,
              [StringValue("2192 ℉")]Range2192degF,
              [StringValue("предел не установлен")]RangeNone

        }

          /// <summary>
          /// Допустимые режимы змерения
          /// </summary>
          public enum MeasureMode {
            [StringValue("Переменное напряжение")]ACV,
            [StringValue("Переменное напряжение")]ACmV,
            [StringValue("Постоянное напряжение")]DCV,
            [StringValue("Постоянное напряжение")]DCmV,
            [StringValue("Переменное ток")] ACI,
            [StringValue("Переменное ток")] ACmA,
            [StringValue("Постоянное ток")] DCI,
            [StringValue("Постоянное ток")] DCmA,
            [StringValue("Измерение переменного напряжения со смещением")]AC_DC_V,
            [StringValue("Измерение переменного напряжения со смещением")]AC_DC_mV,
            [StringValue("Измерение переменного тока со смещением")]AC_DC_I,
            [StringValue("Измерение переменного тока со смещением")]AC_DC_mA,
            [StringValue("Измерение сопртивления")]Ohm,
            [StringValue("Измерение сопротивления малым напряжением")]LowOhm,
            [StringValue("Испытание p-n переходов")]Diode,
            [StringValue("Прозвонка цепей")]Beeper,
            [StringValue("Измерение ёмкости")]Cap,
            [StringValue("Измерение частоты")]Herz,
            [StringValue("Измерение коэффициента заполнения")]DutyFactor,
            [StringValue("Измерение температуры в град. Цельсия")]degC,
            [StringValue("Измерение температуры в град. Фарингейта")]DegF,
            [StringValue("Неизвестный режим")]None
        }

         /// <summary>
         /// Возвращает информацию о текущем режиме измерения прибора
         /// </summary>
        public MeasureMode GetMeasureMode
        {
            get
            {
                BlueState currBlueState = GetBlueState;
                Rotor currRotor = GetRotor;
                

                if (currRotor == Rotor.V && currBlueState == BlueState.NoPress) return MeasureMode.ACV;
                if (currRotor == Rotor.V && currBlueState == BlueState.OnPress) return MeasureMode.DCV;
                if (currRotor == Rotor.V && currBlueState == BlueState.DoublePress) return MeasureMode.AC_DC_V;

                if (currRotor == Rotor.mV && currBlueState == BlueState.NoPress) return MeasureMode.ACmV;
                if (currRotor == Rotor.mV && currBlueState == BlueState.OnPress) return MeasureMode.DCmV;
                if (currRotor == Rotor.mV && currBlueState == BlueState.DoublePress) return MeasureMode.AC_DC_mV;

                if (currRotor == Rotor.Ohm && currBlueState == BlueState.NoPress) return MeasureMode.Ohm;
                if (currRotor == Rotor.Ohm && currBlueState == BlueState.OnPress) return MeasureMode.LowOhm;

                if (currRotor == Rotor.Diode && currBlueState == BlueState.NoPress) return MeasureMode.Diode;
                if (currRotor == Rotor.Diode && currBlueState == BlueState.OnPress) return MeasureMode.Beeper;

                if (currRotor == Rotor.mA && currBlueState == BlueState.NoPress) return MeasureMode.ACmA;
                if (currRotor == Rotor.mA && currBlueState == BlueState.OnPress) return MeasureMode.DCmA;
                if (currRotor == Rotor.mA && currBlueState == BlueState.DoublePress) return MeasureMode.AC_DC_mA;

                if (currRotor == Rotor.A && currBlueState == BlueState.NoPress) return MeasureMode.ACI;
                if (currRotor == Rotor.A && currBlueState == BlueState.OnPress) return MeasureMode.DCI;
                if (currRotor == Rotor.A && currBlueState == BlueState.DoublePress) return MeasureMode.AC_DC_I;

                if (currRotor == Rotor.Cap && currBlueState == BlueState.NoPress) return MeasureMode.Cap;

                if (currRotor == Rotor.Hz && currBlueState == BlueState.NoPress) return MeasureMode.Herz;
                if (currRotor == Rotor.Hz && currBlueState == BlueState.OnPress) return MeasureMode.DutyFactor;

                if (currRotor == Rotor.Temp && currBlueState == BlueState.NoPress) return MeasureMode.degC;
                if (currRotor == Rotor.Temp && currBlueState == BlueState.OnPress) return MeasureMode.DegF;

                return MeasureMode.None;

            }
            
        }

         /// <summary>
         /// Возвращает номинал текущего предела измерения
         /// </summary>
         public RangeNominal GetRangeNominal
         {
             get
             {
                 MeasureMode currMode = GetMeasureMode;
                 RangeCode currRangeCode = GetRangeCode;

                 if (currMode == MeasureMode.DCV)
                 {
                    if (((int)currRangeCode & 3) == 3) return RangeNominal.Range1000V;
                    if (((int)currRangeCode & 2) == 2) return RangeNominal.Range200V;
                    if (((int)currRangeCode & 1) == 1) return RangeNominal.Range20V;
                    return RangeNominal.Range2V;
                 }

                 if (currMode == MeasureMode.ACV)
                 {
                    if (((int)currRangeCode & 3) == 3) return RangeNominal.Range750V;
                    if (((int)currRangeCode & 2) == 2) return RangeNominal.Range200V;
                    if (((int)currRangeCode & 1) == 1) return RangeNominal.Range20V;
                    return RangeNominal.Range2V;
                 }

                 if (currMode == MeasureMode.AC_DC_V)
                 {
                    if (((int)currRangeCode & 3) == 3) return RangeNominal.Range750V;
                    if (((int)currRangeCode & 2) == 2) return RangeNominal.Range200V;
                    if (((int)currRangeCode & 1) == 1) return RangeNominal.Range20V;
                    return RangeNominal.Range2V;
                 }
                 
                 if (currMode == MeasureMode.DCmV || currMode == MeasureMode.ACmV || currMode == MeasureMode.AC_DC_mV)
                 {
                    
                     if (((int) currRangeCode & 1) == 1) return RangeNominal.Range200mV;
                     return RangeNominal.Range20mV;
                 }
                 
                
                if (currMode == MeasureMode.DCmA || currMode == MeasureMode.ACmA || currMode == MeasureMode.AC_DC_mA)
                {
                    
                    if (((int) currRangeCode & 1) == 1) return RangeNominal.Range200mA;
                    return RangeNominal.Range20mA;
                }
                
                if (currMode == MeasureMode.DCI || currMode == MeasureMode.ACI || currMode == MeasureMode.AC_DC_I)
                {
                    
                    if (((int)currRangeCode & 1) == 1) return RangeNominal.Range10A;
                    return RangeNominal.Range2A;
                }
                
                if (currMode == MeasureMode.Ohm)
                {
                    if (((int)currRangeCode & 7) == 7) return RangeNominal.Range2Gohm;
                    if (((int)currRangeCode & 6) == 6) return RangeNominal.Range200Mohm;
                    if (((int)currRangeCode & 5) == 5) return RangeNominal.Range20Mohm;
                    if (((int)currRangeCode & 4) == 4) return RangeNominal.Range2Mohm;
                    if (((int)currRangeCode & 3) == 3) return RangeNominal.Range200kOhm;
                    if (((int)currRangeCode & 1)== 1) return RangeNominal.Range2kOhm;
                    if (((int)currRangeCode & 2) == 2) return RangeNominal.Range20kOhm;
                    return RangeNominal.Range200Ohm;
                }

                
                if (currMode == MeasureMode.LowOhm)
                {

                    if (((int)currRangeCode & 6) == 6) return RangeNominal.Range2Gohm;
                    if (((int)currRangeCode & 5) == 5) return RangeNominal.Range200Mohm;
                    if (((int)currRangeCode & 4) == 4) return RangeNominal.Range20Mohm;
                    if (((int)currRangeCode & 3) == 3) return RangeNominal.Range2Mohm;
                    if (((int)currRangeCode & 2) == 2) return RangeNominal.Range200kOhm;
                    if (((int)currRangeCode & 1) == 1) return RangeNominal.Range20kOhm;
                    return RangeNominal.Range2kOhm;
                }
               
                if (currMode == MeasureMode.Cap)
                {
                    if (((int)currRangeCode & 7) == 7) return RangeNominal.Range40mF;
                    if (((int)currRangeCode & 6) == 6) return RangeNominal.Range4mF;
                    if (((int)currRangeCode & 5) == 5) return RangeNominal.Range400uF;
                    if (((int)currRangeCode & 4) == 4) return RangeNominal.Range40uF;
                    if (((int)currRangeCode & 3) == 3) return RangeNominal.Range4uF;
                    if (((int)currRangeCode & 2) == 2) return RangeNominal.Range400nF;
                    if (((int)currRangeCode & 1) == 1) return RangeNominal.Range40nF;
                    return RangeNominal.Range4nF;
                }
                
                
                if (currMode == MeasureMode.Herz)
                {
                    if (((int)currRangeCode & 5) == 5) return RangeNominal.Range1MHz;
                    if (((int)currRangeCode & 4) == 4) return RangeNominal.Range200kHz;
                    if (((int)currRangeCode & 3) == 3) return RangeNominal.Range20kHz;
                    if (((int)currRangeCode & 2) == 2) return RangeNominal.Range2kHz;
                    if (((int)currRangeCode & 1) == 1) return RangeNominal.Range200Hz;
                    return RangeNominal.Range20Hz;
                }
                

                if (currMode == MeasureMode.degC)
                {
                    if (((int)currRangeCode & 1) == 1) return RangeNominal.Range1200degC;
                    return RangeNominal.Range400degC;
                }
                

                if (currMode == MeasureMode.DegF)
                {
                    if (((int)currRangeCode & 1) == 1) return RangeNominal.Range2192degF;
                    return RangeNominal.Range400degF;

                }

                

                return RangeNominal.RangeNone;
             }
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
        private readonly byte[] _readData = { 0x55, 0x55, 0x00, 0x0E};


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
                Logger.Debug($"Единицы измерени на основном экране {((Units)(_data[11] >> 3)).ToString()}");
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
                SendQuery();
                Logger.Debug($"Единицы измерения на втором экране {((Units)(_data[16] >> 3)).ToString()}");
                return (Units)(_data[16] >> 3);
            }
        }

        public BlueState GetBlueState
        {
            get
            {
                SendQuery();
                Logger.Debug($"Статус синей клавиши {UserType} { ((BlueState)_data[5]).ToString() }");
                return (BlueState)_data[5];
            }
        }

        /// <summary>
        /// Позволяет получить статус включенных дополнительных функций прибора
        /// </summary>
        public Function GetGeneralFunction
        {
            get
            {
                SendQuery();
                Logger.Info($"{UserType} дополнительная функция {((Function)_data[12]).ToString()}");
                return (Function)_data[12];
            }
        }

        /// <summary>
        /// Возвращает среднее арифметическое после countOfMeasure измерений. Исключает из выборки выбросы.
        /// </summary>
        /// <param name="countOfMeasure">Число необходимых измерений.</param>
        /// <param name="mult">Множитель единицы измерений (миллиб кило и т.д.)</param>
        /// <param name="generalDsiplay">Если флаг true, тогда возвращаются показания с основного экрана прибора. Иначе с второстепенного.</param>
        /// <returns>Среднее арифметическое на основе выборки из countOfMeasure измерений.</returns>
        public double GetValue(int countOfMeasure = 10, Multipliers mult = AP.Utils.Helps.Multipliers.None, bool generalDsiplay = true)
        {
            
            
            double resultValue;

            decimal[] valBuffer = new decimal[countOfMeasure];
            do
            {
                for (int i = 0; i < valBuffer.Length; i++)
                    valBuffer[i] = (decimal)GetSingleValue(mult, generalDsiplay);
            } while ( !AP.Math.MathStatistics.IntoTreeSigma(valBuffer) );
           

            AP.Math.MathStatistics.Grubbs(ref valBuffer);
            resultValue = (double)AP.Math.MathStatistics.GetArithmeticalMean(valBuffer);

            return resultValue;
        }

        /// <summary>
        /// Возвращает одно измеренное значение с прибора, приведенное к единицам в соответствии с множителем mult
        /// </summary>
        /// <param name="mult">Множитель единицы измерения.</param>
        /// <param name="generalDsiplay">Если флаг true, тогда возвращаются показания с основного экрана прибора. Иначе с второстепенного.</param>
        /// <returns></returns>
        public double GetSingleValue(Multipliers mult  = AP.Utils.Helps.Multipliers.None, bool generalDsiplay = true)
        {
            SendQuery();
            double value;
            
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


            Logger.Info($"{UserType} Измеренное значение {value}");
            return value;
        }

        

        public RangeCode GetRangeCode
        {
            get
            {
                SendQuery();
                Logger.Info($"{UserType} диапазон измерения {((RangeCode)_data[7]).ToString()}");
                 return (RangeCode)_data[7];
                
            }
        }

        public Rotor GetRotor
        {
            get
            {
                SendQuery();
                Logger.Info($"{UserType} положение переключателя {((Rotor)_data[4]).ToString()}");
                return (Rotor)_data[4];
            }
        }

        /// <summary>
        /// Позволяет получить дополнительные функции, включенные на дополнительном экране.
        /// </summary>
        public Function GetSubFunction
        {
            get
            {
                SendQuery();
                Logger.Info($"{UserType} дополнительные функции на втором экране {((Function)_data[17]).ToString()}");
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
            //this.StringConnection = null;
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

            WaitEvent.WaitOne();
            if (_flagTimeout)
            {
                _flagTimeout = false;
                Logger.Debug($"{ UserType} не отвечает.");
                throw new TimeoutException($"{UserType} не отвечает.");
            }

        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) _wait?.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Получает данные с прибора, записывает их в буфер для проверки контрольной суммы.
        /// </summary>
        /// <param name = "sender">Устройство передающее данные</param>
        /// <param name = "e"></param>
        protected override void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int CountResend = 0;
            _readingBuffer.Clear();
            var port = (SerialPort)sender;
            try
            {
                for (int i=0; i< _readData.Length;i++)
                {
                    var bt = (byte)port.ReadByte();
                    if (bt == _readData[i])_readingBuffer.Add(bt);
                    else
                    {
                        DiscardInBuffer();
                        _readingBuffer.Clear();
                        CountResend++;
                        SendQuery();
                        if (CountResend == 5)
                        {
                            Logger.Error($"Было выполнено 5 неудачных попыток запроса данных с {UserType}");
                            throw new TimeoutException($"Было выполнено 5 неудачных попыток запроса данных с {UserType}");

                        }

                        i = -1;
                    }
                    
                }
                
                while (_readingBuffer.Count < Cadr)
                {
                    _readingBuffer.Add((byte)port.ReadByte());
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
            if (_readingBuffer.Count != Cadr) return;
            var chechSum = 0;
            for (var i = 0; i < _readingBuffer.Count - 1; i++) chechSum = chechSum + _readingBuffer[i];

            /*берем последние биты*/
            var match = chechSum & 0xFF;
            Logger.Debug($"{this.StringConnection}:  Контрольная сумма считанных данных: {match.ToString("X")} а ожидается {_readingBuffer[_readingBuffer.Count - 1].ToString("X")}");
            if ((match != _readingBuffer[_readingBuffer.Count - 1]))
            {
                WaitEvent.Reset();
                SendQuery();
                return;
            }

            //for (var i = 0; i < _readingBuffer.Count - 2; i++) chechSum = chechSum + _readingBuffer[i];
            //if (_readingBuffer[_readingBuffer.Count - 1] != (chechSum & 0xFF))
            //{
            //    SendQuery();
            //    WaitEvent.Reset();
            //    return;
            //}

           

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

    public class MultAPPA107N : Mult107_109N
    {
        public MultAPPA107N()
        {
            UserType = "APPA-107N";
        }
    }

    public class MultAPPA109N : Mult107_109N
    {
        public MultAPPA109N()
        {
            UserType = "APPA-109N";
        }
    }
}