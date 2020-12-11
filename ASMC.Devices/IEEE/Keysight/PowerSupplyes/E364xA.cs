using System;
using System.Text.RegularExpressions;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE.Keysight.PowerSupplyes;
using NLog;

namespace ASMC.Devices.IEEE.Keysight.PowerSupplies
{
    public class E364xA : IeeeBase, E36XX_IPowerSupply
    {
        public enum Chanel
        {
            [StringValue("OUTP1")]OUTP1,
            [StringValue("OUTP2")]OUTP2
        }

        /// <summary>
        /// Пределы изменения напряжения и тока (зависят от модели прибора).
        /// </summary>
        public enum RangePowerSupply
        {
            [StringValue("LOW")] LOW,
            [StringValue("HIGH")] HIGH
        }

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

        public E364xA SetActiveChanel(Chanel inChanel)
        {
            WriteLine($"inst {inChanel.GetStringValue()}");
            return this;
        }

        #endregion

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
            string answer = QueryLine("VOLTage:AMPLitude?");
            decimal numb = (decimal)StrToDoubleMindMind(answer);
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
            string answer = QueryLine("CURRent:AMPLitude?");
            decimal numb = (decimal)StrToDoubleMindMind(answer);
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

        public void SetHighVoltageRange()
        {
            VOLT.SetRange(RangePowerSupply.HIGH);
        }

        public void SetLowVoltageRange()
        {
            VOLT.SetRange(RangePowerSupply.LOW);
        }

        public MeasPoint<Voltage> GetVoltageRange()
        {
            string answer = QueryLine("VOLTage:RANGe?");
            Regex regex  = new Regex("P\\d+V\n",RegexOptions.IgnoreCase);
            Match match = regex.Match(answer);
            decimal numb;
            decimal.TryParse(match.Value,out numb);
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
                var numAnswer = (decimal) StrToDoubleMindMind(answer);
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

            public RangePowerSupply GetRange()
            {
                var answer = _powerSupply.QueryLine($"{ComandeRange}?");
                answer = answer.TrimEnd('\n');
                foreach (RangePowerSupply range in Enum.GetValues(typeof(RangePowerSupply)))
                    if (range.ToString().Equals(answer))
                        return range;

                var errorStr = $"Запрос диапазона! E364xA выдал непредвиденный результат: {answer}";
                Logger.Error(errorStr);

                throw new Exception(errorStr);
            }

            public MeasPoint<Voltage> GetValue()
            {
                var answer = _powerSupply.QueryLine($"{ComandtoSetValue}?");
                var numAnswer = (decimal) StrToDoubleMindMind(answer);
                var returnPoint = new MeasPoint<Voltage>(numAnswer);
                return returnPoint;
            }

            public E364xA SetRange(RangePowerSupply inRange)
            {
                _powerSupply.WriteLine($"{ComandeRange} {inRange.ToString()}");
                return _powerSupply;
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
                var numberAnswer = (decimal) StrToDoubleMindMind(answer.Replace(',', '.'));
                var answerPoint = new MeasPoint<Current>(numberAnswer);
                return answerPoint;
            }

            public MeasPoint<Voltage> GetMeasureVoltage()
            {
                var answer = _powerSupply.QueryLine("MEASure::VOLT:DC?");
                var numberAnswer = (decimal) StrToDoubleMindMind(answer.Replace(',', '.'));
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
                var returnNumb = (decimal) StrToDoubleMindMind(answer);
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
}