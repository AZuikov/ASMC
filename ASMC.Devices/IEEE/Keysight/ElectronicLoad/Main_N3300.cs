using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using AP.Reports.Utils;
using NLog;

namespace ASMC.Devices.IEEE.Keysight.ElectronicLoad
{
    public abstract class MainN3300 : IeeeBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Номер канала, в котором установлен модуль нагрузки.
        /// </summary>
        private int _chanNum;

        //массивы с номиналами пределов модуля нагрузки
        //будет инициализироваться в конструкторе
        protected decimal[] RangeVoltArr;
        protected decimal[] RangeCurrentArr;
        protected string ModuleModel;

        protected MainN3300()
        {
            Resistance = new LResistance(this);
            Meas = new LMeas(this);
            this.UserType = "N3300A";
        }

        public enum VoltMultipliers
        {
          // [StringValue(" V")] Volt,
           [StringValue("")] None
        }

        public enum CurrMultipliers
        {
            //[StringValue(" A")] Amp,
            [StringValue("")] None
        }

        public enum ResistanceMultipliers
        {
            //[StringValue(" OHM")] Ohm,
            [StringValue("")] None
        }

        public string GetModuleModel
        {
            get =>ModuleModel;
        }

        /// <summary>
        /// Проверяет установлен ли в нагрузке такой модуль. Если модуль устновлен, то записывает номер канала в объект и возвращает его. 
        /// Если модуль не устнановлен, то прописывает в объект отрицательный номер канала -1 и возвращает его, тогда объект не рабочий. 
        /// </summary>
        /// <returns>true если модуль с такой моделью установлен</returns>
        public int FindThisModule()
        {
            //если в шасси стоит несколько блоков с одинаковой моделью, то метод завершит работу на первом же
            foreach (var model in GetInstalledModulesName())
            {
                //если модуль найден, то прописывается канал
                if (model.Type.Equals(this.ModuleModel))
                {
                    this._chanNum = model.Channel;
                    break;
                }

                this._chanNum = -1;
            }
            

            return this._chanNum;
        }

        /// <summary>
        /// Возвращает массив с моделями установленных вставок нагрузки
        /// </summary>
        /// <returns></returns>
        public Model[] GetInstalledModulesName()
        {
            this.WriteLine("*RDT?");
            Thread.Sleep(10);
            var answer = this.ReadLine().TrimEnd('\n');
            var reg = new Regex(@"(?<=.+)\d+");
            return answer.Split(';').Select(mod => mod.Split(':')).Select(s => new Model {Channel = int.Parse(reg.Match(s[0]).Value), Type = s[1]}).ToArray();
        }

        public struct Model
        {
            /// <summary>
            /// Позволяет задать или получить номер канала.
            /// </summary>
            public int Channel { get; set; }
            /// <summary>
            /// Позволяет задать или получить Тип.
            /// </summary>
            public string Type
            {
                get; set;
            }
        }

        /// <summary>
        /// Устанавливает рабочий канал нагрузки
        /// </summary>
        /// <returns></returns>
        public  MainN3300 SetWorkingChanel()
        {    
            this.WriteLine("CHAN:LOAD " + _chanNum);
            this.WriteLine("CHAN:LOAD?");
            if (int.Parse(this.ReadLine()) == ChanelNumber) return this;
            Logger.Error("Канал не устанавлен");
            throw new NullReferenceException("Канал не устанавлен");
        }



        /// <summary>
        /// Возвращает номер канала модуля нагрузки
        /// </summary>
        /// <returns></returns>
        public int ChanelNumber
        {
            get =>_chanNum;
        }
     
        public enum ModeWorks
        {
            /// <summary>
            /// Режим стабилизации тока.
            /// </summary>
            [StringValue("CURR")]
           Current,
            /// <summary>
            /// Режим нагрузки.
            /// </summary>
            [StringValue("RES")]
            Resistance,
            /// <summary>
            /// Режим стабелизации напряжения.
            /// </summary>
            [StringValue("VOLT")]
            Voltage
        }
        /// <summary>
        /// Отвечает, какой текущий режим канала
        /// </summary>
        public ModeWorks ModeWork
        {
            get
            {
                this.WriteLine("FUNC?");
                foreach (ModeWorks mode in Enum.GetValues(typeof(ModeWorks)) )
                {
                    if (mode.GetStringValue().Equals(this.ReadLine(), StringComparison.CurrentCultureIgnoreCase))
                        return mode;
                }
                Logger.Error("Режим не определен.");
                throw new Exception("Режим не определен.");
            }
        }
        /// <summary>
        /// Устанавливает режим нагрузки.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public MainN3300 SetModeWork(ModeWorks mode)
        {
            this.WriteLine($@"FUNC {mode.GetStringValue()}");
            if (ModeWork == mode) return this;
            Logger.Error("Режим не установлен.");
            throw new Exception("Режим не установлен.");

        }

        public LResistance Resistance { get; protected internal set; }

        public class LResistance:HelpIeeeBase
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
            private readonly MainN3300 _mainN3300;
            public ICommand[] Ranges { get; protected internal set; }
            public LResistance(MainN3300 mainN3300)
            {  
               _mainN3300 = mainN3300;
            }
            /// <summary>
            /// Возвращает маскимальное значение сопротивление на этой нагрузке
            /// </summary>
            /// <returns></returns>
            public ICommand MaxResistenceRange
            {
              get  => Ranges.FirstOrDefault(f=>Equals(f.Value, Ranges.Select(s => s.Value).Max()));
            }

            /// <summary>
            /// Устанавливает ПРЕДЕЛ сопротивления для режима CR 
            /// </summary>
            /// <param name="value">Значение сопротивления, которое нужно установить</param>
            public MainN3300 SetRange(decimal value, Multipliers mult = Devices.Multipliers.None)
            {

                if (value<0) throw  new ArgumentException("Значение меньше 0");

               var val = value* (decimal) mult.GetDoubleValue();
                var res = Ranges.FirstOrDefault(q => q.Value <= (double) val);

                if(res == null)
                {
                    Logger.Info($@"Входное знаечние больше допустимого,установлен максимальный предел.");
                    res = Ranges.First(q => Equals(q.Value, Ranges.Select(p => p.Value).Max()));
                }

                _mainN3300.WriteLine(res.StrCommand);

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
            /// Устанавливает ВЕЛИЧИНУ сопротивления для режима CR
            /// </summary>
            /// <param name = "value"></param>
            /// <param name = "mult"></param>
            public MainN3300 Set(decimal value, Multipliers mult = Devices.Multipliers.None)
            {
                if(value < 0)
                    throw new ArgumentException("Значение меньше 0");

                var val = value * (decimal)mult.GetDoubleValue();
                SetRange(val, mult); 
                _mainN3300.WriteLine($@"RESistance { this.JoinValueMult(val, mult)}");
                return _mainN3300;
            }

        }
        public class LVoltage
        {
            
        }
        public class LCurrent
        {
            
        }
       

        /// <summary>
        /// Задает ограничение по току на канале
        /// </summary>
        /// <param name="currLevelIn"></param>
        /// <returns></returns>
        public bool SetCurrLevel(decimal currLevelIn)
        {
            
            this.WriteLine("CURR:LEV " + currLevelIn.ToString("G17", new CultureInfo("en-US")));
            Thread.Sleep(10);
            //!!!!   доделать с прибором   !!!!!
            decimal currLevAnswer = this.GetCurrLevel();

            
            return currLevelIn == currLevAnswer? true: false;
        }

        /// <summary>
        /// Конвертирует строку в число decimal
        /// </summary>
        /// <param name="inStr"></param>
        /// <returns></returns>
        private static decimal StrToDecimal(string inStr)
        {
            var val = inStr.TrimEnd('\n').Replace(".", ",").Split('E');

            return decimal.Parse(val[0]) * (decimal)Math.Pow(10, double.Parse(val[1]));
        }

        /// <summary>
        /// Возвращает ограничение тока на канале
        /// </summary>
        /// <returns></returns>
        public decimal GetCurrLevel()
        {
            this.WriteLine("CURR:LEV?");
            return StrToDecimal(this.ReadLine());
        }
        public LMeas Meas { get; }
        public class LMeas
        {
            private readonly MainN3300 _mainN3300;

            public LMeas(MainN3300 mainN3300)
            {
                this._mainN3300 = mainN3300;
            }

            /// <summary>
            /// Возвращает измеренное значение напряжения на нагрузке
            /// </summary>
            public decimal Voltage
            {
                get
                {
                    _mainN3300.WriteLine("MEAS:VOLT?");
                    return StrToDecimal(_mainN3300.ReadLine());
                }
            }
            /// <summary>
            /// Возвращает измеренное значение тока в цепи
            /// </summary>
            public decimal Current
            {
                get
                {
                    _mainN3300.WriteLine("MEAS:CURR?");
                    return StrToDecimal(_mainN3300.ReadLine());
                }
            }
            public decimal Power
            {
                get
                {
                    _mainN3300.WriteLine("MEAS:POWer?");
                    return StrToDecimal(_mainN3300.ReadLine());
                }
            }
        }  


        /// <summary>
        /// Устанавливает предел ИЗМЕРЯЕМОГО тока для нагрузки (зависит от модели вставки нагрузки)
        /// </summary>
        /// <param name="inRange">Значение предела измерения тока</param>
        public  bool SetCurrMeasRange(decimal inRange)
        {
            decimal moduleMaxRange = RangeCurrentArr.Last();
            if (inRange > moduleMaxRange) throw new ArgumentOutOfRangeException();

            this.WriteLine("SENSe:CURR:RANGe " + inRange.ToString("G17", new CultureInfo("en-US")));
            return true;
        }

        /// <summary>
        /// Устанавливает предел ИЗМЕРЯЕМОГО напряжения для нагрузки (зависит от модели вставки нагрузки)
        /// </summary>
        /// <param name="inRange">Значение предела измерения напряжения</param>
        public  bool SetVoltMeasRange(decimal inRange)
        {
            decimal moduleMaxRange = RangeVoltArr.Last();
            if (inRange > moduleMaxRange) throw new ArgumentOutOfRangeException();

            this.WriteLine("SENSe:VOLTage:RANGe " + inRange.ToString("G17", new CultureInfo("en-US")));
            return true;

        }

       

        


        /// <summary>
        /// Устанавливает МАКСИМАЛЬНЫЙ предел ИЗМЕРЯЕМОГО тока для нагрузки (зависит от модели вставки нагрузки)
        /// </summary>
        public void SetCurrMaxMeasRange()
        {
            

            this.WriteLine("SENS:CURR:RANGE MAX");
            
            
        }

        /// <summary>
        /// Устанавливает МИНИМАЛЬНЫЙ предел ИЗМЕРЯЕМОГО тока для нагрузки (зависит от модели вставки нагрузки)
        /// </summary>
        public void SetCurrMinMeasRange()
        {
           
            this.WriteLine("SENS:CURR:RANGE MIN");
        }

        /// <summary>
        /// Возвращает текущий предел измерения по току
        /// </summary>
        /// <returns></returns>
        public decimal GetCurrMeasRange()
        {
            
            this.WriteLine("SENS:CURR:RANGE?");
            return StrToDecimal(this.ReadLine());
        }

        /// <summary>
        /// Ограничение напряжения на канале для режима CV
        /// </summary>
        /// <param name="inLevel"></param>
        public bool SetVoltLevel(decimal inLevel)
        {
            if (inLevel > RangeVoltArr.Last()) throw new ArgumentOutOfRangeException();
            
            //отправляем команду с уставкой по вольтам
            this.WriteLine("VOLT " + inLevel.ToString("G17", new CultureInfo("en-US")));
            Thread.Sleep(10);
            
            //удостоверимся, что значение принято
            this.WriteLine("VOLT?");
            string answer =this.ReadLine();
            
            //преобразуем строку в число
            string[] val = answer.TrimEnd('\n').Replace(".",",").Split('E');
            decimal resultVoltLevel = decimal.Parse(val[0]) * (decimal)System.Math.Pow(10, double.Parse(val[1]));

            //если значение установилось, то ответ прибора будет тем же числом, что мы отправили на прибор - тогда вернем true
            return resultVoltLevel == inLevel;
        }

        /// <summary>
        /// Устанавливает МАКСИМАЛЬНЫЙ предел ИЗМЕРЯЕМОГО напряжения для нагрузки (зависит от модели вставки нагрузки)
        /// </summary>
        public void SetVoltMaxMeasRange()
        {
            this.WriteLine("SENS:VOLT:RANGE MAX");
        }

        /// <summary>
        /// Устанавливает МИНИМАЛЬНЫЙ предел ИЗМЕРЯЕМОГО напряжения для нагрузки (зависит от модели вставки нагрузки)
        /// </summary>
        public void SetVoltMinMeasRange()
        {
            

            this.WriteLine("SENS:VOLT:RANGE MIN");

            
        }

        public enum State
        {
            [StringValue("outp 0")]
            Off,
            [StringValue("outp 1")]
            On
        }
        public MainN3300 SetOutputState(State state)
        {
            this.WriteLine(state.GetStringValue());
            Thread.Sleep(10);  
            if(StateOutput!=state) throw new Exception("Состояние выхода не изменено.");
            return this;
        }
       
       public State StateOutput
        {
            get
            {
                this.WriteLine("outp?");
                return (State) int.Parse(this.ReadLine());
            }
        }


       

    }

   
}
