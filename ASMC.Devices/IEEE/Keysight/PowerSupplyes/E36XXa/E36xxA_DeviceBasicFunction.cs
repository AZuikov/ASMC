using System;
using System.Text.RegularExpressions;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using NLog;

namespace ASMC.Devices.IEEE.Keysight.PowerSupplyes
{
    public enum E36xxChanels
    {
        OUTP1,
        OUTP2
    }

    public enum E36xxA_Ranges
    {
        LOW,
        HIGH
    }

    public class E36xxA_DeviceBasicFunction : IeeeBase
    {
        public enum TriggerSource
        {
            BUS,
            IMMediate
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields

        protected E36xxChanels _e36XxChanels;

        /// <summary>
        /// Текущий предел источника.
        /// </summary>
        private MeasPoint<Voltage, Current> range;

        #endregion

        #region Property

       

        public CURRent CURR { get; }
        public MEASure MEAS { get; }
        public Output OUT { get; }
        public E36xxChanels[] outputs { get; protected set; }

        /// <summary>
        /// Пределы изменения напряжения и тока (зависят от модели прибора).
        /// </summary>
        public  MeasPoint<Voltage, Current>[] Ranges { get; protected set; }

        public TRIGger TRIG { get; }
        public VOLTage VOLT { get; }

        #endregion

        public E36xxA_DeviceBasicFunction()
        {
            UserType = "E36XXA";
            CURR = new CURRent(this);
            VOLT = new VOLTage(this);
            MEAS = new MEASure(this);
            OUT = new Output(this);
            TRIG = new TRIGger(this);
        }

        public static explicit operator E36xxA_DeviceBasicFunction(Type v)
        {
            throw new NotImplementedException();
        }

        #region Methods





        public MeasPoint<Current> GetCurrentLevel()
        {
            var answer = QueryLine("CURRent:AMPLitude?");
            var numb = (decimal) StrToDouble(answer);
            return new MeasPoint<Current>(numb);
        }

        public MeasPoint<Current> GetMeasureCurrent()
        {
            return MEAS.GetMeasureCurrent();
        }

        public MeasPoint<Voltage> GetMeasureVoltage()
        {
            return MEAS.GetMeasureVoltage();
        }

        public MeasPoint<Voltage, Current> GetRange()
        {
            return range;
        }

        public MeasPoint<Voltage> GetVoltageLevel()
        {
            var answer = QueryLine("VOLTage:AMPLitude?");
            var numb = (decimal) StrToDouble(answer);
            return new MeasPoint<Voltage>(numb);
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

        public void OutputOff()
        {
            OUT.OutputOff();
        }

        public void OutputOn()
        {
            OUT.OutputOn();
        }

        public void SetCurrentLevel(MeasPoint<Current> inPoint)
        {
            CURR.SetValue(inPoint);
        }

        public void SetMaxCurrentLevel()
        {
            WriteLine("CURRent:AMPLitude MAX");
        }

        public void SetMaxVoltageLevel()
        {
            WriteLine("VOLTage:AMPLitude MAX");
        }

        public void SetRange(E36xxA_Ranges inRange)
        {
            WriteLine($"VOLTage:RANGe {inRange}");
            range = inRange== E36xxA_Ranges.LOW? Ranges[0]: Ranges[1];
            //todo пределы могут быть установлены командой с указанием номинала напряжения... это тоже нужно реализовать
        }

        public void SetVoltageLevel(MeasPoint<Voltage> inPoint)
        {
            VOLT.SetValue(inPoint);
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

        public class CURRent
        {
            #region Fields

            private readonly E36xxA_DeviceBasicFunction _powerSupply;

            private readonly string Comand = "CURRent:LEVel:IMMediate:AMPLitude";

            #endregion

            public CURRent(E36xxA_DeviceBasicFunction powerSupply)
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

            public E36xxA_DeviceBasicFunction SetValue(MeasPoint<Current> inPoint)
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

            private readonly E36xxA_DeviceBasicFunction _powerSupply;
            private readonly string ComandeRange = "VOLTage:RANGe";
            private readonly string ComandtoSetValue = "VOLTage:LEVel:IMMediate:AMPLitude";

            #endregion

            public VOLTage(E36xxA_DeviceBasicFunction powerSupply)
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

            public E36xxA_DeviceBasicFunction SetValue(MeasPoint<Voltage> inPoint)
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

            private readonly E36xxA_DeviceBasicFunction _powerSupply;

            #endregion

            public MEASure(E36xxA_DeviceBasicFunction powerSupply)
            {
                _powerSupply = powerSupply;
            }

            #region Methods

            public MeasPoint<Current> GetMeasureCurrent()
            {
                var answer = _powerSupply.QueryLine("MEASure:CURR:DC?");
                var numberAnswer = (decimal) StrToDouble(answer.Replace(',', '.'));
                var answerPoint = new MeasPoint<Current>(numberAnswer);
                return answerPoint;
            }

            public MeasPoint<Voltage> GetMeasureVoltage()
            {
                var answer = _powerSupply.QueryLine("MEASure:VOLT:DC?");
                var numberAnswer = (decimal) StrToDouble(answer.Replace(',', '.'));
                var answerPoint = new MeasPoint<Voltage>(numberAnswer);
                return answerPoint;
            }

            #endregion
        }

        public class TRIGger
        {
            #region Fields

            private readonly E36xxA_DeviceBasicFunction _powerSupply;

            #endregion

            public TRIGger(E36xxA_DeviceBasicFunction powerSupply)
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

            public E36xxA_DeviceBasicFunction InitTrigger()
            {
                _powerSupply.WriteLine("INITiate");
                return _powerSupply;
            }

            public E36xxA_DeviceBasicFunction SetTriggerDelay(int millisecond)
            {
                if (millisecond < 0) millisecond = 0;
                if (millisecond > 3600) millisecond = 3600;
                {
                }
                _powerSupply.WriteLine($"TRIGger:DELay {millisecond}");
                return _powerSupply;
            }

            public E36xxA_DeviceBasicFunction SetTriggerSource(TriggerSource inSource)
            {
                _powerSupply.WriteLine($"TRIGger:SOURce {inSource}");
                return _powerSupply;
            }

            public E36xxA_DeviceBasicFunction TRG()
            {
                _powerSupply.WriteLine("*TRG");
                return _powerSupply;
            }

            #endregion
        }

        public class Output
        {
            #region Fields

            private readonly E36xxA_DeviceBasicFunction _powerSupply;
            private readonly string Comand = "OUTPut";

            #endregion

            public Output(E36xxA_DeviceBasicFunction powerSupply)
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

            public E36xxA_DeviceBasicFunction OutputOff()
            {
                _powerSupply.WriteLine($"{Comand} off");
                return _powerSupply;
            }

            public E36xxA_DeviceBasicFunction OutputOn()
            {
                _powerSupply.WriteLine($"{Comand} on");
                return _powerSupply;
            }

            #endregion
        }
    }

   
}