// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Globalization;
using System.Threading;
using AP.Utils.Data;
using AP.Utils.Helps;
using MathNet.Numerics.Statistics;

namespace ASMC.Devices.IEEE.Tektronix.Oscilloscope
{
    public class TDS_Oscilloscope : IeeeBase
    {
        /// <summary>
        /// Перечень каналов
        /// </summary>
        public enum ChanelSet
        {
            /// <summary>
            /// Канал 1
            /// </summary>
            CH1 = 1,

            /// <summary>
            /// Канал 2
            /// </summary>
            CH2 = 2,

            /// <summary>
            /// Канал 3
            /// </summary>
            CH3 = 3,

            /// <summary>
            /// Канал 4
            /// </summary>
            CH4 = 4
        }

       
        /// <summary>
        /// Режимы работы канала
        /// </summary>
        public enum COUPling
        {
            /// <summary>
            /// Только АС
            /// </summary>
            AC,

            /// <summary>
            /// AC + DC
            /// </summary>
            DC,

            /// <summary>
            /// Земля
            /// </summary>
            GND
        }

        /// <summary>
        /// Допустимые значения развертки по времени.
        /// </summary>
        public enum HorisontalSCAle
        {
            /// <summary>
            /// 2.5 нс
            /// </summary>
            [StringValue("2.5E-9")] Scal_2_5nSec,

            /// <summary>
            /// 5 нс
            /// </summary>
            [StringValue("5E-9")] Scal_5nSec,

            /// <summary>
            /// 10 нс
            /// </summary>
            [StringValue("10E-9")] Scal_10nSec,

            /// <summary>
            /// 25 нс
            /// </summary>
            [StringValue("25E-9")] Scal_25nSec,

            /// <summary>
            /// 50 нс
            /// </summary>
            [StringValue("50E-9")] Scal_50nSec,

            /// <summary>
            /// 100 нс
            /// </summary>
            [StringValue("10E-8")] Scal_100nSec,

            /// <summary>
            /// 250 нс
            /// </summary>
            [StringValue("25E-8")] Scal_250nSec,

            /// <summary>
            /// 500 нс
            /// </summary>
            [StringValue("50E-8")] Scal_500nSec,

            /// <summary>
            /// 1 мкс
            /// </summary>
            [StringValue("1E-6")] Scal_1mkSec,

            /// <summary>
            /// 2.5 мкс
            /// </summary>
            [StringValue("2.5E-6")] Scal_2_5mkSec,

            /// <summary>
            /// 5 мкс
            /// </summary>
            [StringValue("5E-6")] Scal_5mkSec,

            /// <summary>
            /// 10 мкс
            /// </summary>
            [StringValue("10E-6")] Scal_10mkSec,

            /// <summary>
            /// 25 мкс
            /// </summary>
            [StringValue("25E-6")] Scal_25mkSec,

            /// <summary>
            /// 50 мкс
            /// </summary>
            [StringValue("50E-6")] Scal_50mkSec,

            /// <summary>
            /// 100 мкс
            /// </summary>
            [StringValue("100E-6")] Scal_100mkSec,

            /// <summary>
            /// 250 мкс
            /// </summary>
            [StringValue("250E-6")] Scal_250mkSec,

            /// <summary>
            /// 500 мкс
            /// </summary>
            [StringValue("500E-6")] Scal_500mkSec,

            /// <summary>
            /// 1 мс
            /// </summary>
            [StringValue("1E-3")] Scal_1mSec,

            /// <summary>
            /// 2.5 мс
            /// </summary>
            [StringValue("2.5E-3")] Scal_2_5mSec,

            /// <summary>
            /// 5 мс
            /// </summary>
            [StringValue("5E-3")] Scal_5mSec,

            /// <summary>
            /// 10 мс
            /// </summary>
            [StringValue("10E-3")] Scal_10mSec,

            /// <summary>
            /// 25 мс
            /// </summary>
            [StringValue("25E-3")] Scal_mSec,

            /// <summary>
            /// 50 мс
            /// </summary>
            [StringValue("50E-3")] Scal_50mSec,

            /// <summary>
            /// 100 мс
            /// </summary>
            [StringValue("100E-3")] Scal_100mSec,

            /// <summary>
            /// 250 мс
            /// </summary>
            [StringValue("250E-3")] Scal_250mSec,

            /// <summary>
            /// 500 мс
            /// </summary>
            [StringValue("500E-3")] Scal_500mSec,

            /// <summary>
            /// 1 с
            /// </summary>
            [StringValue("1")] Scal_1Sec,

            /// <summary>
            /// 2.5 с
            /// </summary>
            [StringValue("2.5")] Scal_2_5Sec,

            /// <summary>
            /// 5 с
            /// </summary>
            [StringValue("5")] Scal_5Sec,

            /// <summary>
            /// 10 с
            /// </summary>
            [StringValue("10")] Scal_10Sec,

            /// <summary>
            /// 25 с
            /// </summary>
            [StringValue("25")] Scal_25Sec,

            /// <summary>
            /// 50 с
            /// </summary>
            [StringValue("50")] Scal_50Sec
        }

        /// <summary>
        /// Режим сбора данных
        /// </summary>
        public enum MiscellaneousMode
        {
            SAMple,

            /// <summary>
            /// The pea kdetect
            /// </summary>
            PEAKdetect,

            /// <summary>
            /// Усреднение
            /// </summary>
            AVErage
        }

        /// <summary>
        /// Количество накоплений данных при усреднении
        /// </summary>
        public enum MiscellaneousNUMAV
        {
            Number_4 = 4,
            Number_16 = 16,
            Number_64 = 64,
            Number_128 = 128
        }

        /// <summary>
        /// ПО каким параметрам необходимо выбирать автопредел
        /// </summary>
        public enum MiscellaneousSetting
        {
            /// <summary>
            /// Развертка
            /// </summary>
            HORizontal,

            /// <summary>
            /// Коэфициент отклонения
            /// </summary>
            VERTical,

            /// <summary>
            /// По Х и Y, Значение по умолчанию
            /// </summary>
            BOTH
        }

        /// <summary>
        /// Значения внешнего делителя в режиме напряжения
        /// </summary>
        public enum Probe
        {
            Att_1 = 1,
            Att_10 = 10,
            Att_20 = 20,
            Att_50 = 50,
            Att_100 = 100,
            Att_500 = 500,
            Att_1000 = 1000
        }

        /// <summary>
        /// Состояние
        /// </summary>
        public enum State
        {
            /// <summary>
            /// Выключить
            /// </summary>
            [StringValue("OFF")]OFF,

            /// <summary>
            /// Включить
            /// </summary>
            [StringValue("ON")]ON
        }

        /// <summary>
        /// Тип измеряемой величины
        /// </summary>
        public enum TypeMeas
        {
            /// <summary>
            /// Частота
            /// </summary>
            FREQ,

            /// <summary>
            /// Среднеарефметическе
            /// </summary>
            MEAN,

            /// <summary>
            /// Период
            /// </summary>
            PERI,

            /// <summary>
            /// Размах
            /// </summary>
            PK2,

            /// <summary>
            /// Среднеквадратическое
            /// </summary>
            CRM,

            /// <summary>
            /// Минимум
            /// </summary>
            MIN,

            /// <summary>
            /// Максимум
            /// </summary>
            MAXI,

            /// <summary>
            /// Фронт
            /// </summary>
            RIS,

            /// <summary>
            /// Срез
            /// </summary>
            FALL,

            /// <summary>
            /// Длительноть положительного
            /// </summary>
            PWI,

            /// <summary>
            /// Длительность отрецательного
            /// </summary>
            NWI,

            /// <summary>
            /// Нет
            /// </summary>
            NONe
        }

        /// <summary>
        /// Масштаб, указан для <see cref = "Probe.Att_1" />
        /// </summary>
        public enum VerticalScale
        {
            /// <summary>
            /// 2 mV
            /// </summary>
            Scale_2 = 2,

            /// <summary>
            /// 5 mV
            /// </summary>
            Scale_5 = 5,

            /// <summary>
            /// 10 mV
            /// </summary>
            Scale_10 = 10,

            /// <summary>
            /// 20 mV
            /// </summary>
            Scale_20 = 20,

            /// <summary>
            /// 50 mV
            /// </summary>
            Scale_50 = 50,

            /// <summary>
            /// 100 mV
            /// </summary>
            Scale_100 = 100,

            /// <summary>
            /// 200 mV
            /// </summary>
            Scale_200 = 200,

            /// <summary>
            /// 500 mV
            /// </summary>
            Scale_500 = 500,

            /// <summary>
            /// 1000 mV
            /// </summary>
            Scale_1000 = 1000,

            /// <summary>
            /// 2000 mV
            /// </summary>
            Scale_2000 = 2000,

            /// <summary>
            /// 5000 mV
            /// </summary>
            Scale_5000 = 5000
        }

        #region Property

        public CCursor Cursor { get; }

        public CDisplay Display { get; }

        public CHorizontal Horizontal { get; }

        public CMath Math { get; }

        public CMeasurement Measurement { get; }

        public CChanel Chanel { get; }

        #endregion

        public TDS_Oscilloscope()
        {
            Cursor = new CCursor(this);
            Display = new CDisplay(this);
            Horizontal = new CHorizontal(this);
            //Math = new CMath(this);
            Measurement = new CMeasurement(this);
            Chanel = new CChanel(this);
        }

        #region Methods

        /// <summary>
        /// Выбрать канал
        /// </summary>
        /// <param name = "ch">The ch.</param>
        /// <param name = "st">The st.</param>
        /// <returns></returns>
        public static string SetChanel(ChanelSet ch, State st = State.ON)
        {
            return "SELECT:" + ch + " " + st;
        }

        /// <summary>
        /// Запуск или остановка осцилографа
        /// </summary>
        /// <param name = "st">The st.</param>
        /// <returns></returns>
        public static string EnableRun(State st)
        {
            return "ACQ:STATE " + st;
        }

        /// <summary>
        /// Пребразует данные в нужных единицах
        /// </summary>
        /// <param name = "date">The date.</param>
        /// <param name = "Mult">The mult.</param>
        /// <returns></returns>
        public double DataPreparationAndConvert(string date, Multipliers Mult = AP.Utils.Helps.Multipliers.None)
        {
            var Value = date.Split(',');
            var a = new double[Value.Length];
            for (var i = 0; i < Value.Length; i++) a[i] = Convert(Value[i], Mult);
            return a.Mean() < 0 ? a.RootMeanSquare() * -1 : a.RootMeanSquare();
        }

        private double Convert(string date, Multipliers Mult)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            double _return = 0;
            var dDate = new double[2];
            var Value = date.Split('E');
            dDate[0] = System.Convert.ToDouble(Value[0]);
            dDate[1] = System.Convert.ToDouble(Value[1]);
            switch (Mult)
            {
                case AP.Utils.Helps.Multipliers.Mega:
                    _return = dDate[0] * System.Math.Pow(10, dDate[1]) * 1E-6;
                    break;

                case AP.Utils.Helps.Multipliers.Mili:
                    _return = dDate[0] * System.Math.Pow(10, dDate[1]) * 1E3;
                    break;

                case AP.Utils.Helps.Multipliers.Nano:
                    _return = dDate[0] * System.Math.Pow(10, dDate[1]) * 1E9;
                    break;

                case AP.Utils.Helps.Multipliers.Kilo:
                    _return = dDate[0] * System.Math.Pow(10, dDate[1]) * 1E-3;
                    break;

                case AP.Utils.Helps.Multipliers.Micro:
                    _return = dDate[0] * System.Math.Pow(10, dDate[1]) * 1E6;
                    break;

                case AP.Utils.Helps.Multipliers.None:
                    _return = dDate[0] * System.Math.Pow(10, dDate[1]);
                    break;
            }

            return _return;
        }

        #endregion

        /// <summary>
        /// Команды калибровки и диагностики
        /// </summary>
        public class CalibrAndDiagnostic
        {
        }

        /// <summary>
        /// Команды управление курсорами
        /// </summary>
        public class CCursor
        {
            private readonly TDS_Oscilloscope _tdsOscilloscope;
            public CCursor(TDS_Oscilloscope inTdsOscilloscope)
            {
                _tdsOscilloscope = inTdsOscilloscope;
            }
        }

        /// <summary>
        /// Команды управления дисплеем
        /// </summary>
        public class CDisplay
        {

            private readonly TDS_Oscilloscope _tdsOscilloscope;
            public CDisplay(TDS_Oscilloscope inTdsOscilloscope)
            {
                _tdsOscilloscope = inTdsOscilloscope;
            }
        }

        /// <summary>
        /// Управление разверткой
        /// </summary>
        public class CHorizontal
        {
            private readonly TDS_Oscilloscope _tdsOscilloscope;
            public CHorizontal(TDS_Oscilloscope inTdsOscilloscope)
            {
                _tdsOscilloscope = inTdsOscilloscope;
            }

            #region Methods

            /// <summary>
            /// Установка временной развертки.
            /// </summary>
            /// <param name = "horisontalSc">Допустимое значение временной развертки.</param>
            /// <returns>Объекта осциллографа.</returns>
            public  TDS_Oscilloscope SetScale(HorisontalSCAle horisontalSc)
            {
                _tdsOscilloscope.WriteLine($"HORi:SCAL {horisontalSc.GetStringValue()}"); 
                return _tdsOscilloscope;
            }

            #endregion

            //HORizontal:DELay:POSition Position window
            //HORizontal:DELay:SCAle Set or query the window time base time/division
            //HORizontal:DELay:SECdiv Same as HORizontal:DELay:SCAle
            //HORizontal:MAIn:POSition Set or query the main time base trigger point
            //HORizontal:MAIn:SCAle Set or query the main time base time/division
            //HORizontal:MAIn:SECdiv Same as HORizontal:MAIn:SCAle
            //HORizontal:POSition Set or query the position of waveform to display
            //HORizontal:RECOrdlength Return waveform record length
            //HORizontal:SCAle Same as HORizontal:MAIn:SCAle
            //HORizontal:SECdiv Same as HORizontal:MAIn:SCAle
            //HORizontal:VIEW Select view
        }

        /// <summary>
        /// Математика
        /// </summary>
        public class CMath
        {
            //MATH:DEFINE Set or query the math waveform definition
            //            MATH:FFT:HORizontal:POSition(TDS200 with a TDS2MM module, TDS1000, TDS2000, TDS1000B, TDS2000B, and TPS2000 only)
            //Set or query the FFT horizontal display position
            //            MATH:FFT:HORizontal:SCAle(TDS200 with a TDS2MM module, TDS1000, TDS2000, TDS1000B, TDS2000B, and TPS2000 only)
            //Set or query the FFT horizontal zoom factor
            //            MATH:FFT:VERtical:POSition(TDS200 with a TDS2MM module, TDS1000, TDS2000, TDS1000B, TDS2000B, and TPS2000 only)
            //Set or query the FFT vertical display position
            //            MATH:FFT:VERtical:SCAle(TDS200 with a TDS2MM module, TDS1000, TDS2000, TDS1000B, TDS2000B, and TPS2000 only)
            //Set or query the FFT vertical zoom factor
            //            MATH:VERtical:POSition(TDS1000B, TDS2000B, and TPS2000 only)
            //Set or query the math waveform display position
            //            MATH:VERtical:SCAle(TDS1000B, TDS2000B, and TPS2000 only)
            //Set or query the math waveform display scale
        }

        /// <summary>
        /// Измерение
        /// </summary>
        public class CMeasurement
        {
            private readonly TDS_Oscilloscope _tdsOscilloscope;
            public CMeasurement(TDS_Oscilloscope inTdsOscilloscope)
            {
                _tdsOscilloscope = inTdsOscilloscope;
            }

            #region Methods

            /// <summary>
            /// Установить измерения
            /// </summary>
            /// <param name = "ch">на канале</param>
            /// <param name = "chm">измерительный канал</param>
            /// <param name = "tm">тип измерения</param>
            /// <returns></returns>
            public static string SetMeas(ChanelSet ch,  TypeMeas tm)
            {
                return "MEASU:MEAS" + (int) ch + ":SOU " + ch + "\nMEASU:MEAS" + (int) ch + ":TYP " + tm;
            }

            /// <summary>
            /// Запрос данных с указаного измерительного канала
            /// </summary>
            /// <param name = "chm">Измерительный канал</param>
            /// <returns></returns>
            public static string QueruValue(ChanelSet chm)
            {
                return "MEASU:MEAS" + (int) chm + ":VAL?";
            }

            #endregion
        }

        /// <summary>
        /// Различные команды
        /// </summary>
        public class CMiscellaneous
        {
            #region Methods

            /// <summary>
            /// Выбор автодиапазона(TDS1000B, TDS2000B, and TPS2000 only)
            /// </summary>
            /// <param name = "st">Состояние</param>
            /// <param name = "sett">Параметр</param>
            /// <returns></returns>
            public static string AutoRange(State st, MiscellaneousSetting sett = MiscellaneousSetting.BOTH)
            {
                return "AUTOR:STATE " + st + "\n" + "AUTOR:SETT" + sett;
            }

            /// <summary>
            /// Автоустановка
            /// </summary>
            /// <returns></returns>
            public static string AutoSet()
            {
                return "AUTOS EXEC";
            }

            /// <summary>
            /// Установка сбора данных
            /// </summary>
            /// <param name = "md">Режим сбора данных</param>
            /// <param name = "num">
            /// Количество накоплений, только для <see cref = "MiscellaneousMode.AVErage" />/param>
            /// <returns></returns>
            public static string SetDataCollection(MiscellaneousMode md,
                MiscellaneousNUMAV num = MiscellaneousNUMAV.Number_128)
            {
                if (md == MiscellaneousMode.AVErage)
                    return "ACQuire:MODe " + md + "\nACQuire:NUMAVg " + (int) num;
                return "ACQuire:MODe " + md;
            }

            #endregion
        }

        /// <summary>
        /// Управление тригером
        /// </summary>
        public class Trigger
        {
            /// <summary>
            /// Режим работы
            /// </summary>
            public enum Mode
            {
                /// <summary>
                /// Авто
                /// </summary>
                AUTO,

                /// <summary>
                /// Вручную
                /// </summary>
                NORM
            }

            /// <summary>
            /// Сробатывание тригера
            /// </summary>
            public enum Slope
            {
                /// <summary>
                /// по фронту
                /// </summary>
                FALL,

                /// <summary>
                /// По резу
                /// </summary>
                RIS
            }

            public enum Sours
            {
                /// <summary>
                /// Канал 1
                /// </summary>
                [StringValue("CH1")] CH1,

                /// <summary>
                /// Канал 2
                /// </summary>
                [StringValue("CH2")] CH2,

                /// <summary>
                /// Канал 3
                /// </summary>
                [StringValue("CH3")] CH3,

                /// <summary>
                /// Канал 4
                /// </summary>
                [StringValue("CH4")] CH4,

                [StringValue("EXT")] EXT,

                [StringValue("EXT5")] EXT5,

                [StringValue("EXT10")] EXT10,

                [StringValue("AC LINE")] AC_LINE
            }

            //public static string SetTriger(Sours sur, Slope sp,Mode md= Mode.AUTO, double Level=0)
            //{
            //    if (md!= Mode.AUTO)
            //    {
            //        return "TRIG:MAIn:EDGE:SOU " + MyEnum.GetStringValue(sur) + "\nTRIG:MAI:EDGE:SLO " + sp.ToString() + "\nTRIG:MAI:MODe " + md.ToString()+ "\nTRIG:MAI:LEV " +Level;
            //    }
            //    else
            //    {
            //        return "TRIG:MAIn:EDGE:SOU " + MyEnum.GetStringValue(sur) + "\nTRIG:MAI:EDGE:SLO " + sp.ToString() + "\nTRIG:MAI:MODe " + md.ToString();
            //    }

            //}
        }

       
    }

    

    /// <summary>
    /// класс для управления работой канала
    /// </summary>
    public class CChanel
    {
        public CVertical Vertical { get; }
        private readonly TDS_Oscilloscope _tdsOscilloscope;

        public CChanel(TDS_Oscilloscope inOsciloscope)
        {
            _tdsOscilloscope = inOsciloscope;
            Vertical = new CVertical(inOsciloscope);
        }

       

        public CChanel()
        {
            Vertical = new CVertical(_tdsOscilloscope);
        }

        /// <summary>
        /// Позволяет включить канал осциллографа
        /// </summary>
        /// <param name="inChanel">Канал с которым работаем</param>
        /// <param name="OnOffState">Статус канала вкл/выкл</param>
        public TDS_Oscilloscope SetChanelState(TDS_Oscilloscope.ChanelSet inChanel, TDS_Oscilloscope.State OnOffState)
        {
            _tdsOscilloscope.WriteLine($"select:{inChanel} {OnOffState}");

            return _tdsOscilloscope;
        }

       

        /// <summary>
        /// Коэфициент отклонения
        /// </summary>
        public class CVertical
        {
            private readonly TDS_Oscilloscope _tdsOscilloscope;
            public CVertical(TDS_Oscilloscope inTdsOscilloscope)
            {
                _tdsOscilloscope = inTdsOscilloscope;
            }

            #region Methods
            /// <summary>
            /// Возвращает строк с настройками канала.
            /// </summary>
            /// <param name="inChanel"></param>
            /// <returns></returns>
            public string GetVerticalParametr(TDS_Oscilloscope.ChanelSet inChanel)
            {
                return _tdsOscilloscope.QueryLine($"{inChanel}?");
            }

            /// <summary>
            /// Устанавливает полосу канала.
            /// </summary>
            /// <param name="chanel">Канал осциллографа.</param>
            /// <param name="setState">Статус опции (вкл/выкл).</param>
            /// <returns></returns>
            public TDS_Oscilloscope SetBandwith(TDS_Oscilloscope.ChanelSet chanel,TDS_Oscilloscope.State setState)
            {
                _tdsOscilloscope.WriteLine($"{chanel}:Band {setState}");
                return _tdsOscilloscope;
            }

            /// <summary>
            /// Отвечает какой сейчас статус полосы пропускания на канале.
            /// </summary>
            /// <param name="chanel">Канал осциллографа.</param>
            /// <returns></returns>
            public TDS_Oscilloscope.State GetBandwith(TDS_Oscilloscope.ChanelSet chanel)
            {
                string answer = _tdsOscilloscope.QueryLine($"{chanel}:Band?");
                if (answer.Equals(TDS_Oscilloscope.State.ON)) return TDS_Oscilloscope.State.ON;
                return TDS_Oscilloscope.State.OFF;
                
               
            }

          
            /// <summary>
            /// Устанавливает связь каналаю
            /// </summary>
            /// <param name="inChanel">Канал осциллографа.</param>
            /// <param name="inCouPling">Какой вариант связи установить: AC, DC, GND.</param>
            /// <returns></returns>
            public TDS_Oscilloscope SetCoupl(TDS_Oscilloscope.ChanelSet inChanel, TDS_Oscilloscope.COUPling inCouPling)
            {
                _tdsOscilloscope.WriteLine($"{inChanel}:coupl {inCouPling}");
                return _tdsOscilloscope;
            }

            /// <summary>
            /// Отвечает какой вид связи установлен на канале.
            /// </summary>
            /// <param name="inChanel">Канал осциллографа.</param>
            /// <returns></returns>
            public TDS_Oscilloscope.COUPling GetCoupling(TDS_Oscilloscope.ChanelSet inChanel)
            {
                string answer= _tdsOscilloscope.QueryLine($"{inChanel}:coupl?");
                if (answer.Equals(TDS_Oscilloscope.COUPling.DC)) return TDS_Oscilloscope.COUPling.DC;
                else if (answer.Equals(TDS_Oscilloscope.COUPling.AC)) return TDS_Oscilloscope.COUPling.AC;
                return TDS_Oscilloscope.COUPling.GND;
            }

            /// <summary>
            /// Инвертирует канал осциллографа.
            /// </summary>
            /// <param name="chanel">Канал осциллографа.</param>
            /// <param name="setState">Инверсия вкл/выкл.</param>
            /// <returns></returns>
            public TDS_Oscilloscope SetInvert(TDS_Oscilloscope.ChanelSet chanel, TDS_Oscilloscope.State setState)
            {
                _tdsOscilloscope.WriteLine($"{chanel}:inv {setState}");
                return _tdsOscilloscope;
            }

            /// <summary>
            /// Отвечает о статусе инверсии канала.
            /// </summary>
            /// <param name="chanel">Канал осциллографа.</param>
            /// <returns></returns>
            public TDS_Oscilloscope.State GetInvert(TDS_Oscilloscope.ChanelSet chanel)
            {
                string answer = _tdsOscilloscope.QueryLine($"{chanel}:inv?");
                if (answer.Equals(TDS_Oscilloscope.State.ON)) return TDS_Oscilloscope.State.ON;
                return TDS_Oscilloscope.State.OFF;
            }

            /// <summary>
            /// Устанавливает смещение канала по вертикали.
            /// </summary>
            /// <param name="chanel">Канал осциллографа.</param>
            /// <param name="verticalOffset">Величина смещения в делениях. Может быть дробным числом.</param>
            /// <returns></returns>
            public TDS_Oscilloscope SetPosition(TDS_Oscilloscope.ChanelSet chanel, decimal verticalOffset)
            {
                _tdsOscilloscope.WriteLine($"{chanel}:pos {(double)verticalOffset}");
                return _tdsOscilloscope;
            }

            public decimal GetPosition(TDS_Oscilloscope.ChanelSet chanel)
            {
                string answer = _tdsOscilloscope.QueryLine($"{chanel}:pos?");
                return decimal.Parse(answer);
            }

            /// <summary>
        /// Настройка канала
        /// </summary>
        /// <param name = "ch">Канал<see cref = "TDS_Oscilloscope.ChanelSet" /></param>
        /// <param name = "coup">Режимы работы канала<see cref = "TDS_Oscilloscope.COUPling" /></param>
        /// <param name = "BWlim">Ограничение полосы пропускания</param>
        /// <param name = "Invert">Инвертирование сигнала</param>
        /// <returns></returns>
        //public  void SetSetupChanel(TDS_Oscilloscope.ChanelSet ch, TDS_Oscilloscope.COUPling coup, TDS_Oscilloscope.State BWlim = TDS_Oscilloscope.State.OFF,
        //    TDS_Oscilloscope.State Invert = TDS_Oscilloscope.State.OFF)
        //{
        //   // return ch + ":COUP " + coup + "\n" + ch + ":BAN " + BWlim + "\n" + ch + ":INV " + Invert;
        //}

        /// <summary>
        /// Sets the sc ale.
        /// </summary>
        /// <param name = "ch">The ch.</param>
        /// <param name = "sc">The sc.</param>
        /// <param name = "prb">The PRB.</param>
        /// <returns></returns>
        public static string SetSCAle(TDS_Oscilloscope.ChanelSet ch, TDS_Oscilloscope.VerticalScale sc, TDS_Oscilloscope.Probe prb = TDS_Oscilloscope.Probe.Att_1)
            {
                return ch + ":PRO " + (int)prb + "\n" + ch + ":SCAle " + (int)sc / 1000.0 * (int)prb;
            }

            #endregion
        }
    }
}