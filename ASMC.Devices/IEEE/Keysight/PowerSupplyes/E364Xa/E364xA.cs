using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE.Keysight.PowerSupplyes;
using NLog;

namespace ASMC.Devices.IEEE.Keysight.PowerSupplies
{
    public abstract class E364xA : IeeeBase
    {
        public enum Chanel
        {
            OUTP1,
            OUTP2
        }

        private Chanel _chanel;

        public Chanel ActiveChanel
        {
            get
            {
                return _chanel;
            }
            set
            {
                _chanel = value;
                WriteLine($"inst {_chanel.ToString()}");
            }
        }



        /// <summary>
        /// Пределы изменения напряжения и тока (зависят от модели прибора).
        /// </summary>
        public MeasPoint<Voltage,Current>[] Ranges { get; protected set; }
        /// <summary>
        /// Текущий предел источника.
        /// </summary>
        private MeasPoint<Voltage, Current> range;


        public enum TriggerSource
        {
            BUS,
            IMMediate
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Property

        public CURRent CURR { get; }
        public MEASure MEAS { get; }
        public Output OUT { get; }
        public TRIGger TRIG { get; }
        public VOLTage VOLT { get; }

        #endregion

        public E364xA()
        {
            UserType = "E364XA";
            CURR = new CURRent(this);
            VOLT = new VOLTage(this);
            MEAS = new MEASure(this);
            OUT = new Output(this);
            TRIG = new TRIGger(this);
        }

        #region Methods

        public Chanel GetActiveChanel()
        {
            var answer = QueryLine("inst?");
            foreach (Chanel chanel in Enum.GetValues(typeof(Chanel)))
                if (chanel.ToString().Equals(answer))
                    return chanel;

            var errorStr = $"Запрос активного канала E364XA. Прибор ответил: {answer}";
            Logger.Error(errorStr);
            throw new Exception(errorStr);
        }

       

        private MeasPoint<IPhysicalQuantity>
            LocalMeasPointRound<IPhysicalQuantity>(MeasPoint<IPhysicalQuantity> inPoint)
            where IPhysicalQuantity : class, IPhysicalQuantity<IPhysicalQuantity>, new()
        {
            var str = inPoint.MainPhysicalQuantity.Value.ToString().Replace(',', '.').Split('.');
            //сплит по разделителю
            //берем первый символ
            //склеиваем всё обратно в число
            var buf = "";
            var result = "";
            if (str.Length > 1)
            {
                buf = str[1].Substring(0, 1);
                result = $"{str[0]}.{buf}";
            }
            else
            {
                result = str[0];
            }

            decimal val;
            decimal.TryParse(result, out val);
            inPoint.MainPhysicalQuantity.Value = val;
            return inPoint;
        }

        #endregion


       

        public void SetRange(MeasPoint<Voltage, Current> inRange)
        {
            WriteLine($"VOLTage:RANGe P{inRange.MainPhysicalQuantity.Value}V");

            MeasPoint<Voltage> volt = new MeasPoint<Voltage>();
            volt.MainPhysicalQuantity = inRange.MainPhysicalQuantity;
            SetVoltageLevel(volt);

            MeasPoint<Current> current = new MeasPoint<Current>();
            current.MainPhysicalQuantity = inRange.AdditionalPhysicalQuantity;
            SetCurrentLevel(current);
            
            range = inRange;
        }

        public MeasPoint<Voltage, Current> GetRange()
        {
            return range;
        }

        public MeasPoint<Voltage, Current>[] GetAllRanges()
        {
            return Ranges;
        }

        public void SetVoltageLevel(MeasPoint<Voltage> inPoint)
        {
            VOLT.SetValue(inPoint);
        }

        public void SetMaxVoltageLevel()
        {
            WriteLine("VOLTage:AMPLitude MAX");
        }

        public MeasPoint<Voltage> GetVoltageLevel()
        {
            var answer = QueryLine("VOLTage:AMPLitude?");
            var numb = (decimal) StrToDouble(answer);
            return new MeasPoint<Voltage>(numb);
        }

        
        public void SetCurrentLevel(MeasPoint<Current> inPoint)
        {
            CURR.SetValue(inPoint);
        }

        public void SetMaxCurrentLevel()
        {
            WriteLine("CURRent:AMPLitude MAX");
        }

        public MeasPoint<Current> GetCurrentLevel()
        {
            var answer = QueryLine("CURRent:AMPLitude?");
            var numb = (decimal) StrToDouble(answer);
            return new MeasPoint<Current>(numb);
        }

        

        public void OutputOn()
        {
            OUT.OutputOn();
        }

        public void OutputOff()
        {
            OUT.OutputOff();
        }

       

        public MeasPoint<Voltage> GetVoltageRange()
        {
            var answer = QueryLine("VOLTage:RANGe?");
            var regex = new Regex(@"\d+", RegexOptions.IgnoreCase);
            var match = regex.Match(answer);
            decimal numb;
            decimal.TryParse(match.Value, out numb);
            return new MeasPoint<Voltage>(numb);
        }

        public MeasPoint<Voltage> GetMeasureVoltage()
        {
            return MEAS.GetMeasureVoltage();
        }

        public MeasPoint<Current> GetMeasureCurrent()
        {
            return MEAS.GetMeasureCurrent();
        }

        public class CURRent
        {
            #region Fields

            private readonly E364xA _powerSupply;

            private readonly string Comand = "CURRent:LEVel:IMMediate:AMPLitude";

            #endregion

            public CURRent(E364xA powerSupply)
            {
                _powerSupply = powerSupply;
            }

            #region Methods

            public MeasPoint<Current> GetValue()
            {
                var answer = _powerSupply.QueryLine($"{Comand}?");
                var numAnswer = (decimal) StrToDouble(answer);
                var returnPoint = new MeasPoint<Current>(numAnswer);
                return returnPoint;
            }

            public E364xA SetValue(MeasPoint<Current> inPoint)
            {
                _powerSupply
                   .WriteLine($"{Comand} {inPoint.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
                return _powerSupply;
            }

            #endregion
        }

        public class VOLTage
        {
            #region Fields

            private readonly E364xA _powerSupply;
            private readonly string ComandeRange = "VOLTage:RANGe";
            private readonly string ComandtoSetValue = "VOLTage:LEVel:IMMediate:AMPLitude";

            #endregion

            public VOLTage(E364xA powerSupply)
            {
                _powerSupply = powerSupply;
            }

            #region Methods

            

            public MeasPoint<Voltage> GetValue()
            {
                var answer = _powerSupply.QueryLine($"{ComandtoSetValue}?");
                var numAnswer = (decimal) StrToDouble(answer);
                var returnPoint = new MeasPoint<Voltage>(numAnswer);
                return returnPoint;
            }

           

            public E364xA SetValue(MeasPoint<Voltage> inPoint)
            {
                _powerSupply
                   .WriteLine($"{ComandtoSetValue} {inPoint.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
                return _powerSupply;
            }

            #endregion
        }

        public class MEASure
        {
            #region Fields

            private readonly E364xA _powerSupply;

            #endregion

            public MEASure(E364xA powerSupply)
            {
                _powerSupply = powerSupply;
            }

            #region Methods

            public MeasPoint<Current> GetMeasureCurrent()
            {
                var answer = _powerSupply.QueryLine("MEASure::CURR:DC?");
                var numberAnswer = (decimal) StrToDouble(answer.Replace(',', '.'));
                var answerPoint = new MeasPoint<Current>(numberAnswer);
                return answerPoint;
            }

            public MeasPoint<Voltage> GetMeasureVoltage()
            {
                var answer = _powerSupply.QueryLine("MEASure::VOLT:DC?");
                var numberAnswer = (decimal) StrToDouble(answer.Replace(',', '.'));
                var answerPoint = new MeasPoint<Voltage>(numberAnswer);
                return answerPoint;
            }

            #endregion
        }

        public class TRIGger
        {
            #region Fields

            private readonly E364xA _powerSupply;

            #endregion

            public TRIGger(E364xA powerSupply)
            {
                _powerSupply = powerSupply;
            }

            #region Methods

            public decimal GetTriggerDelay()
            {
                var answer = _powerSupply.QueryLine("TRIGger:DELay?");
                var returnNumb = (decimal) StrToDouble(answer);
                return returnNumb;
            }

            public TriggerSource GetTriggerSource()
            {
                var answer = _powerSupply.QueryLine("TRIGger:SOURce?");
                answer = answer.TrimEnd('\n');
                foreach (TriggerSource source in Enum.GetValues(typeof(TriggerSource)))
                    if (source.ToString().Equals(answer))
                        return source;
                var errorStr = $"Запрос источника триггера! E364xA выдал непредвиденный результат: {answer}";
                Logger.Error(errorStr);
                throw new Exception(errorStr);
            }

            public E364xA InitTrigger()
            {
                _powerSupply.WriteLine("INITiate");
                return _powerSupply;
            }

            public E364xA SetTriggerDelay(int millisecond)
            {
                if (millisecond < 0) millisecond = 0;
                if (millisecond > 3600) millisecond = 3600;
                {
                }
                _powerSupply.WriteLine($"TRIGger:DELay {millisecond}");
                return _powerSupply;
            }

            public E364xA SetTriggerSource(TriggerSource inSource)
            {
                _powerSupply.WriteLine($"TRIGger:SOURce {inSource}");
                return _powerSupply;
            }

            public E364xA TRG()
            {
                _powerSupply.WriteLine("*TRG");
                return _powerSupply;
            }

            #endregion
        }

        public class Output
        {
            #region Fields

            private readonly E364xA _powerSupply;
            private readonly string Comand = "OUTPut";

            #endregion

            public Output(E364xA powerSupply)
            {
                _powerSupply = powerSupply;
            }

            #region Methods

            public bool IsOutputOn()
            {
                var answer = _powerSupply.QueryLine($"{Comand}?");
                answer = answer.TrimEnd('\n');
                if (answer.Equals("1")) return true;
                return false;
            }

            public E364xA OutputOff()
            {
                _powerSupply.WriteLine($"{Comand} off");
                return _powerSupply;
            }

            public E364xA OutputOn()
            {
                _powerSupply.WriteLine($"{Comand} on");
                return _powerSupply;
            }

            #endregion
        }
    }

    public class E3648A : E364xA
    {
        public E3648A()
        {
            Ranges = new[]
            {
                new MeasPoint<Voltage, Current>(8, 5),
                new MeasPoint<Voltage, Current>(20M, 2.5M)
            };
            
        }
    }
}