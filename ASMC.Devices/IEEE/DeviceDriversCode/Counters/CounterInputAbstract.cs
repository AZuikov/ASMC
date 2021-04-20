using AP.Utils.Data;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;

namespace ASMC.Devices.IEEE.PENDULUM
{
    public abstract class CounterInputAbstract: ICounterInput
    {
        #region Enums

        public enum InputCouple
        { AC, DC }

        public enum InputImpedance
        {
            [DoubleValue(50)] IMP50Ohm,
            [DoubleValue(1e6)] IMPMegaOhm
        }

        public enum InputAttenuator
        {
            ATT1,
            ATT10
        }

        public enum InputSlope
        {
            POS,
            NEG
        }

        protected enum Status
        {ON, OFF}

        #endregion

        private CounterAbstract _counter;
        public CounterInputAbstract(string chanelName, CounterAbstract counter)
        {
            NameOfChanel = chanelName;
            InputSetting = ChanelSetting.getInstance();
            _counter = counter;
            
        }


        public string NameOfChanel { get; }
        public  ITypicalCounterInputSettings InputSetting { get; set; }
        public IMeterPhysicalQuantity<Frequency> MeasFrequency { get; set; }
        public IMeterPhysicalQuantity<Frequency> MeasFrequencyBURSt { get; set; }
        public IMeterPhysicalQuantity<NoUnits> MeasNumberOfCyclesInBurst { get; set; }
        public IMeterPhysicalQuantity<Power> MeasPowerAC_Signal { get; set; }
        public IMeterPhysicalQuantity<Percent> MeasPulseRepetitionFrequencyBurstSignal { get; set; }
        public IMeterPhysicalQuantity<Percent> MeasPositiveDutyFactor { get; set; }
        public IMeterPhysicalQuantity<Percent> MeasNegativeDutyFactor { get; set; }
        public IMeterPhysicalQuantity<NoUnits> MeasFrequencyRatio { get; set; }
        public IMeterPhysicalQuantity<Voltage> MeasMaximum { get; set; }
        public IMeterPhysicalQuantity<Voltage> MeasMinimum { get; set; }
        public IMeterPhysicalQuantity<Voltage> MeasPeakToPeak { get; set; }
        public IMeterPhysicalQuantity<Voltage> MeasRatio { get; set; }
        public IMeterPhysicalQuantity<Time> MeasPeriod { get; set; }
        public IMeterPhysicalQuantity<Time> MeasPeriodAver { get; set; }
        public IMeterPhysicalQuantity<Time> MeasTimeInterval { get; set; }
        public IMeterPhysicalQuantity<Time> MeasPositivePulseWidth { get; set; }
        public IMeterPhysicalQuantity<Time> MeasNegativePulseWidth { get; set; }
        public IMeterPhysicalQuantity<Degreas> MeasPhase { get; set; }

        /// <summary>
        /// Настройка канала.
        /// </summary>
        public class ChanelSetting : ITypicalCounterInputSettings
        {
            /*Настройки у канала должны быть одни, поэтому будет синглтон!!!*/

            public InputAttenuator Attenuator { get; protected set; }
            public InputImpedance Impedance { get; protected set; }
            public InputCouple Couple { get; protected set; }
            public InputSlope Slope { get; protected set; }

            
            private static ChanelSetting instance;

            private ChanelSetting()
            {
                //настройки канала по умолчаниюа 
                Attenuator = InputAttenuator.ATT1;
                Impedance = InputImpedance.IMP50Ohm;
                Couple = InputCouple.DC;
                Slope = InputSlope.POS;
            }

            public static ChanelSetting getInstance()
            {
                if (instance == null)
                {
                    instance = new ChanelSetting();
                }

                return instance;
            }

            public virtual void SetAtt_1()
            {
                Attenuator = InputAttenuator.ATT1;
            }

            public virtual void SetAtt_10()
            {
                Attenuator = InputAttenuator.ATT10;
            }

            public virtual void SetHightImpedance()
            {
                Impedance = InputImpedance.IMPMegaOhm;
            }

            public virtual void SetLowImpedance()
            {
                Impedance = InputImpedance.IMP50Ohm;
            }

            public virtual void SetCoupleAC()
            {
                Couple = InputCouple.AC;
            }

            public virtual void SetCoupleDC()
            {
                Couple = InputCouple.DC;
            }

            public virtual void SetInputSlopePositive()
            {
                Slope = InputSlope.POS;
            }

            public virtual void SetInputSlopeNegative()
            {
                Slope = InputSlope.NEG;
            }
        }
    }
}