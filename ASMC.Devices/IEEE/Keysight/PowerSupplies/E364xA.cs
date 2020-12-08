﻿using System;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using NLog;

namespace ASMC.Devices.IEEE.Keysight.PowerSupplies
{
    internal class E364xA : IeeeBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public enum Chanel
        {
            OUTP1,
            OUTP2
        }

        /// <summary>
        /// Пределы изменения напряжения и тока (зависят от модели прибора).
        /// </summary>
        public enum RangePowerSupply
        {
            [StringValue("P8V")]P8V,
            [StringValue("P20V")]P20V,
            [StringValue("P35V")]P35V,
            [StringValue("P60V")]P60V,
            [StringValue("LOW")]LOW,
            [StringValue("HIGH")]HIGH
        }

        public enum TriggerSource
        {
            BUS,
            IMMediate
        }

        #region Property

        public CURRent CURR { get; }
        public MEASure MEAS { get; }
        public Output OUT { get; }
        public TRIGger TRIG { get; }
        public VOLTage VOLT { get; }

        #endregion

        public E364xA()
        {
            CURR = new CURRent(this);
            VOLT = new VOLTage(this);
            MEAS = new MEASure(this);
            OUT = new Output(this);
            TRIG = new TRIGger(this);
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
            private readonly string ComandtoSetValue = "VOLTage:LEVel:IMMediate:AMPLitude";
            private readonly string ComandeRange = "VOLTage:RANGe";

            #endregion

            public VOLTage(E364xA powerSupply)
            {
                _powerSupply = powerSupply;
            }

            #region Methods

            public MeasPoint<Voltage> GetValue()
            {
                var answer = _powerSupply.QueryLine($"{ComandtoSetValue}?");
                var numAnswer = (decimal) StrToDoubleMindMind(answer);
                var returnPoint = new MeasPoint<Voltage>(numAnswer);
                return returnPoint;
            }

            public E364xA SetValue(MeasPoint<Voltage> inPoint)
            {
                _powerSupply
                   .WriteLine($"{ComandtoSetValue} {inPoint.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
                return _powerSupply;
            }

            public RangePowerSupply GetRange()
            {
                string answer = _powerSupply.QueryLine($"{ComandeRange}?");
                answer = answer.TrimEnd('\n');
                foreach (RangePowerSupply range in Enum.GetValues(typeof(RangePowerSupply)))
                {
                    if (range.ToString().Equals(answer))
                        return range;
                }

                string errorStr = $"Запрос диапазона! E364xA выдал непредвиденный результат: {answer}";
                Logger.Error(errorStr);

                throw new Exception(errorStr);
            }

            public E364xA SetRange(RangePowerSupply inRange)
            {
                _powerSupply.WriteLine($"{ComandeRange} {inRange.ToString()}");
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

            public MeasPoint<Current> GetMeasureCurrent()
            {
                string answer = _powerSupply.QueryLine("MEASure::CURR:DC?");
                decimal numberAnswer = (decimal)StrToDoubleMindMind(answer.Replace(',', '.'));
                MeasPoint<Current> answerPoint = new MeasPoint<Current>(numberAnswer);
                return answerPoint;
            }

            public MeasPoint<Voltage> GetMeasureVoltage()
            {
                string answer = _powerSupply.QueryLine("MEASure::VOLT:DC?");
                decimal numberAnswer = (decimal)StrToDoubleMindMind(answer.Replace(',', '.'));
                MeasPoint<Voltage> answerPoint = new MeasPoint<Voltage>(numberAnswer);
                return answerPoint;
            }

            
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

            public decimal GetTriggerDelay()
            {
               string answer = _powerSupply.QueryLine("TRIGger:DELay?");
               answer = answer.TrimEnd('\n');
               decimal returnNumb = (decimal) StrToDoubleMindMind(answer);
               return returnNumb;
            }

            public E364xA SetTriggerSource(TriggerSource inSource)
            {
                _powerSupply.WriteLine($"TRIGger:SOURce {inSource}");
                return _powerSupply;
            }

            public TriggerSource GetTriggerSource()
            {
                string answer = _powerSupply.QueryLine("TRIGger:SOURce?");
                answer = answer.TrimEnd('\n');
                foreach (TriggerSource source in Enum.GetValues(typeof(TriggerSource)))
                {
                    if (source.ToString().Equals(answer))
                        return source;
                }
                string errorStr = $"Запрос источника триггера! E364xA выдал непредвиденный результат: {answer}";
                Logger.Error(errorStr);
                throw new Exception(errorStr);
            }

            public E364xA TRG()
            {
                _powerSupply.WriteLine("*TRG");
                return _powerSupply;
            }

        }

        public class Output
        {
            #region Fields

            private readonly E364xA _powerSupply;
            string Comand="OUTPut";
            #endregion

            public Output(E364xA powerSupply)
            {
                _powerSupply = powerSupply;
            }

            public E364xA OutputOn()
            {
                _powerSupply.WriteLine($"{Comand} on");
                return _powerSupply;
            }

            public E364xA OutputOff()
            {
                _powerSupply.WriteLine($"{Comand} off");
                return _powerSupply;
            }

            public bool IsOutputOn()
            {
                string answer = _powerSupply.QueryLine($"{Comand}?");
                answer = answer.TrimEnd('\n');
                if (answer.Equals("1")) return true;
                return false;
            }
        }
    }
}