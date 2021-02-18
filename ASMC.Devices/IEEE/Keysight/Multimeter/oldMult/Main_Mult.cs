// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Globalization;
using System.Linq;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using NLog;

namespace ASMC.Devices.IEEE.Keysight.Multimeter
{
    
    [Obsolete("Устарел. ")]public abstract class MultMain : IeeeBase
    {
        /// <summary>
        /// Запрос о включенных выходах. Ответ FRON - передняя панель,
        /// </summary>
        private const string QueryTerminal = "ROUT:TERM?";

        /// <summary>
        /// Запрос на получение значения (U, I, R, F), для преобразования использовать ConvertDate
        /// </summary>
        private const string QueryValue = "init; READ?";

        #region Property

        /// <summary>
        /// Предоставляет доступ к измерению переменного тока, напряжения и частоты.
        /// </summary>
        public MAc Ac { get; }

        /// <summary>
        /// Передоставляет настройки постоянного тока, напряжения, споротивления, ёмкости.
        /// </summary>
        public MDc Dc { get; }
        

        /// <summary>
        /// Предоставляет признак включения входов на передней панели  true - передняя панель, false - задняя
        /// </summary>
        public bool IsTerminal
        {
            get
            {
                Open();
                WriteLine(QueryTerminal);
                var result = ReadLine().StartsWith("FRON", StringComparison.InvariantCultureIgnoreCase);
                Close();
                return result;
            }
        }

        /// <summary>
        /// Предоставляет время последнего измерения.
        /// </summary>
        public TimeSpan TimeLastMeas { get; private set; }

        /// <summary>
        /// Содержит пароль снятия защиты.
        /// </summary>
        protected static string SecuredCodeCalibr { get; set; }

        #endregion

        protected MultMain()
        {
            Dc = new MDc(this);
            Ac = new MAc(this);
        }

        #region Methods

        /// <summary>
        /// Запрос на получение значения (U, I, R, F)
        /// </summary>
        /// <param name = "unitMultiplier">Желаемый множитель.</param>
        /// <returns>Возвращает значение с желаемым множителем.</returns>
        public double GetMeasValue(UnitMultiplier unitMultiplier = UnitMultiplier.None)
        {
            WriteLine(":TRIG:SOUR BUS");
            WriteLine("INIT;*TRG");
            QueryLine("*opc?");
            WriteLine("FETCH?");
            
            var res = DataStrToDoubleMind(ReadString(), unitMultiplier);
            return res;
        }

        #endregion
        

        public class Calibr
        {
            #region Methods

            /// <summary>
            /// Установить калибровочное значение
            /// </summary>
            /// <param name = "value">значение</param>
            /// <param name = "typeCalValue">Тип калибровки</param>
            /// <param name = "multipliers">Множитель</param>
            /// <returns></returns>
            public static string SetCalValue(double value, string typeCalValue, string multipliers)
            {
                return "CAL:VAL " + value + multipliers.Insert(multipliers.Length, typeCalValue);
            }

            /// <summary>
            /// Запрос на установки калибровки
            /// </summary>
            /// <returns></returns>
            public static string QueryCal()
            {
                return "CAL?";
            }

            #endregion

            public class Secured
            {
                /// <summary>
                /// Статус
                /// </summary>
                public enum State
                {
                    /// <summary>
                    /// Включить
                    /// </summary>
                    On,

                    /// <summary>
                    /// Выключить
                    /// </summary>
                    Off
                }

                /// <summary>
                /// Запросить статус безопасности
                /// </summary>
                public static string ChecState = "CAL:SEC:STAT?";

                #region Fields

                /// <summary>
                /// Автовыбор предела
                /// </summary>
                public string Off = "CAL:SEC:STAT OFF, " + SecuredCodeCalibr;

                /// <summary>
                /// Автовыбор предела
                /// </summary>
                public string On = "CAL:SEC:STAT ON, " + SecuredCodeCalibr;

                #endregion

                #region Methods

                /// <summary>
                /// Снятие и установка защиты
                /// </summary>
                /// <param name = "state">Состояние защиты</param>
                /// <returns></returns>
                public static string InputCode(State state)
                {
                    return "CAL:SEC:STAT " + (state == 0 ? "ON, " : "OFF, ") + SecuredCodeCalibr;
                }

                /// <summary>
                /// Снятие и установка защиты
                /// </summary>
                /// <param name = "code">Код защиты</param>
                /// <param name = "state">Состояние защиты</param>
                /// <returns></returns>
                public static string InputCode(string code, State state)
                {
                    return "CAL:SEC:STAT " + (state == 0 ? "ON, " : "OFF, ") + code;
                }

                #endregion
            }

            /// <summary>
            /// Типы калибровок
            /// </summary>
            public struct TypeCalValue
            {
                /// <summary>
                /// Калибровка напряжения
                /// </summary>
                public const string Volt = "V";

                /// <summary>
                /// Калибровка по току
                /// </summary>
                public const string Current = "";
            }
        }

        public class MAc
        {
            #region Fields

            private MultMain _multMain;

            #endregion

            #region Property

            public MVoltage Voltage { get; }
            

            #endregion

            public MAc(MultMain multMain)
            {
                _multMain = multMain;
                Voltage = new MVoltage(multMain);
            }
        }

        /// <summary>
        /// Содержит команды отвечающие за настройку измерения частоты
        /// </summary>
        public class Frequency
        {
            /// <summary>
            /// Содержит перечнь комант отвечающих за выбор предела
            /// </summary>
            public class Range
            {
                /// <summary>
                /// Автовыбор предела включение измерения частоты
                /// </summary>
                public const string AutoAndOn = "CONF:FREQ";

                /// <summary>
                /// Предел 100 мВ
                /// </summary>
                public const string Mv100 = "SENS:FREQ:VOLT:RANG 0.1";

                /// <summary>
                /// Предел 1 В
                /// </summary>
                public const string V1 = "SENS:FREQ:VOLT:RANG 1";

                /// <summary>
                /// Предел 10 В
                /// </summary>
                public const string V10 = "SENS:FREQ:VOLT:RANG 10";

                /// <summary>
                /// Предел 100 В
                /// </summary>
                public const string V100 = "SENS:FREQ:VOLT:RANG 100";

                /// <summary>
                /// Предел 1000 В
                /// </summary>
                public const string V1000 = "SENS:FREQ:VOLT:RANG 1000";
            }
        }

        /// <summary>
        /// Содержит команды отвечающие за настройку измерения сопротивления
        /// </summary>
        public class Resistance
        {
            /// <summary>
            /// Обычный режим измерения сопротиления
            /// </summary>
            public class Normal
            {
                /// <summary>
                /// Определяет предел
                /// </summary>
                public class Range
                {
                    /// <summary>
                    /// Автовыбор предела
                    /// </summary>
                    public const string Auto = "CONF:RES";

                    /// <summary>
                    /// Предел 100 Ом
                    /// </summary>
                    public const string R100 = "CONF:RES 100";

                    /// <summary>
                    /// Предел 1 кОм
                    /// </summary>
                    public const string Rk1 = "CONF:RES 1 KOhm";

                    /// <summary>
                    /// Предел 10 кОм
                    /// </summary>
                    public const string Rk10 = "CONF:RES 10 KOhm";

                    /// <summary>
                    /// Предел 100 кОм
                    /// </summary>
                    public const string Rk100 = "CONF:RES 100 KOhm";

                    /// <summary>
                    /// Предел 1 MОм
                    /// </summary>
                    public const string Rm1 = "CONF:RES 1 MOhm";

                    /// <summary>
                    /// Предел 10 MОм
                    /// </summary>
                    public const string Rm10 = "CONF:RES 10 MOhm";

                    /// <summary>
                    /// Предел 100 MОм
                    /// </summary>
                    public const string Rm100 = "CONF:RES 100 MOhm";

                    /// <summary>
                    /// Предел 1 ГОм
                    /// </summary>
                    public const string Rm1000 = "CONF:RES 1 GOhm";
                }
            }

            /// <summary>
            /// 4х проводное измерение
            /// </summary>
            public class TrueOhm
            {
                /// <summary>
                /// Определяет предел
                /// </summary>
                public class Range
                {
                    /// <summary>
                    /// Автовыбор предела
                    /// </summary>
                    public const string Auto = "CONF:FRES";

                    /// <summary>
                    /// Предел 100 Ом
                    /// </summary>
                    public const string R100 = "CONF:FRES 100";

                    /// <summary>
                    /// Предел 1 кОм
                    /// </summary>
                    public const string Rk1 = "CONF:FRES 1 KOhm";

                    /// <summary>
                    /// Предел 10 кОм
                    /// </summary>
                    public const string Rk10 = "CONF:FRES 10 KOhm";

                    /// <summary>
                    /// Предел 100 кОм
                    /// </summary>
                    public const string Rk100 = "CONF:FRES 100 KOhm";

                    /// <summary>
                    /// Предел 1 MОм
                    /// </summary>
                    public const string Rm1 = "CONF:FRES 1 MOhm";

                    /// <summary>
                    /// Предел 10 MОм
                    /// </summary>
                    public const string Rm10 = "CONF:FRES 10 MOhm";

                    /// <summary>
                    /// Предел 100 MОм
                    /// </summary>
                    public const string Rm100 = "CONF:FRES 100 MOhm";

                    /// <summary>
                    /// Предел 1 ГОм
                    /// </summary>
                    public const string Rm1000 = "CONF:FRES 1 GOhm";
                }
            }
        }

        /// <summary>
        /// Содержит команды для настроек измерения емкости
        /// </summary>
        public class Capacitance
        {
            /// <summary>
            /// Содержит набор команд для установки необходимого диапазона
            /// </summary>
            public class Range
            {
                /// <summary>
                /// Автовыбор предела включение измерения частоты
                /// </summary>
                public const string AutoAndOn = "CONF:CAP";

                /// <summary>
                /// Предел 1 нФ
                /// </summary>
                public const string Nf1 = "CONF:CAP 1 NF";

                /// <summary>
                /// Предел 10 нФ
                /// </summary>
                public const string Nf10 = "CONF:CAP 10 NF";

                /// <summary>
                /// Предел 100 нФ
                /// </summary>
                public const string Nf100 = "CONF:CAP 100 NF";

                /// <summary>
                /// Предел 1 мкФ
                /// </summary>
                public const string Uf1 = "CONF:CAP 1 UF";

                /// <summary>
                /// Предел 10 мкФ
                /// </summary>
                public const string Uf10 = "CONF:CAP 10 UF";

                /// <summary>
                /// Предел 100 мкФ
                /// </summary>
                public const string Uf100 = "CONF:CAP 100 UF";
            }
        }
    }

    /// <summary>
    /// Измерение переменного напряжения
    /// </summary>
    public class MVoltage
    {
        #region Fields

        private MultMain _multMain;

        #endregion

        #region Property

        public AcvFiltr Filtr { get; protected internal set; }

        /// <summary>
        /// Позволяет настраивать предел измерения.
        /// </summary>
        public AcvRange Range { get; protected internal set; }

        #endregion

        public MVoltage(MultMain multMain)
        {
            _multMain = multMain;
            Range = new AcvRange(multMain);
            Filtr = new AcvFiltr(multMain);
        }
    }

    /// <summary>
    /// Определяет предел
    /// </summary>
    public class AcvRange
    {
        public enum ERanges
        {
            /// <summary>
            /// Команда автоматического выбора придела
            /// </summary>
            [StringValue("CONF:VOLT:AC AUTO")] [DoubleValue(0)]
            Auto,

            /// <summary>
            /// Предел 100 мВ
            /// </summary>
            [StringValue("CONF:VOLT:AC 100 MV")] [DoubleValue(0.1)]
            Mv100,

            /// <summary>
            /// Предел 1 В
            /// </summary>
            [StringValue("CONF:VOLT:AC 1")] [DoubleValue(1)]
            V1,

            /// <summary>
            /// Предел 10 В
            /// </summary>
            [StringValue("CONF:VOLT:AC 10")] [DoubleValue(10)]
            V10,

            /// <summary>
            /// Предел 100 В
            /// </summary>
            [StringValue("CONF:VOLT:AC 100")] [DoubleValue(100)]
            V100,

            /// <summary>
            /// Предел 1000 В
            /// </summary>
            [StringValue("CONF:VOLT:AC 750")] [DoubleValue(750)]
            V750
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        private readonly MultMain _multMain;

        #endregion

        #region Property

        /// <summary>
        /// Позволяет настраивать фильтр.
        /// </summary>
        public AcvFiltr Filtr { get; }

        public ICommand[] Ranges { get; protected set; }

        #endregion

        public AcvRange(MultMain multMain)
        {
            _multMain = multMain;
            Filtr = new AcvFiltr(multMain);
            Ranges = new ICommand[]
            {
                new Command("CONF:VOLT:AC AUTO", "Автоматичсекий выбор предела", 0),
                new Command("CONF:VOLT:AC 100 MV", "100 мВ", 100E-3),
                new Command("CONF:VOLT:AC 1", "1 В", 1),
                new Command("CONF:VOLT:AC 10", "10 В", 10),
                new Command("CONF:VOLT:AC 100", "100 В", 100),
                new Command("CONF:VOLT:AC 750", "750 В", 750)
            };
        }

        #region Methods

        /// <summary>
        /// Устанавливает диапазон взависимости от значения.
        /// </summary>
        /// <param name = "value">Входное значение.</param>
        /// <param name = "unitMultiplier">Множитель вногдной величины.</param>
        /// <returns></returns>
        public MultMain Set(double value, UnitMultiplier unitMultiplier = UnitMultiplier.None)
        {
            var val = Math.Abs(value);
            val *= unitMultiplier.GetDoubleValue();
            var res = Ranges.FirstOrDefault(q => q.Value <= val);

            if (res == null)
            {
                Logger.Info(@"Входное знаечние больше допустимого,установлен максимальный предел.");
                res = Ranges.First(q => Equals(q.Value, Ranges.Select(p => p.Value).Max()));
            }

            _multMain.WriteLine(res.StrCommand);

            return _multMain;
        }

        public MultMain Set(ERanges range = ERanges.Auto)
        {
            _multMain.WriteLine(range.GetStringValue());
            return _multMain;
        }

        #endregion
    }

    /// <summary>
    /// Содержит команды отвечающие за настройку измерения силы тока
    /// </summary>
    public class AcCurrent
    {
        /// <summary>
        /// Определяет предел
        /// </summary>
        public class AcaRange
        {
            /// <summary>
            /// Предел 1 А
            /// </summary>
            public const string A1 = "CONF:CURR:AC 1";

            /// <summary>
            /// Предел 10 А
            /// </summary>
            public const string A10 = "CONF:CURR:AC 10";

            /// <summary>
            /// Предел 3 А
            /// </summary>
            public const string A3 = "CONF:CURR:AC 3";

            /// <summary>
            /// Автовыбор предела
            /// </summary>
            public const string Auto = "CONF:CURR:DC:RANG AUTO ON";

            /// <summary>
            /// Предел 1 мА
            /// </summary>
            public const string M1 = "CONF:CURR:AC 1 MA";

            /// <summary>
            /// Предел 10 мА
            /// </summary>
            public const string M10 = "CONF:CURR:AC 10 MA";

            /// <summary>
            /// Предел 100 мА
            /// </summary>
            /// COLT
            public const string M100 = "CONF:CURR:AC 100 MA";

            /// <summary>
            /// Предел 100 мкА
            /// </summary>
            public const string U100 = "CONF:CURR:AC 100 UA";

            #region Methods

            /// <summary>
            /// Установка предела от ожидаемого значения
            /// </summary>
            /// <param name = "value">Ожидаемое знаечние</param>
            /// <returns>Сформированую команду</returns>
            public static string Set(double value)
            {
                return "CONF:CURR:AC:RANG " + value.ToString(CultureInfo.GetCultureInfo("en-US"));
            }

            #endregion
        }

        /// <summary>
        /// Фильтр
        /// </summary>
        public class AcaFiltr
        {
            /// <summary>
            /// Установить фильтр 200 Гц
            /// </summary>
            public const string Filtr200 = "SENS:CURR:AC:BAND 200";

            /// <summary>
            /// Установить фильтр 20 Гц, по умолчанию
            /// </summary>
            public const string Filtr20Def = "SENS:CURR:AC:BAND 20";

            /// <summary>
            /// Установить фильтр 3 Гц
            /// </summary>
            public const string Filtr3 = "SENS:CURR:AC:BAND 3";
        }
    }

    /// <summary>
    /// Измерение переменного напряжения
    /// </summary>
    public class AcVoltage
    {
        #region Fields

        private MultMain _multMain;

        #endregion

        #region Property

        public AcvFiltr Filtr { get; protected internal set; }

        /// <summary>
        /// Позволяет настраивать предел измерения.
        /// </summary>
        public AcvRange Range { get; protected internal set; }

        #endregion

        public AcVoltage(MultMain multMain)
        {
            _multMain = multMain;
            Range = new AcvRange(multMain);
            Filtr = new AcvFiltr(multMain);
        }
    }

    /// <summary>
    /// Фильтр
    /// </summary>
    public class AcvFiltr
    {
        public enum EFiltrs
        {
            /// <summary>
            /// Установить фильтр 3 Гц
            /// </summary>
            [StringValue("SENS:VOLT:AC:BAND 3")] [DoubleValue(3)]
            F3,

            /// <summary>
            /// Установить фильтр 20 Гц, по умолчанию
            /// </summary>
            [StringValue("SENS:VOLT:AC:BAND 20")] [DoubleValue(20)]
            F20,

            /// <summary>
            /// Установить фильтр 200 Гц
            /// </summary>
            [StringValue("CSENS:VOLT:AC:BAND 200")] [DoubleValue(200)]
            F200
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        private readonly MultMain _multMain;

        #endregion

        #region Property

        public ICommand[] Filters { get; protected set; }

        #endregion

        public AcvFiltr(MultMain multMain)
        {
            _multMain = multMain;
            Filters = new ICommand[]
            {
                new Command("SENS:VOLT:AC:BAND 3", "ФВЧ 3 Гц", 3),
                new Command("SENS:VOLT:AC:BAND 20", "ФВЧ 3 Гц", 20),
                new Command("SENS:VOLT:AC:BAND 200", "ФВЧ 3 Гц", 200)
            };
        }

        #region Methods

        /// <summary>
        /// Устанавливает диапазон фильтра взависимости от значения.
        /// </summary>
        /// <param name = "value">Входное значение.</param>
        /// <param name = "unitMultiplier">Множитель вногдной величины.</param>
        /// <returns></returns>
        public MultMain Set(double value, UnitMultiplier unitMultiplier = UnitMultiplier.None)
        {
            var val = Math.Abs(value);
            val *= unitMultiplier.GetDoubleValue();
            var res = Filters.FirstOrDefault(q => q.Value >= val);

            if (res == null)
            {
                Logger.Info(@"Входное знаечние меньше допустимого,установлен минимальная полоса.");
                res = Filters.First(q => Equals(q.Value, Filters.Select(p => p.Value).Min()));
            }

            _multMain.WriteLine(res.StrCommand);

            return _multMain;
        }

        #endregion
    }

    public class MDc/* : IDmmBaseMeasure*/
    {
        #region Fields

        private MultMain _multMain;

        #endregion

        #region Property

        public Current Current { get; }

        public DcVoltage Voltage { get; }

        #endregion

        public MDc(MultMain multMain)
        {
            _multMain = multMain;
            Current = new Current(multMain);
            Voltage = new DcVoltage(multMain);
        }
    }

    public class DcVoltage
    {
        #region Fields

        private MultMain _multMain;

        #endregion

        #region Property

        public DcvRange Range { get; }

        #endregion

        public DcVoltage(MultMain multMain)
        {
            _multMain = multMain;
            Range = new DcvRange(multMain);
        }
    }

    /// <summary>
    /// Определяет предел
    /// </summary>
    public class DcvRange
    {
        public enum ERanges
        {
            /// <summary>
            /// Команда автоматического выбора придела
            /// </summary>
            [StringValue("CONF:VOLT:DC AUTO")] [DoubleValue(0)]
            Auto,

            /// <summary>
            /// Предел 100 мВ
            /// </summary>
            [StringValue("CONF:VOLT:DC 100 MV")] [DoubleValue(0.1)]
            Mv100,

            /// <summary>
            /// Предел 1 В
            /// </summary>
            [StringValue("CONF:VOLT:DC 1")] [DoubleValue(1)]
            V1,

            /// <summary>
            /// Предел 10 В
            /// </summary>
            [StringValue("CONF:VOLT:DC 10")] [DoubleValue(10)]
            V10,

            /// <summary>
            /// Предел 100 В
            /// </summary>
            [StringValue("CONF:VOLT:DC 100")] [DoubleValue(100)]
            V100,

            /// <summary>
            /// Предел 1000 В
            /// </summary>
            [StringValue("CONF:VOLT:DC 1000")] [DoubleValue(1000)]
            V1000
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        private readonly MultMain _multMain;

        #endregion

        #region Property

        public ICommand[] Ranges { get; protected internal set; }

        #endregion

        public DcvRange(MultMain multMain)
        {
            Ranges = new ICommand[]
            {
                new Command("CONF:VOLT:DC AUTO", "Автоматичсекий выбор предела", 0),
                new Command("CONF:VOLT:DC 100 MV", "100 мВ", 1E-3),
                new Command("CONF:VOLT:DC 1 V", "1 В", 1),
                new Command("CONF:VOLT:DC 10 V", "10 В", 10),
                new Command("CONF:VOLT:DC 100 V", "100 В", 100),
                new Command("CONF:VOLT:DC 1000 V", "1000 В", 1000)
            };
            _multMain = multMain;
        }

        #region Methods

        /// <summary>
        /// Устанавливает диапазон взависимости от значения.
        /// </summary>
        /// <param name = "value">Входное значение.</param>
        /// <param name = "unitMultiplier">Множитель вногдной величины.</param>
        /// <returns></returns>
        public MultMain Set(double value, UnitMultiplier unitMultiplier = UnitMultiplier.None)
        {
            var val = Math.Abs(value);
            val *= unitMultiplier.GetDoubleValue();
            var res = Ranges.OrderBy(o => o.Value).FirstOrDefault(q => q.Value <= val);

            if (res == null)
            {
                Logger.Info(@"Входное знаечние больше допустимого,установлен максимальный предел.");
                res = Ranges.First(q => Equals(q.Value, Ranges.Select(p => p.Value).Max()));
            }

            _multMain.WriteLine(res.StrCommand);

            return _multMain;
        }

        public MultMain Set(ERanges range = ERanges.Auto)
        {
            _multMain.WriteLine(range.GetStringValue());
            return _multMain;
        }

        #endregion
    }

    public class Current
    {
        #region Fields

        private MultMain _multMain;

        #endregion

        #region Property

        public DcaRange Range { get; protected internal set; }

        #endregion

        public Current(MultMain multMain)
        {
            _multMain = multMain;
            Range = new DcaRange(multMain);
        }
    }

    /// <summary>
    /// Определяет предел
    /// </summary>
    public class DcaRange
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        private readonly MultMain _multMain;

        #endregion

        #region Property

        public ICommand[] Ranges { get; protected set; }

        #endregion

        public DcaRange(MultMain multMain)
        {
            _multMain = multMain;
            Ranges = new ICommand[]
            {
                new Command("CONF:CURR:DC:RANG AUTO ON", "Автоматичсекий выбор предела", 0),
                new Command("CONF:CURR:DC 10 UA", "10 мкА", 10E-6),
                new Command("CONF:CURR:DC 100 MA", "100 мА", 100E-3),
                new Command("CONF:CURR:DC 10 MA", "10 мА", 10E-3),
                new Command("CONF:CURR:DC 1 MA", "1 мА", 1E-3),
                new Command("CONF:CURR:DC 1 A", "1 А", 1),
                new Command("CONF:CURR:DC 3 A", "3 А", 3)
            };
            _multMain = multMain;
        }

        #region Methods

        /// <summary>
        /// Устанавливает диапазон взависимости от значения.
        /// </summary>
        /// <param name = "value">Входное значение.</param>
        /// <param name = "unitMultiplier">Множитель вногдной величины.</param>
        /// <returns></returns>
        public MultMain Set(double value, UnitMultiplier unitMultiplier = UnitMultiplier.None)
        {
            var val = Math.Abs(value);
            val *= unitMultiplier.GetDoubleValue();
            var res = Ranges.FirstOrDefault(q => q.Value <= val);

            if (res == null)
            {
                Logger.Info(@"Входное знаечние больше допустимого,установлен максимальный предел.");
                res = Ranges.First(q => Equals(q.Value, Ranges.Select(p => p.Value).Max()));
            }

            _multMain.WriteLine(res.StrCommand);

            return _multMain;
        }

        #endregion
    }
}