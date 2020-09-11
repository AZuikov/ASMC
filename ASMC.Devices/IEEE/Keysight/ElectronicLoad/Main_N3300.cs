using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using AP.Utils.Data;
using AP.Utils.Helps;
using NLog;

namespace ASMC.Devices.IEEE.Keysight.ElectronicLoad
{
    public  class MainN3300 : IeeeBase
    {

       
        public enum ModeWorks
        {
            /// <summary>
            /// Режим стабилизации тока.
            /// </summary>
            [StringValue("CURR")] Current,

            /// <summary>
            /// Режим нагрузки.
            /// </summary>
            [StringValue("RES")] Resistance,

            /// <summary>
            /// Режим стабелизации напряжения.
            /// </summary>
            [StringValue("VOLT")] Voltage
        }

        public enum State
        {
            [StringValue("0")] Off=0,

            [StringValue("1")] On=1
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        /// <summary>
        /// Номер канала, в котором установлен модуль нагрузки.
        /// </summary>
        private int _chanNum;

        protected string ModuleModel;

        protected decimal[] RangeCurrentArr;

        //массивы с номиналами пределов модуля нагрузки
        //будет инициализироваться в конструкторе
        protected decimal[] RangeVoltArr;

        #endregion

        #region Property

        /// <summary>
        /// Возвращает номер канала модуля нагрузки
        /// </summary>
        /// <returns></returns>
        public int ChanelNumber => _chanNum;

        public Current Current { get; protected set; }

        public string GetModuleModel => ModuleModel;
        //public Meas Meas { get; protected set; }

        //public Meas Meas { get; }

        /// <summary>
        /// Отвечает, какой текущий режим канала
        /// </summary>
        public ModeWorks ModeWork
        {
            get
            {
                var answer = QueryLine("FUNC?");

                foreach (ModeWorks mode in Enum.GetValues(typeof(ModeWorks)))
                    if (mode.GetStringValue().Equals(answer, StringComparison.CurrentCultureIgnoreCase))
                        return mode;
                Logger.Error("Режим не определен.");
                throw new Exception("Режим не определен.");
            }
        }

        public Resistance Resistance { get; protected internal set; }

        public State StateOutput
        {
            get
            {
                return (State) int.Parse(QueryLine("outp?"));
            }
        }

        public Voltage Voltage { get; protected set; }

        #endregion

        protected MainN3300()
        {
            Resistance = new Resistance(this);
            UserType = "N3300A";
            Current = new Current(this);
            Voltage = new Voltage(this);
        }

        #region Methods

        /// <summary>
        /// Конвертирует строку в число decimal
        /// </summary>
        /// <param name = "inStr"></param>
        /// <returns></returns>
        private static decimal StrToDecimal(string inStr)
        {
            var val = inStr.TrimEnd('\n').Replace(".", ",").Split('E');

            return decimal.Parse(val[0]) * (decimal) Math.Pow(10, double.Parse(val[1]));
        }

        /// <summary>
        /// Проверяет установлен ли в нагрузке такой модуль. Если модуль устновлен, то записывает номер канала в объект и
        /// возвращает его.
        /// Если модуль не устнановлен, то прописывает в объект отрицательный номер канала -1 и возвращает его, тогда объект не
        /// рабочий.
        /// </summary>
        /// <returns>true если модуль с такой моделью установлен</returns>
        public int FindThisModule()
        {
            //если в шасси стоит несколько блоков с одинаковой моделью, то метод завершит работу на первом же
            foreach (var model in GetInstalledModulesName())
            {
                //если модуль найден, то прописывается канал
                if (model.Type.Equals(ModuleModel))
                {
                    _chanNum = model.Channel;
                    break;
                }

                _chanNum = -1;
            }

            return _chanNum;
        }

        /// <summary>
        /// Возвращает ограничение тока на канале
        /// </summary>
        /// <returns></returns>
        public decimal GetCurrLevel()
        {
            return StrToDecimal(QueryLine("CURR:LEV?"));
        }

        /// <summary>
        /// Возвращает текущий предел измерения по току
        /// </summary>
        /// <returns></returns>
        public decimal GetCurrMeasRange()
        {
            return StrToDecimal(QueryLine("SENS:CURR:RANGE?"));
        }

        /// <summary>
        /// Возвращает массив с моделями установленных вставок нагрузки
        /// </summary>
        /// <returns></returns>
        public ModuleInfo[] GetInstalledModulesName()
        {
            var answer = QueryLine("*RDT?"); 
            var reg = new Regex(@"(?<=.+)\d+");
            return answer.Split(';').Select(mod => mod.Split(':'))
                         .Select(s => new ModuleInfo {Channel = int.Parse(reg.Match(s[0]).Value), Type = s[1]}).ToArray();
        }

        /// <summary>
        /// Задает ограничение по току на канале
        /// </summary>
        /// <param name = "currLevelIn"></param>
        /// <returns></returns>
        public bool SetCurrLevel(decimal currLevelIn)
        {
            WriteLine("CURR:LEV " + currLevelIn.ToString("G17", new CultureInfo("en-US")));
            Thread.Sleep(10);
            //!!!!   доделать с прибором   !!!!!
            var currLevAnswer = GetCurrLevel();

            return currLevelIn == currLevAnswer ? true : false;
        }

        /// <summary>
        /// Устанавливает МАКСИМАЛЬНЫЙ предел ИЗМЕРЯЕМОГО тока для нагрузки (зависит от модели вставки нагрузки)
        /// </summary>
        public void SetCurrMaxMeasRange()
        {
            WriteLine("SENS:CURR:RANGE MAX");
        }

        /// <summary>
        /// Устанавливает предел ИЗМЕРЯЕМОГО тока для нагрузки (зависит от модели вставки нагрузки)
        /// </summary>
        /// <param name = "inRange">Значение предела измерения тока</param>
        public bool SetCurrMeasRange(decimal inRange)
        {
            var moduleMaxRange = RangeCurrentArr.Last();
            if (inRange > moduleMaxRange) throw new ArgumentOutOfRangeException();

            WriteLine("SENSe:CURR:RANGe " + inRange.ToString("G17", new CultureInfo("en-US")));
            return true;
        }

        /// <summary>
        /// Устанавливает МИНИМАЛЬНЫЙ предел ИЗМЕРЯЕМОГО тока для нагрузки (зависит от модели вставки нагрузки)
        /// </summary>
        public void SetCurrMinMeasRange()
        {
            WriteLine("SENS:CURR:RANGE MIN");
        }

        /// <summary>
        /// Устанавливает режим нагрузки.
        /// </summary>
        /// <param name = "mode"></param>
        /// <returns></returns>
        public MainN3300 SetModeWork(ModeWorks mode)
        {
            WriteLine($@"FUNC {mode.GetStringValue()}");
            if (ModeWork == mode) return this;
            Logger.Error("Режим не установлен.");
            throw new Exception("Режим не установлен.");
        }

        public MainN3300 SetOutputState(State state)
        {
            WriteLine($"outp {state.GetStringValue()}");
            Thread.Sleep(10);
            if (StateOutput != state) throw new Exception("Состояние выхода не изменено.");
            return this;
        }

        /// <summary>
        /// Ограничение напряжения на канале для режима CV
        /// </summary>
        /// <param name = "inLevel"></param>
        public bool SetVoltLevel(decimal inLevel)
        {
            if (inLevel > RangeVoltArr.Last()) throw new ArgumentOutOfRangeException();

            //отправляем команду с уставкой по вольтам
            WriteLine("VOLT " + inLevel.ToString("G17", new CultureInfo("en-US")));
            Thread.Sleep(10);

            //удостоверимся, что значение принято
            var answer = QueryLine("VOLT?");

            //преобразуем строку в число
            var val = answer.TrimEnd('\n').Replace(".", ",").Split('E');
            var resultVoltLevel = decimal.Parse(val[0]) * (decimal) Math.Pow(10, double.Parse(val[1]));

            //если значение установилось, то ответ прибора будет тем же числом, что мы отправили на прибор - тогда вернем true
            return resultVoltLevel == inLevel;
        }

        /// <summary>
        /// Устанавливает МАКСИМАЛЬНЫЙ предел ИЗМЕРЯЕМОГО напряжения для нагрузки (зависит от модели вставки нагрузки)
        /// </summary>
        public void SetVoltMaxMeasRange()
        {
            WriteLine("SENS:VOLT:RANGE MAX");
        }

        /// <summary>
        /// Устанавливает предел ИЗМЕРЯЕМОГО напряжения для нагрузки (зависит от модели вставки нагрузки)
        /// </summary>
        /// <param name = "inRange">Значение предела измерения напряжения</param>
        public bool SetVoltMeasRange(decimal inRange)
        {
            var moduleMaxRange = RangeVoltArr.Last();
            if (inRange > moduleMaxRange) throw new ArgumentOutOfRangeException();

            WriteLine("SENSe:VOLTage:RANGe " + inRange.ToString("G17", new CultureInfo("en-US")));
            return true;
        }

        /// <summary>
        /// Устанавливает МИНИМАЛЬНЫЙ предел ИЗМЕРЯЕМОГО напряжения для нагрузки (зависит от модели вставки нагрузки)
        /// </summary>
        public void SetVoltMinMeasRange()
        {
            WriteLine("SENS:VOLT:RANGE MIN");
        }

        /// <summary>
        /// Устанавливает рабочий канал нагрузки
        /// </summary>
        /// <returns></returns>
        public MainN3300 SetWorkingChanel()
        {
            WriteLine("CHAN:LOAD " + _chanNum);
            if (int.Parse(QueryLine("CHAN:LOAD?")) == ChanelNumber) return this;
            Logger.Error("Канал не устанавлен");
            throw new NullReferenceException("Канал не устанавлен");
        }

        #endregion

        public struct ModuleInfo
        {
            /// <summary>
            /// Позволяет задать или получить номер канала.
            /// </summary>
            public int Channel { get; set; }

            /// <summary>
            /// Позволяет задать или получить Тип.
            /// </summary>
            public string Type { get; set; }
        }
    }

    public class Meas : HelpDeviceBase
    {
        #region Fields

        private readonly MainN3300 _mainN3300;

        #endregion

        #region Property

        /// <summary>
        /// Возвращает измеренное значение тока в цепи
        /// </summary>
        public decimal Current => (decimal) DataStrToDoubleMind(_mainN3300.QueryLine("MEAS:CURR?"));

        public decimal Power => (decimal) DataStrToDoubleMind(_mainN3300.QueryLine("MEAS:POWer?"));

        /// <summary>
        /// Возвращает измеренное значение напряжения на нагрузке
        /// </summary>
        public decimal Voltage => (decimal) DataStrToDoubleMind(_mainN3300.QueryLine("MEAS:VOLT?"));

        #endregion

        public Meas(MainN3300 mainN3300)
        {
            _mainN3300 = mainN3300;
            Multipliers = mainN3300.Multipliers;
        }
    }

    public class Resistance : HelpDeviceBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        private readonly MainN3300 _mainN3300;

        #endregion

        #region Property

        /// <summary>
        /// Возвращает маскимальное значение сопротивление на этой нагрузке
        /// </summary>
        /// <returns></returns>
        public ICommand MaxResistenceRange =>
            Ranges.FirstOrDefault(f => Equals(f.Value, Ranges.Select(s => s.Value).Max()));

        /// <summary>
        /// Предоставляет доступные диапазоны сопротивления.
        /// </summary>
        public ICommand[] Ranges { get; protected internal set; }

        #endregion

        public Resistance(MainN3300 mainN3300)
        {
            _mainN3300 = mainN3300;
            Multipliers = _mainN3300.Multipliers;
        }

        #region Methods

        /// <summary>
        /// Устанавливает ВЕЛИЧИНУ сопротивления для режима CR
        /// </summary>
        /// <param name = "value">Номирнал напряжения.</param>
        /// <param name = "mult">Множитель единицы измерения (милли, кило т.д.).</param>
        public MainN3300 Set(decimal value, Multipliers mult = AP.Utils.Helps.Multipliers.None)
        {
            if (value < 0)
                throw new ArgumentException("Значение меньше 0");

            var val = value * (decimal) mult.GetDoubleValue();
            _mainN3300.WriteLine($@"RESistance {JoinValueMult(val, mult)}");
            return _mainN3300;
        }

        /// <summary>
        /// Устанавливает максимальный предел воспроизведения сопротивления
        /// </summary>
        /// <returns></returns>
        public MainN3300 SetMaxResistanceRange()
        {
            _mainN3300.WriteLine("RESistance:RANGe MAX");
            return _mainN3300;
        }

        /// <summary>
        /// Устанавливает ПРЕДЕЛ сопротивления для режима CR
        /// </summary>
        /// <param name = "value">Значение сопротивления, которое нужно установить</param>
        public MainN3300 SetResistanceRange(decimal value, Multipliers mult = AP.Utils.Helps.Multipliers.None)
        {
            if (value < 0)
                throw new ArgumentException("Значение меньше 0");

            var val = value * (decimal) mult.GetDoubleValue();
            var res = Ranges.FirstOrDefault(q => q.Value >= (double) val);

            if (res == null)
            {
                Logger.Info(@"Входное знаечние больше допустимого,установлен максимальный предел.");
                res = Ranges.First(q => Equals(q.Value, Ranges.Select(p => p.Value).Max()));
            }

            _mainN3300.WriteLine(res.StrCommand);

            return _mainN3300;
        }

        #endregion
    }

    public class Voltage : HelpDeviceBase
    {
        #region Fields

        private readonly MainN3300 _mainN3300;

        #endregion

        #region Property

        public decimal MeasureVolt => (decimal) DataStrToDoubleMind(_mainN3300.QueryLine("MEAS:VOLT?"));

        #endregion

        public Voltage(MainN3300 mainN3300)
        {
            _mainN3300 = mainN3300;
            Multipliers = _mainN3300.Multipliers;
        }

        #region Methods

        /// <summary>
        /// Устанавливает значения напряжения (ограничение напряжения) для режима CV.
        /// </summary>
        /// <param name = "value">Номирнал напряжения.</param>
        /// <param name = "mult">Множитель единицы измерения (милли, кило т.д.).</param>
        /// <returns></returns>
        public MainN3300 Set(decimal value, Multipliers mult = AP.Utils.Helps.Multipliers.None)
        {
            if (value < 0)
                throw new ArgumentException("Значение меньше 0");

            var val = value * (decimal) mult.GetDoubleValue();
            _mainN3300.WriteLine($@"VOLTage {JoinValueMult(val, mult)}");
            return _mainN3300;
        }

        #endregion
    }

    public class Current : HelpDeviceBase
    {
        #region Fields

        private readonly MainN3300 _mainN3300;

        #endregion

        #region Property

        public decimal MeasureCurrent => (decimal) DataStrToDoubleMind(_mainN3300.QueryLine("MEAS:CURR?"));

        #endregion

        public Current(MainN3300 mainN3300)
        {
            _mainN3300 = mainN3300;
            Multipliers = _mainN3300.Multipliers;
        }

        #region Methods

        /// <summary>
        /// Устанавливает ВЕЛИЧИНУ тока для режима CC (ограничение тока).
        /// </summary>
        /// <param name = "value">Номирнал напряжения.</param>
        /// <param name = "mult">Множитель единицы измерения (милли, кило т.д.).</param>
        public MainN3300 Set(decimal value, Multipliers mult = AP.Utils.Helps.Multipliers.None)
        {
            if (value < 0)
                throw new ArgumentException("Значение меньше 0");

            var val = value * (decimal) mult.GetDoubleValue();
            _mainN3300.WriteLine($@"CURRent {JoinValueMult(val, mult)}");
            return _mainN3300;
        }

        #endregion
    }

    public class Power : HelpDeviceBase
    {
        #region Fields

        private readonly MainN3300 _mainN3300;

        #endregion

        #region Property

        public decimal MeasPower => (decimal) DataStrToDoubleMind(_mainN3300.QueryLine("MEAS:POWer?"));

        #endregion

        public Power(MainN3300 mainN3300)
        {
            _mainN3300 = mainN3300;
            Multipliers = _mainN3300.Multipliers;
        }
    }
}