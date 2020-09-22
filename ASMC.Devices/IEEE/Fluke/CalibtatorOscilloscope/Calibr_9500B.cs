// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Xml;
using AP.Utils.Data;
using AP.Utils.Helps;
using ASMC.Data.Model;
using ASMC.Devices.IEEE.Tektronix.Oscilloscope;
using NLog;

namespace ASMC.Devices.IEEE.Fluke.CalibtatorOscilloscope
{

    public class Calibr9500B : IeeeBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public enum Chanel
        {
            Ch1 = 1,
            Ch2 = 2,
            Ch3 = 3,
            Ch4 = 4,
            Ch5 = 5
        }

        public enum Direction
        {
            /// <summary>
            /// устанавливает фронт (ступенька вверх).
            /// </summary>
            RIS,

            /// <summary>
            /// устанавливает спад (ступенька вниз).
            /// </summary>
            FALL
        }

        /// <summary>
        /// Выходной импеданс
        /// </summary>
        public enum Impedans
        {
            /// <summary>
            /// 50 ОМ
            /// </summary>
            [StringValue("5.000000E+01")] Res_50 = 50,

            /// <summary>
            /// 1 МОм
            /// </summary>
            [StringValue("1.000000E+06")] Res_1M = 1000000
        }

        /// <summary>
        /// Перечисление форм волны для маркера.
        /// </summary>
        public enum MarkerWaveForm
        {
            SQU,
            PULS,
            TRI,
            LINE,
            UNKNOWN
        }

        public enum Polar
        {
            /// <summary>
            /// устанавливает выходной прямоугольный сигнал положительной полярности относительно заземления
            /// </summary>
            POS,

            /// <summary>
            /// устанавливает выходной прямоугольный сигнал отрицательной полярности относительно заземления
            /// </summary>
            NEG,

            /// <summary>
            /// устанавливает выходной прямоугольный сигнал симметрично относительно земли
            /// </summary>
            SYMM
        }

        public enum Shap
        {
            /// <summary>
            /// Определяет, что последующий выбор напряжения (VOLT) или тока (CURR) будет иметь только постоянную составляющую (DC).
            /// </summary>
            DC,

            /// <summary>
            /// Определяет, что последующий выбор напряжения (VOLT) или тока (CURR) связан с сигналом прямоугольной формы
            /// </summary>
            SQU,

            /// <summary>
            /// Выбирает функцию перепада импульса. Форма сигнала выбирается отдельной командой.
            /// </summary>
            EDG,

            /// <summary>
            /// Выбирает форму сигнала временного маркера.
            /// </summary>
            MARK,

            /// <summary>
            /// Выбирает нормированный синусоидальный сигнал.
            /// </summary>
            SIN,

            /// <summary>
            /// Выбирает энергию импульса, используемого для тестирования защиты от перегрузки осциллографа.
            /// </summary>
            OPUL,

            /// <summary>
            /// Выбирает форму полного видеосигнала.
            /// </summary>
            TEL,

            /// <summary>
            /// Выбирает замкнутое или разомкнутое состояние активной головки для определения тока утечки осциллографа.
            /// </summary>
            LEAK,

            /// <summary>
            /// Выбирает функцию пилообразного сигнала
            /// </summary>
            RAMP,

            /// <summary>
            /// Выбирает функцию выравнивания задержки
            /// </summary>
            SKEW,

            /// <summary>
            /// Выбирает функцию длительности импульса
            /// </summary>
            PWID,

            /// <summary>
            /// Выбирает входной сигнал по дополнительному входу (Auxiliary).
            /// </summary>
            EXT
        }

        /// <summary>
        /// Скорость наростания
        /// </summary>
        public enum SpeedEdge
        {
            /// <summary>
            /// 100нс
            /// </summary>
            Low_100n = 700,

            /// <summary>
            /// 500 пс
            /// </summary>
            Mid_500p = 500,

            /// <summary>
            /// 150 пс
            /// </summary>
            Fast_150p = 100
        }

        public enum State
        {
            On,
            Off
        }

        /// <summary>
        /// Тип измерения
        /// </summary>
        public enum TypeMes
        {
            /// <summary>
            /// Сопротивление
            /// </summary>
            RES,

            /// <summary>
            /// Емкость
            /// </summary>
            CAP
        }

        #region Property

        public CAUXFunctional AuxFunc { get; }

        public CROUTe Route { get; }

        public CSource Source { get; }

        #endregion
        /// <summary>
        /// Список (словарь) возможных моделей активных головок.
        /// </summary>
        public readonly Dictionary<string, ActiveHeadFor9500B> ActiveHeadDictionary = 
            new Dictionary<string, ActiveHeadFor9500B>();
        

        public Calibr9500B()
        {
            UserType = "Fluke 9500B";
            Source = new CSource(this);
            Route = new CROUTe(this);
            AuxFunc = new CAUXFunctional(this);
            Multipliers =  new ICommand[]
            {
                new Command("N", "н", 1E-9),
                new Command("U", "мк", 1E-6),
                new Command("M", "м", 1E-3),
                new Command("", "", 1)
                
            };

            //список возможных головок калибратора
            ActiveHeadDictionary.Add("9510", new ActiveHead9510());
            ActiveHeadDictionary.Add("9530", new ActiveHead9530());
            ActiveHeadDictionary.Add("9550", new ActiveHead9550());
            ActiveHeadDictionary.Add("9560", new ActiveHead9560());
        }

        /// <summary>
        /// Найдет каналы, на которых установлена модель головки.
        /// </summary>
        /// <param name="head">Головка которую хотим найти.</param>
        /// <returns></returns>
        public  List<Chanel> FindActiveHeadOnChanel(ActiveHeadFor9500B head)
        {
            List<Chanel> resultHeadList = new List<Chanel>();
            for (int i = 1; i <= 5; i++)
            {
                string[] answer = QueryLine($"ROUT:FITT? CH{i}").Split(',');
                if (answer[0].Equals(head.GetModelName)) 
                    resultHeadList.Add((Chanel)i);
            }

            return resultHeadList;
        }

        /// <summary>
        /// Возвращает полный переченб подключенных головок к каналам.
        /// </summary>
        /// <returns>Словарь: номер канала - модель головы.</returns>
        public Dictionary<Chanel, ActiveHeadFor9500B> FindAllActiveHead()
        {
            Dictionary<Chanel, ActiveHeadFor9500B> resultDict = new Dictionary<Chanel, ActiveHeadFor9500B>();
            for (int i = 1; i <= 5; i++)
            {
                string[] answer = QueryLine($"ROUT:FITT? CH{i}").Split(',');
                if (!answer[0].Equals("NONE")&& !answer[0].Equals("CABL")) resultDict.Add((Chanel)i, ActiveHeadDictionary[answer[0]]);
            }

            return resultDict.Count == 0? null: resultDict;
        }

        /// <summary>
        /// Настройка выхода
        /// </summary>
        public class CSource : HelpDeviceBase
        {
            #region Fields

            private readonly Calibr9500B _calibrMain;

            #endregion

            #region Property

            public CParametr Parametr { get; }

            #endregion

            public CSource(Calibr9500B calibrMain)
            {
                _calibrMain = calibrMain;
                Parametr = new CParametr(calibrMain);
                Multipliers =  new ICommand[]
                {
                    new Command("N", "н", 1E-9),
                    new Command("U", "мк", 1E-6),
                    new Command("M", "м", 1E-3),
                    new Command("", "", 1),
                    new Command("K", "к", 1E3)
                };
            }

            #region Methods

            /// <summary>
            /// Возвращает значение установленной частоты.
            /// </summary>
            /// <returns>Значение частоты в decimal.</returns>
            public decimal GetFreq()
            {
                var answer = _calibrMain.QueryLine("SOUR:FREQ?");
                var arr = answer.Replace(".", CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator)
                                .Split('E');
                return Decimal.Parse(arr[0]) * (decimal) Math.Pow(10, Double.Parse(arr[1]));
            }

            /// <summary>
            /// Считывает значение напряжения на выходе канала.
            /// </summary>
            /// <returns>Значение напряжения типа decimal.</returns>
            public decimal GetVoltage()
            {
                var answer = _calibrMain.QueryLine("SOUR:VOLT?");
                var var = answer.TrimEnd('\n')
                                .Replace(".", CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator)
                                .Split('E');

                return Decimal.Parse(var[0]) * (decimal) Math.Pow(10, Double.Parse(var[1]));
            }

            /// <summary>
            /// Управление переключением выхода: включено/выключено
            /// </summary>
            public Calibr9500B Output(State st)
            {
                _calibrMain.WriteLine("OUTP " + st);
                return _calibrMain;
            }

            /// <summary>
            /// Установка частоты
            /// </summary>
            /// <param name = "value">The value.</param>
            /// <param name = "mult">The mult.</param>
            /// <returns></returns>
            public Calibr9500B SetFreq(double value, UnitMultipliers mult = UnitMultipliers.None)
            {
                _calibrMain.WriteLine($@"SOUR:FREQ {JoinValueMult(value, mult)}");
                return _calibrMain;
            }

            /// <summary>
            /// Данная команда определяет основной требуемый сигнальный выход, т.е. выбирает функцию источника калибратора 9500B.
            /// </summary>
            public Calibr9500B SetFunc(Shap sp)
            {
                _calibrMain.WriteLine("SOUR:FUNC " + sp);
                return _calibrMain;
            }

            /// <summary>
            /// Установка периода
            /// </summary>
            /// <param name = "value">The value.</param>
            /// <param name = "mult">The mult.</param>
            /// <returns></returns>
            public Calibr9500B SetPeriod(double value, UnitMultipliers mult = UnitMultipliers.None)
            {
                _calibrMain.WriteLine($@"SOUR:PER {JoinValueMult(value, mult)}");
                return _calibrMain;
            }

            /// <summary>
            /// Установка напряжения
            /// </summary>
            /// <param name = "value">The value.</param>
            /// <param name = "mult">The mult.</param>
            /// <returns></returns>
            public Calibr9500B SetVoltage(double value, UnitMultipliers mult = UnitMultipliers.None)
            {
                _calibrMain.WriteLine($@"SOUR:VOLT:ampl {(value *  mult.GetDoubleValue()).ToString().Replace(',','.')}");
                return _calibrMain;
            }

            #endregion

            /// <summary>
            /// Настройки выходного параметра
            /// </summary>
            public class CParametr
            {
                #region Fields

                private Calibr9500B _calibrMain;

                #endregion

                #region Property

                public CDC DC { get; }

                public CEDGE EDGE { get; }

                public CMARKER MARKER { get; }

                #endregion

                public CParametr(Calibr9500B calibr)
                {
                    _calibrMain = calibr;
                    EDGE = new CEDGE(calibr);
                    DC = new CDC(calibr);
                    MARKER = new CMARKER(calibr);
                }

                /// <summary>
                /// Настройка постоянного напряжения
                /// </summary>
                public class CDC
                {
                    #region Fields

                    private readonly Calibr9500B _calibrMain;

                    #endregion

                    public CDC(Calibr9500B calibr)
                    {
                        _calibrMain = calibr;
                    }

                    #region Methods

                    /// <summary>
                    /// Данная команда позволяет установить постоянный сигнал на нескольких каналах (если устанавливается в «ON») и отключить
                    /// многоканальность, если выбрано «OFF»
                    /// </summary>
                    /// <param name = "st">The st.</param>
                    /// <returns></returns>
                    public Calibr9500B MCH(State st)
                    {
                        _calibrMain.WriteLine("SOUR:PAR:DC:MCH " + st);
                        return _calibrMain;
                    }

                    /// <summary>
                    /// Данная команда устанавливает выход напряжения постоянного тока (DCV) в ноль (0 В), если выбрано «ON» и вернет
                    /// предыдущее значение, если выбрано «OFF».
                    /// Изменение функция отключает состояние заземления.
                    /// С выходом, установленным в состояние «ON», выводится ошибка конфликта параметров настройки, если выбрана не функция DC.
                    /// </summary>
                    /// <param name = "st">The st.</param>
                    /// <returns></returns>
                    public Calibr9500B SetGND(State st)
                    {
                        _calibrMain.WriteLine("SOUR:PAR:DC:GRO " + st);
                        return _calibrMain;
                    }

                    #endregion
                }

                /// <summary>
                /// Настройка Меандра
                /// </summary>
                public class CSQU
                {
                    #region Fields

                    private readonly Calibr9500B _calibrMain;

                    #endregion

                    public CSQU(Calibr9500B calibr)
                    {
                        _calibrMain = calibr;
                    }

                    #region Methods

                    /// <summary>
                    /// Данная команда устанавливает выходной сигнал прямоугольной формы в ноль(0 В), если выбрано «ON» и вернет предыдущее
                    /// выходное значение, если выбрано «OFF».
                    /// </summary>
                    /// <param name = "st">The st.</param>
                    /// <returns></returns>
                    public Calibr9500B SetGND(State st)
                    {
                        _calibrMain.WriteLine("SOUR:PAR:SQU:GRO " + st);
                        return _calibrMain;
                    }

                    /// <summary>
                    /// Данная команда выбирает полярность прямоугольного сигнала: выше, ниже или симметрично относительно «0» вольт
                    /// </summary>
                    /// <param name = "pr">The pr.</param>
                    /// <returns></returns>
                    public Calibr9500B SetPolar(Polar pr)
                    {
                        _calibrMain.WriteLine("SOUR:PAR:SQU:POL " + pr);
                        return _calibrMain;
                    }

                    #endregion
                }

                /// <summary>
                /// настройка импульса перепада
                /// </summary>
                public class CEDGE
                {
                    #region Fields

                    private readonly Calibr9500B _calibrMain;

                    #endregion

                    public CEDGE(Calibr9500B calibr)
                    {
                        _calibrMain = calibr;
                    }

                    #region Methods

                    /// <summary>
                    /// Задает направление перепада, который следует за запуском
                    /// </summary>
                    /// <param name = "Se"></param>
                    /// <returns></returns>
                    public Calibr9500B SetEdgeDirection(Direction Der)
                    {
                        _calibrMain.WriteLine("SOUR:PAR:EDGE:TRAN " + Der);
                        return _calibrMain;
                    }

                    /// <summary>
                    /// Данная команда выбирает скорость (скорость нарастания выходного напряжения) функции Edge
                    /// </summary>
                    /// <param name = "Se"></param>
                    /// <returns></returns>
                    public Calibr9500B SetEdgeSpeed(SpeedEdge Se)
                    {
                        _calibrMain.WriteLine("SOUR:PAR:EDGE:SPE " + (double) Se * Math.Pow(10, -12));
                        return _calibrMain;
                    }

                    #endregion
                }

                public class CMARKER
                {
                

                    private Calibr9500B _calibrMain;
                    public CMARKER(Calibr9500B calibr)
                    {
                        _calibrMain = calibr;
                    }

                    public Calibr9500B SetWaveForm(MarkerWaveForm inWaveForm)
                    {
                        _calibrMain.WriteLine($"sour:par:mark:wave {inWaveForm}");
                        return _calibrMain;
                    }

                    public MarkerWaveForm GetMarkerWaveForm()
                    {
                        string answer = _calibrMain.QueryLine($"sour:par:mark:wave?").TrimEnd('\n').ToUpper();
                        //Enum.GetName(typeof(MarkerWaveForm), answer);
                        foreach (MarkerWaveForm markerWave in (MarkerWaveForm[]) Enum.GetValues(typeof(MarkerWaveForm)))
                        {
                            if (markerWave.ToString().ToUpper().Equals(answer)) return markerWave;
                        }

                        Logger.Error($"Неизвестная фома волны считана с калибратора {answer}. Добавьте в перечисление.");
                        return MarkerWaveForm.UNKNOWN;


                    }
                }
            }
    }

        /// <summary>
        /// Данная подсистема используется для конфигурирования выходных каналов, которые используются как выходы сигнала и
        /// источники триггера.
        /// </summary>
        public class CROUTe
        {
            #region Fields

            private Calibr9500B _calibMain;

            #endregion

            #region Property

            public ChanelSetting Chanel { get; }

            #endregion

            public CROUTe(Calibr9500B calibr)
            {
                _calibMain = calibr;
                Chanel = new ChanelSetting(calibr);
            }

            public class ChanelSetting
            {
                #region Fields

                private readonly Calibr9500B _calibMain;

                #endregion

                public ChanelSetting(Calibr9500B calibr)
                {
                    _calibMain = calibr;
                }

                #region Methods

                /// <summary>
                /// Позволяет полчить значение импеданса текущего рабочего канала.
                /// </summary>
                /// <returns></returns>
                public Impedans GetImpedans()
                {
                    var answer = _calibMain.QueryLine("ROUT:SIGN:IMP?");
                    var val = answer.TrimEnd('\n').Replace(".", ",").Split('E');

                    return (Impedans) (int) (double.Parse(val[0]) * Math.Pow(10, double.Parse(val[1])));
                }

                /// <summary>
                /// Данная команда используется для определения канала, связанного с сигнальным выходом.
                /// <cpd> не включает выход, а только выбирает используемый сигнальный канал.
                /// </summary>
                /// <param name = "ch">The ch.</param>
                /// <returns></returns>
                public Calibr9500B SetChanel(Chanel ch)
                {
                    if (ch == null)
                    {
                        string errorStr = $"Невозможно установить канал на калибраторе. Значение параметра: {ch}";
                        Logger.Error(errorStr);
                        throw new ArgumentException(errorStr);
                    }

                    _calibMain.WriteLine("ROUT:SIGN " + ch);
                    return _calibMain;
                }

                /// <summary>
                /// Данная команда выбирает между 50 Ом и 1 МОм импедансом осциллографа, в соответствии с уровнями для выбранного
                /// сигнального канала.
                /// </summary>
                /// <param name = "imp">The imp.</param>
                /// <returns></returns>
                public Calibr9500B SetImpedans(Impedans imp)
                {
                    _calibMain.WriteLine("ROUT:SIGN:IMP " + (int) imp);
                    return _calibMain;
                }

                #endregion
            }
        }

        /// <summary>
        /// Дополнительный функцианал
        /// </summary>
        public class CAUXFunctional
        {
            #region Fields

            private readonly Calibr9500B _calibrMain;

            #endregion

            public CAUXFunctional(Calibr9500B calibr)
            {
                _calibrMain = calibr;
            }

            #region Methods

            /// <summary>
            /// Запрос значения сопротивления или емкости.
            /// </summary>
            public string QueryAuxValue()
            {
                var answer = _calibrMain.QueryLine("READ?");

                return answer;
            }

            /// <summary>
            /// Данная команда используется для выбора функции измерения входного сопротивления или входной емкости испытуемого
            /// осциллографа.
            /// </summary>
            /// <param name = "tm"></param>
            /// <returns></returns>
            public Calibr9500B SetMeasure(TypeMes tm)
            {
                _calibrMain.WriteLine("CONF:" + tm);
                return _calibrMain;
            }

            #endregion
        }
    }

    public abstract class ActiveHeadFor9500B
    {
        protected string ModelName;

        /// <summary>
        /// Максимальная частота для модели активной головки.
        /// </summary>
        private MeasPoint MaxFreq;
        /// <summary>
        /// Длительность фронта для данной головки
        /// </summary>
        private MeasPoint[] ImpulseWidth;

        /// <summary>
        /// Серийный (заводской) номер.
        /// </summary>
        public string HeadSerialNumb { get; protected set; }

        public ActiveHeadFor9500B(MeasPoint maxFreq, MeasPoint[] impulseWidth)
        {
            
            MaxFreq = maxFreq;
            ImpulseWidth = impulseWidth;
        }

        /// <summary>
        /// Какая максимальная частота у головы.
        /// </summary>
        public MeasPoint GetMaxFreq
        {
            get { return MaxFreq; }
        }

        /// <summary>
        /// Возможные длительности импульса головы.
        /// </summary>
        public MeasPoint[] GetImpulseWidthArr
        {
            get { return ImpulseWidth; }
        }

        public string GetModelName
        {
            get { return ModelName; }
        }

    }

    public class ActiveHead9510 : ActiveHeadFor9500B
    {
        public ActiveHead9510() : base(new MeasPoint(MeasureUnits.Herz, UnitMultipliers.Giga, 1),
                                       new MeasPoint[] {new MeasPoint(MeasureUnits.sec, UnitMultipliers.Pico, 500)})
        {
           
            ModelName = "9510";
        }
    }

    public class ActiveHead9530 : ActiveHeadFor9500B
    {
        public ActiveHead9530() : base(new MeasPoint(MeasureUnits.Herz, UnitMultipliers.Giga, (decimal) 3.2),
                                       new MeasPoint[]
                                       {
                                           new MeasPoint(MeasureUnits.sec, UnitMultipliers.Pico, 150),
                                           new MeasPoint(MeasureUnits.sec, UnitMultipliers.Pico, 500)
                                       })
        {
           
            ModelName = "9530";
        }
    }

    public class ActiveHead9550 : ActiveHeadFor9500B
    {
        public ActiveHead9550() : base(new MeasPoint(MeasureUnits.Herz, UnitMultipliers.Giga, 14),
                                       new MeasPoint[] {new MeasPoint(MeasureUnits.sec, UnitMultipliers.Pico, 25)})
        {
            
            ModelName = "9550";
        }
    }

    public class ActiveHead9560 : ActiveHeadFor9500B
    {
        public ActiveHead9560() : base(new MeasPoint(MeasureUnits.Herz, UnitMultipliers.Giga, 6),
                                                new MeasPoint[] {new MeasPoint(MeasureUnits.sec, UnitMultipliers.Pico, 70)})
        {
           
            ModelName = "9560";
        }
    }

   

}