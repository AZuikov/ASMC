// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using AP.Utils.Data;
using NLog;
using System;
using AP.Utils.Helps;

namespace ASMC.Devices.IEEE.Tektronix.Oscilloscope
{
    public class TDS_Oscilloscope : IeeeBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
        /// Режимы работы курсоровю
        /// </summary>
        public enum CursorFunc
        {
            /// <summary>
            /// горизонтальные курсоры.
            /// </summary>
            HBA,

            /// <summary>
            /// Вертикальные курсоры.
            /// </summary>
            VBA,

            /// <summary>
            /// Курсоры отключены.
            /// </summary>
            OFF
        }

        /// <summary>
        /// Единицы измерения для горизонтальных курсоров.
        /// </summary>
        public enum HorizontalCursorUnit
        {
            VOLTS,
            DIVS,
            DECIBELS,
            UNKNOWN
        }

        /// <summary>
        /// Допустимые значения развертки по времени.
        /// </summary>
        public enum HorizontalSCAle
        {
            /// <summary>
            /// 2.5 нс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Nano)]
            [DoubleValue(2.5)] Scal_2_5nSec,

            /// <summary>
            /// 5 нс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Nano)]
            [DoubleValue(5)] Scal_5nSec,

            /// <summary>
            /// 10 нс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Nano)]
            [DoubleValue(10)] Scal_10nSec,

            /// <summary>
            /// 25 нс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Nano)]
            [DoubleValue(25)] Scal_25nSec,

            /// <summary>
            /// 50 нс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Nano)]
            [DoubleValue(50)] Scal_50nSec,

            /// <summary>
            /// 100 нс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Nano)]
            [DoubleValue(100)] Scal_100nSec,

            /// <summary>
            /// 250 нс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Nano)]
            [DoubleValue(250)] Scal_250nSec,

            /// <summary>
            /// 500 нс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Nano)]
            [DoubleValue(500)] Scal_500nSec,

            /// <summary>
            /// 1 мкс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Micro)]
            [DoubleValue(1)] Scal_1mkSec,

            /// <summary>
            /// 2.5 мкс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Micro)]
            [DoubleValue(2.5)] Scal_2_5mkSec,

            /// <summary>
            /// 5 мкс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Micro)]
            [DoubleValue(5)] Scal_5mkSec,

            /// <summary>
            /// 10 мкс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Micro)]
            [DoubleValue(10)] Scal_10mkSec,

            /// <summary>
            /// 25 мкс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Micro)]
            [DoubleValue(25)] Scal_25mkSec,

            /// <summary>
            /// 50 мкс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Micro)]
            [DoubleValue(50)] Scal_50mkSec,

            /// <summary>
            /// 100 мкс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Micro)]
            [DoubleValue(100)] Scal_100mkSec,

            /// <summary>
            /// 250 мкс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Micro)]
            [DoubleValue(250)] Scal_250mkSec,

            /// <summary>
            /// 500 мкс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Micro)]
            [DoubleValue(500)] Scal_500mkSec,

            /// <summary>
            /// 1 мс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Mili)]
            [DoubleValue(1)] Scal_1mSec,

            /// <summary>
            /// 2.5 мс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Mili)]
            [DoubleValue(2.5)] Scal_2_5mSec,

            /// <summary>
            /// 5 мс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Mili)]
            [DoubleValue(5)] Scal_5mSec,

            /// <summary>
            /// 10 мс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Mili)]
            [DoubleValue(10)] Scal_10mSec,

            /// <summary>
            /// 25 мс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Mili)]
            [DoubleValue(25)] Scal_25mSec,

            /// <summary>
            /// 50 мс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Mili)]
            [DoubleValue(50)] Scal_50mSec,

            /// <summary>
            /// 100 мс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Mili)]
            [DoubleValue(100)] Scal_100mSec,

            /// <summary>
            /// 250 мс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Mili)]
            [DoubleValue(250)] Scal_250mSec,

            /// <summary>
            /// 500 мс
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.Mili)]
            [DoubleValue(500)] Scal_500mSec,

            /// <summary>
            /// 1 с
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.None)]
            [DoubleValue(1)] Scal_1Sec,

            /// <summary>
            /// 2.5 с
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.None)]
            [DoubleValue(2.5)] Scal_2_5Sec,

            /// <summary>
            /// 5 с
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.None)]
            [DoubleValue(5)] Scal_5Sec,

            /// <summary>
            /// 10 с
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.None)]
            [DoubleValue(10)] Scal_10Sec,

            /// <summary>
            /// 25 с
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.None)]
            [DoubleValue(25)] Scal_25Sec,

            /// <summary>
            /// 50 с
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.Time)]
            [UnitMultipliers(UnitMultipliers.None)]
            [DoubleValue(50)] Scal_50Sec
        }

        public enum HorizontalView
        {
            MAIN,
            WINDOW,
            ZONE
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
        /// Режим запуска осциллографа. Единичный по условию или непрерывный.
        /// </summary>
        public enum AcquireMode
        {
            SEQUENCE,
            RUNSTOP
        }
        /// <summary>
        /// Запуск и остановка режимов сбора данных осциллорафа.
        /// </summary>
        public enum  AcquireState
        {
            OFF,
            ON,
            RUN,
            STOP
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
            [StringValue("OFF")] OFF,

            /// <summary>
            /// Включить
            /// </summary>
            [StringValue("ON")] ON
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

        public enum VerticalCursorUnits
        {
            SECOND,
            HERTZ
        }

        /// <summary>
        /// Масштаб, указан для <see cref = "Probe.Att_1" />
        /// </summary>
        public enum VerticalScale
        {
            /// <summary>
            /// 2 mV
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.V)]
            [UnitMultipliers(UnitMultipliers.Mili)] 
            [DoubleValue(2)] Scale_2mV,

            /// <summary>
            /// 5 mV
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.V)] 
            [UnitMultipliers(UnitMultipliers.Mili)] 
            [DoubleValue(5)] Scale_5mV,

            /// <summary>
            /// 10 mV
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.V)] 
            [UnitMultipliers(UnitMultipliers.Mili)] 
            [DoubleValue(10)] Scale_10mV,

            /// <summary>
            /// 20 mV
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.V)] 
            [UnitMultipliers(UnitMultipliers.Mili)] 
            [DoubleValue(20)] Scale_20mV,

            /// <summary>
            /// 50 mV
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.V)] 
            [UnitMultipliers(UnitMultipliers.Mili)] 
            [DoubleValue(50)] Scale_50mV,

            /// <summary>
            /// 100 mV
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.V)] 
            [UnitMultipliers(UnitMultipliers.Mili)] 
            [DoubleValue(100)] Scale_100mV,

            /// <summary>
            /// 200 mV
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.V)] 
            [UnitMultipliers(UnitMultipliers.Mili)] 
            [DoubleValue(200)] Scale_200mV,

            /// <summary>
            /// 500 mV
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.V)]
            [UnitMultipliers(UnitMultipliers.Mili)]
            [DoubleValue(500)] Scale_500mV,

            /// <summary>
            /// 1000 mV
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.V)]
            [UnitMultipliers(UnitMultipliers.None)]
            [DoubleValue(1)] Scale_1V,

            /// <summary>
            /// 2000 mV
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.V)]
            [UnitMultipliers(UnitMultipliers.None)]
            [DoubleValue(2)] Scale_2V,

            /// <summary>
            /// 5000 mV
            /// </summary>
            [MeasureUnitsValue(MeasureUnits.V)]
            [UnitMultipliers(UnitMultipliers.None)]
            [DoubleValue(5)] Scale_5V
        }

        #region Property

        public CChanel Chanel { get; }
        public int ChanelCount { get; protected set; }

        public CCursor Cursor { get; }

        public CDisplay Display { get; }

        public bool GetSelfTestResult => QueryLine("diag:result:flag?").Equals("PASS") ? true : false;

        public CHorizontal Horizontal { get; }

        public CMath Math { get; }

        public CMeasurement Measurement { get; }

        public CMiscellaneous Acquire { get; }

        #endregion Property

       

        public TDS_Oscilloscope()
        {
            Cursor = new CCursor(this);
            Display = new CDisplay(this);
            Horizontal = new CHorizontal(this);
            //Math = new CMath(this);
            Measurement = new CMeasurement(this);
            Chanel = new CChanel(this);
            Acquire = new CMiscellaneous(this);
            Trigger = new CTrigger(this);
            

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

        #endregion Methods

        /// <summary>
        /// класс для управления работой канала
        /// </summary>
        public class CChanel
        {
            #region Fields

            private readonly TDS_Oscilloscope _tdsOscilloscope;

            #endregion Fields

            #region Property

            public CVertical Vertical { get; }

            #endregion Property

            public CChanel(TDS_Oscilloscope inOsciloscope)
            {
                _tdsOscilloscope = inOsciloscope;
                Vertical = new CVertical(inOsciloscope);
            }

            public CChanel()
            {
                Vertical = new CVertical(_tdsOscilloscope);
            }

            #region Methods

            /// <summary>
            /// Позволяет включить канал осциллографа
            /// </summary>
            /// <param name = "inChanel">Канал с которым работаем</param>
            /// <param name = "OnOffState">Статус канала вкл/выкл</param>
            public TDS_Oscilloscope SetChanelState(ChanelSet inChanel, State OnOffState)
            {
                _tdsOscilloscope.WriteLine($"select:{inChanel} {OnOffState}");

                return _tdsOscilloscope;
            }

            /// <summary>
            /// Устанавливает пробник для канала.
            /// </summary>
            /// <param name = "chanel">Канал осциллографа.</param>
            /// <param name = "inProbe">Номинал пробника.</param>
            /// <returns></returns>
            public TDS_Oscilloscope SetProbe(ChanelSet chanel, Probe inProbe)
            {
                _tdsOscilloscope.WriteLine($"{chanel}:pro {(int)inProbe}");
                return _tdsOscilloscope;
            }

            #endregion Methods

            /// <summary>
            /// Коэфициент отклонения
            /// </summary>
            public class CVertical : HelpDeviceBase
            {
                #region Fields

                private readonly TDS_Oscilloscope _tdsOscilloscope;

                #endregion Fields

                public CVertical(TDS_Oscilloscope inTdsOscilloscope)
                {
                    _tdsOscilloscope = inTdsOscilloscope;
                    Multipliers = new ICommand[]
                    {
                        new Command("N", "н", 1E-9),
                        new Command("U", "мк", 1E-6),
                        new Command("M", "м", 1E-3),
                        new Command("", "", 1)
                    };
                }

                #region Methods

                /// <summary>
                /// Отвечает какой сейчас статус полосы пропускания на канале.
                /// </summary>
                /// <param name = "chanel">Канал осциллографа.</param>
                /// <returns></returns>
                public State GetBandwith(ChanelSet chanel)
                {
                    var answer = _tdsOscilloscope.QueryLine($"{chanel}:Band?");
                    if (answer.Equals(State.ON)) return State.ON;
                    return State.OFF;
                }

                /// <summary>
                /// Отвечает какой вид связи установлен на канале.
                /// </summary>
                /// <param name = "inChanel">Канал осциллографа.</param>
                /// <returns></returns>
                public COUPling GetCoupling(ChanelSet inChanel)
                {
                    var answer = _tdsOscilloscope.QueryLine($"{inChanel}:coupl?");
                    if (answer.Equals(COUPling.DC)) return COUPling.DC;
                    if (answer.Equals(COUPling.AC)) return COUPling.AC;
                    return COUPling.GND;
                }

                /// <summary>
                /// Отвечает о статусе инверсии канала.
                /// </summary>
                /// <param name = "chanel">Канал осциллографа.</param>
                /// <returns></returns>
                public State GetInvert(ChanelSet chanel)
                {
                    var answer = _tdsOscilloscope.QueryLine($"{chanel}:inv?");
                    if (answer.Equals(State.ON)) return State.ON;
                    return State.OFF;
                }

                /// <summary>
                /// Возвращает смещение канала по вертикали в делениях.
                /// </summary>
                /// <param name = "chanel">Канал осциллографа.</param>
                /// <returns>Смещение по вертикали в делениях.</returns>
                public decimal GetPosition(ChanelSet chanel)
                {
                    var answer = _tdsOscilloscope.QueryLine($"{chanel}:pos?");
                    //проверить как парсится, там будет степень десятки
                    var doub = DataStrToDoubleMind(answer);
                    return (decimal)doub;
                }

                /// <summary>
                /// Отвечает какой пробник на канале.
                /// </summary>
                /// <param name = "chanel">Канал осциллографа.</param>
                /// <returns></returns>
                public Probe GetProbe(ChanelSet chanel)
                {
                    var answer = _tdsOscilloscope.QueryLine($"{chanel}:pro?");
                    return (Probe)(int)DataStrToDoubleMind(answer);
                }

                /// <summary>
                /// Отвечает какая вертикальная развертка сейчас установлена на канале.
                /// </summary>
                /// <param name = "chanel">Канал осциллорафа.</param>
                /// <returns></returns>
                public decimal GetScale(ChanelSet chanel)
                {
                    var answer = _tdsOscilloscope.QueryLine($"{chanel}:scale?");
                    var doub = DataStrToDoubleMind(answer);
                    return (decimal)doub;
                }

                /// <summary>
                /// Возвращает строк с настройками канала.
                /// </summary>
                /// <param name = "inChanel"></param>
                /// <returns></returns>
                public string GetVerticalParametr(ChanelSet inChanel)
                {
                    return _tdsOscilloscope.QueryLine($"{inChanel}?");
                }

                /// <summary>
                /// Устанавливает полосу канала.
                /// </summary>
                /// <param name = "chanel">Канал осциллографа.</param>
                /// <param name = "setState">Статус опции (вкл/выкл).</param>
                /// <returns></returns>
                public TDS_Oscilloscope SetBandwith(ChanelSet chanel, State setState)
                {
                    _tdsOscilloscope.WriteLine($"{chanel}:Band {setState}");
                    return _tdsOscilloscope;
                }

                /// <summary>
                /// Устанавливает связь каналаю
                /// </summary>
                /// <param name = "inChanel">Канал осциллографа.</param>
                /// <param name = "inCouPling">Какой вариант связи установить: AC, DC, GND.</param>
                /// <returns></returns>
                public TDS_Oscilloscope SetCoupl(ChanelSet inChanel, COUPling inCouPling)
                {
                    _tdsOscilloscope.WriteLine($"{inChanel}:coupl {inCouPling}");
                    return _tdsOscilloscope;
                }

                /// <summary>
                /// Инвертирует канал осциллографа.
                /// </summary>
                /// <param name = "chanel">Канал осциллографа.</param>
                /// <param name = "setState">Инверсия вкл/выкл.</param>
                /// <returns></returns>
                public TDS_Oscilloscope SetInvert(ChanelSet chanel, State setState)
                {
                    _tdsOscilloscope.WriteLine($"{chanel}:inv {setState}");
                    return _tdsOscilloscope;
                }

                /// <summary>
                /// Устанавливает смещение канала по вертикали.
                /// </summary>
                /// <param name = "chanel">Канал осциллографа.</param>
                /// <param name = "verticalOffset">Величина смещения в делениях. Может быть дробным числом.</param>
                /// <returns></returns>
                public TDS_Oscilloscope SetPosition(ChanelSet chanel, int verticalOffset)
                {
                    _tdsOscilloscope.WriteLine($"{chanel}:pos {verticalOffset}");
                    return _tdsOscilloscope;
                }

               

                /// <summary>
                /// Устанвливает вертикальную развертку.
                /// </summary>
                /// <param name = "ch">Канал осциллорафа.</param>
                /// <param name = "sc">Разверктка по вертикали.</param>
                /// <returns></returns>
                public TDS_Oscilloscope SetSCAle(ChanelSet chanel, VerticalScale inScale)
                {
                    double setVertScale = inScale.GetUnitMultipliersValue().GetDoubleValue() * inScale.GetDoubleValue();
                    _tdsOscilloscope
                       .WriteLine($"{chanel}:scale {setVertScale.ToString().Replace(',', '.')}");
                    return _tdsOscilloscope;
                }

                /// <summary>
                /// Позволяет задать развертку по вертикали с точностью до сотой Вольт. Например 1.76 Вольт/Клетка
                /// </summary>
                /// <param name = "chanel">Канал осциллографа.</param>
                /// <param name = "inScale">Величина разверкти, с точностью до сотой Вольт.</param>
                /// <returns></returns>
                public TDS_Oscilloscope SetSCAle(ChanelSet chanel, double inScale)
                {
                    _tdsOscilloscope.WriteLine($"{chanel}:scale {inScale.ToString().Replace(',', '.')}");
                    return _tdsOscilloscope;
                }

                #endregion Methods
            }
        }

        /// <summary>
        /// Команды калибровки и диагностики
        /// </summary>
        public class CalibrAndDiagnostic
        {
        }

        /// <summary>
        /// Команды управление курсорами
        /// </summary>
        public class CCursor : HelpDeviceBase
        {
            private const string curs_str = "cursor";

            #region Fields

            private readonly TDS_Oscilloscope _tdsOscilloscope;

            #endregion Fields

            #region Property

            /// <summary>
            /// Вовзращае строку с информацией о настройке курсоров
            /// </summary>
            public string[] GetCursorstatus => _tdsOscilloscope.QueryLine($"{curs_str}?").TrimEnd('\n').Split(';');

            public decimal GetDeltaBeetwenHorizontalCursors =>
                (decimal)DataStrToDoubleMind(_tdsOscilloscope.QueryLine($"{curs_str}:{CursorFunc.HBA}:delta?"));

            public decimal GetDeltaBeetwenVerticalCursors =>
                (decimal)DataStrToDoubleMind(_tdsOscilloscope.QueryLine($"{curs_str}:{CursorFunc.VBA}:delta?"));

            /// <summary>
            /// Возвращает данные о настройках горизонтальных курсоров.
            /// </summary>
            public string[] GetHorizontalCursorState =>
                _tdsOscilloscope.QueryLine($"{curs_str}:{CursorFunc.HBA}?").TrimEnd('\n').Split(';');

            /// <summary>
            /// Отвечает какие сейчас единицы измерения стоят на горизонтальных курсорах.
            /// </summary>
            public HorizontalCursorUnit GetHorizontalCursorUnits
            {
                get
                {
                    var answer = _tdsOscilloscope.QueryLine($"{curs_str}:{CursorFunc.HBA}:units?").TrimEnd('\n');
                    return (HorizontalCursorUnit)Enum.Parse(typeof(HorizontalCursorUnit), answer);
                }
            }

            public decimal GetPositionHorisontalCursor1 =>
                (decimal)DataStrToDoubleMind(_tdsOscilloscope.QueryLine($"{curs_str}:{CursorFunc.HBA}:position1?"));

            public decimal GetPositionHorisontalCursor2 =>
                (decimal)DataStrToDoubleMind(_tdsOscilloscope.QueryLine($"{curs_str}:{CursorFunc.HBA}:position2?"));

            public decimal GetPositionVerticalCursor1 =>
                (decimal)DataStrToDoubleMind(_tdsOscilloscope.QueryLine($"{curs_str}:{CursorFunc.VBA}:position1?"));

            public decimal GetPositionVerticalCursor2 =>
                (decimal)DataStrToDoubleMind(_tdsOscilloscope.QueryLine($"{curs_str}:{CursorFunc.VBA}:position2?"));

            /// <summary>
            /// Возвращает данные о настройках вертикальных курсоров.
            /// </summary>
            public string[] GetVerticalCursorState =>
                _tdsOscilloscope.QueryLine($"{curs_str}:{CursorFunc.VBA}?").TrimEnd('\n').Split(';');

            public VerticalCursorUnits GetVerticalCursorUnits
            {
                get
                {
                    var answer = _tdsOscilloscope.QueryLine($"{curs_str}:{CursorFunc.VBA}:units?").TrimEnd('\n');
                    return (VerticalCursorUnits)Enum.Parse(typeof(VerticalCursorUnits), answer);
                }
            }

            #endregion Property

            public CCursor(TDS_Oscilloscope inTdsOscilloscope)
            {
                _tdsOscilloscope = inTdsOscilloscope;
                Multipliers = new ICommand[]
                {
                    new Command("N", "н", 1E-9),
                    new Command("U", "мк", 1E-6),
                    new Command("M", "м", 1E-3),
                    new Command("", "", 1)
                };
            }

            #region Methods

            public TDS_Oscilloscope SelectSource(ChanelSet inChanel)
            {
                _tdsOscilloscope.WriteLine($"{curs_str}:sel:sou {inChanel}");
                return _tdsOscilloscope;
            }

            public TDS_Oscilloscope SetCursorFunc(CursorFunc inCursorFunc)
            {
                _tdsOscilloscope.WriteLine($"{curs_str}:func {inCursorFunc}");
                return _tdsOscilloscope;
            }

            #endregion Methods
        }

        /// <summary>
        /// Команды управления дисплеем
        /// </summary>
        public class CDisplay
        {
            #region Fields

            private readonly TDS_Oscilloscope _tdsOscilloscope;

            #endregion Fields

            public CDisplay(TDS_Oscilloscope inTdsOscilloscope)
            {
                _tdsOscilloscope = inTdsOscilloscope;
            }
        }

        /// <summary>
        /// Управление разверткой
        /// </summary>
        public class CHorizontal : HelpDeviceBase
        {
            #region Fields

            private readonly TDS_Oscilloscope _tdsOscilloscope;

            #endregion Fields

            #region Property

            public decimal GetHorizontalSCAle =>
                (decimal)DataStrToDoubleMind(_tdsOscilloscope.QueryLine("HORi:SCALE?"));

            public HorizontalView GetHorizontalView =>
                (HorizontalView)Enum.Parse(typeof(HorizontalView), _tdsOscilloscope.QueryLine("hor:vie?"));

            #endregion Property

            public CHorizontal(TDS_Oscilloscope inTdsOscilloscope)
            {
                _tdsOscilloscope = inTdsOscilloscope;
            }

            #region Methods

            public TDS_Oscilloscope SetHorizontalView(HorizontalView inHorizontalView)
            {
                _tdsOscilloscope.WriteLine($"hor:vie {inHorizontalView}");
                return _tdsOscilloscope;
            }

            /// <summary>
            /// Установка временной развертки.
            /// </summary>
            /// <param name = "horizontalSc">Допустимое значение временной развертки.</param>
            /// <returns>Объекта осциллографа.</returns>
            public TDS_Oscilloscope SetHorizontalScale(HorizontalSCAle inHorizontalScale)
            {
                double horizontScale = inHorizontalScale.GetUnitMultipliersValue().GetDoubleValue() * inHorizontalScale.GetDoubleValue();
                _tdsOscilloscope.WriteLine($"hor:sca {horizontScale.ToString().Replace(',','.')}");
                return _tdsOscilloscope;
            }

            #endregion Methods

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
        public class CMeasurement : HelpDeviceBase
        {
            #region Fields

            private readonly TDS_Oscilloscope _tdsOscilloscope;

            #endregion Fields

            public CMeasurement(TDS_Oscilloscope inTdsOscilloscope)
            {
                _tdsOscilloscope = inTdsOscilloscope;

                Multipliers = new ICommand[]
                {
                    new Command("N", "н", 1E-9),
                    new Command("U", "мк", 1E-6),
                    new Command("M", "м", 1E-3),
                    new Command("", "", 1),
                    new Command("K", "к", 1e3)
                };
            }

            #region Methods

            /// <summary>
            /// Устанавливает источник измерения (например канал осциллографа) и тип измерения.
            /// </summary>
            /// <param name="ch">Номер канала осциллографа.</param>
            /// <param name="tm">Тип измерений.</param>
            /// <param name="MeasureNumb">Задает номер измеряемого параметра (от 1 до 4).</param>
            /// <returns></returns>
            public TDS_Oscilloscope SetMeas(ChanelSet ch, TypeMeas tm, int MeasureNumb = 1)
            {
                if (MeasureNumb < 1 && MeasureNumb > 4)
                {
                    string strError =
                        $"У осциллографа может быть только 4 измеряемых параметра.\nЗначение {MeasureNumb} вне диапазона.";
                    Logger.Error(strError);
                    throw new ArgumentException(strError);
                }
                _tdsOscilloscope.WriteLine($"MEASU:MEAS{MeasureNumb}:SOU {ch}");
                _tdsOscilloscope.WriteLine($"MEASU:MEAS{MeasureNumb}:TYP {tm}");
                return _tdsOscilloscope;
            }

            /// <summary>
            /// Возвращает значение измеренного параметра. Одновременно прибор может измерять до 4-х параметров.
            /// </summary>
            /// <param name="MeasureNumb">Номер измеряемого параметра (от 1 до 4).</param>
            /// <returns></returns>
            public decimal MeasureValue(int MeasureNumb = 1)
            {
                if (MeasureNumb < 1 && MeasureNumb > 4)
                {
                    string strError =
                        $"У осциллографа может быть только 4 измеряемых параметра.\nЗначение {MeasureNumb} вне диапазона.";
                    Logger.Error(strError);
                    throw new ArgumentException(strError);
                }
                string answer = _tdsOscilloscope.QueryLine($"measu:meas{MeasureNumb}:val?").TrimEnd('\n');
                return (decimal)DataStrToDoubleMind(answer);
            }

            #endregion Methods
        }

        /// <summary>
        /// Различные команды
        /// </summary>
        public class CMiscellaneous
        {
            #region Methods

            private TDS_Oscilloscope _tdsOscilloscope;

            public CMiscellaneous(TDS_Oscilloscope inOscilloscope)
            {
                _tdsOscilloscope = inOscilloscope;
            }

            /// <summary>
            /// Выбор автодиапазона(TDS1000B, TDS2000B, and TPS2000 only)
            /// </summary>
            /// <param name = "st">Состояние</param>
            /// <param name = "sett">Параметр</param>
            /// <returns></returns>
            public TDS_Oscilloscope AutoRange(State st, MiscellaneousSetting sett = MiscellaneousSetting.BOTH)
            {
                _tdsOscilloscope.WriteLine($"AUTOR:STATE {st}"); 
                _tdsOscilloscope.WriteLine($"AUTOR:SETT {sett}");
                return _tdsOscilloscope;
            }

            /// <summary>
            /// Автоустановка
            /// </summary>
            /// <returns></returns>
            public TDS_Oscilloscope AutoSet()
            {
                _tdsOscilloscope.WriteLine("AUTOS EXEC");
                return _tdsOscilloscope;
            }

            /// <summary>
            /// Установка сбора данных
            /// </summary>
            /// <param name = "md">Режим сбора данных</param>
            /// <param name = "num">
            /// Количество накоплений, только для <see cref = "MiscellaneousMode.AVErage" />/param>
            /// <returns></returns>
            public TDS_Oscilloscope SetDataCollection(MiscellaneousMode md, MiscellaneousNUMAV num = MiscellaneousNUMAV.Number_128)
            {
                _tdsOscilloscope.WriteLine($"ACQuire:MODe {md}");
                if (md == MiscellaneousMode.AVErage)
                {
                       
                    _tdsOscilloscope.WriteLine($"ACQuire:NUMAVg {(int)num}");
                }

                return _tdsOscilloscope;
            }

            public TDS_Oscilloscope SetSingleOrRunStopMode(AcquireMode mode)
            {
                _tdsOscilloscope.WriteLine($"acq:stopafter {mode}");
                return _tdsOscilloscope;
            }

            public TDS_Oscilloscope StartAcquire()
            {
                _tdsOscilloscope.WriteLine($"acq:state {AcquireState.RUN}");
                return _tdsOscilloscope;
            }

            #endregion Methods
        }

        public CTrigger Trigger { get; protected set; }

        /// <summary>
        /// Управление тригером
        /// </summary>
        public class CTrigger
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

            public enum Type
            {
                EDGE,
                VIDeo
            }

            private TDS_Oscilloscope _tdsOscilloscope;

            public CTrigger( TDS_Oscilloscope incOscilloscope)
            {
                _tdsOscilloscope = incOscilloscope;
            }

            /// <summary>
            /// Устанавдивает режимработы триггера (auto/manual).
            /// </summary>
            /// <param name="inMode">Режим работы триггера.</param>
            /// <returns></returns>
            public TDS_Oscilloscope SetTriggerMode(Mode inMode)
            {
                _tdsOscilloscope.WriteLine($"TRIG:MAI:MODe {inMode}");
                return _tdsOscilloscope;
            }

            /// <summary>
            /// Задает тип сигнала для срабатывания триггера.
            /// </summary>
            /// <param name="inType">Тип сигнала</param>
            /// <returns></returns>
            public TDS_Oscilloscope SetTriggerType(Type inType)
            {
                _tdsOscilloscope.WriteLine($"TRIG:MAI:TYPe {inType}");
                return _tdsOscilloscope;
            }

            /// <summary>
            /// Задает уровень срадатывания триггера.
            /// </summary>
            /// <param name="inLevel">Уровень сигнала в вольтах.</param>
            /// <returns></returns>
            public TDS_Oscilloscope SetTriggerLevel(double inLevel)
            {
                _tdsOscilloscope.WriteLine($"TRIG:MAI:LEV {inLevel.ToString().Replace(',','.')}");
                return _tdsOscilloscope;
            }

            /// <summary>
            /// Устанавливает уровень срабатывания триггера на 50 % от уровня входного сигнала.
            /// </summary>
            /// <returns></returns>
            public TDS_Oscilloscope SetTriggerLevelOn50Percent()
            {
                _tdsOscilloscope.WriteLine($"trig:main setlevel");
                return _tdsOscilloscope;
            }

            /// <summary>
            /// Задает источник сигнала для триггера в режиме EDGE.
            /// </summary>
            /// <param name="inChanel">Источник сигнала для триггера.</param>
            /// <returns></returns>
            public TDS_Oscilloscope SetTriggerEdgeSource(ChanelSet inChanel)
            {
                _tdsOscilloscope.WriteLine($"TRIG:MAIn:EDGE:SOU {inChanel}");
                return _tdsOscilloscope;
            }

            /// <summary>
            /// Задает срабатывание триггера на фронт или спад в режиме EDGE.
            /// </summary>
            /// <param name="inSlope">Режимы срабатывания триггера.</param>
            /// <returns></returns>
            public TDS_Oscilloscope SetTriggerEdgeSlope(Slope inSlope)
            {
                _tdsOscilloscope.WriteLine($"TRIG:MAI:EDGE:SLO {inSlope}");
                return _tdsOscilloscope;
            }


            //public string SetTriger(Sours sur, Slope sp, Mode md = Mode.AUTO, double Level = 0)
            //{
            //    if (md != Mode.AUTO)
            //    {
            //        return "TRIG:MAIn:EDGE:SOU " + MyEnum.GetStringValue(sur) + "\nTRIG:MAI:EDGE:SLO " + sp.ToString() + "\nTRIG:MAI:MODe " + md.ToString() + "\nTRIG:MAI:LEV " + Level;
            //    }
            //    else
            //    {
            //        return "TRIG:MAIn:EDGE:SOU " + MyEnum.GetStringValue(sur) + "\nTRIG:MAI:EDGE:SLO " + sp.ToString() + "\nTRIG:MAI:MODe " + md.ToString();
            //    }

            //}
        }

       
    }
}