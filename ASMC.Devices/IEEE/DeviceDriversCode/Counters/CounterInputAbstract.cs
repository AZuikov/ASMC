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
            InputSetting = new ChanelSetting();
            _counter = counter;
            
        }


        public string NameOfChanel { get; }
        public ITypicalCounterInputSettings InputSetting { get; }
        public IMeterPhysicalQuantity<Frequency> MeasFrequency { get; }
        public IMeterPhysicalQuantity<Frequency> MeasFrequencyBURSt { get; }
        public IMeterPhysicalQuantity<NoUnits> MeasNumberOfCyclesInBurst { get; }
        public IMeterPhysicalQuantity<Power> MeasPowerAC_Signal { get; }
        public IMeterPhysicalQuantity<Percent> MeasPulseRepetitionFrequencyBurstSignal { get; }
        public IMeterPhysicalQuantity<Percent> MeasPositiveDutyFactor { get; }
        public IMeterPhysicalQuantity<Percent> MeasNegativeDutyFactor { get; }
        public IMeterPhysicalQuantity<NoUnits> MeasFrequencyRatio { get; }
        public IMeterPhysicalQuantity<Voltage> MeasMaximum { get; }
        public IMeterPhysicalQuantity<Voltage> MeasMinimum { get; }
        public IMeterPhysicalQuantity<Voltage> MeasPeakToPeak { get; }
        public IMeterPhysicalQuantity<Voltage> MeasRatio { get; }
        public IMeterPhysicalQuantity<Time> MeasPeriod { get; }
        public IMeterPhysicalQuantity<Time> MeasPeriodAver { get; }
        public IMeterPhysicalQuantity<Time> MeasTimeInterval { get; }
        public IMeterPhysicalQuantity<Time> MeasPositivePulseWidth { get; }
        public IMeterPhysicalQuantity<Time> MeasNegativePulseWidth { get; }
        public IMeterPhysicalQuantity<Degreas> MeasPhase { get; }

        public class ChanelSetting : ITypicalCounterInputSettings
        {
            public InputAttenuator Attenuator { get; protected set; }
            public InputImpedance Impedance { get; protected set; }
            public InputCouple Couple { get; protected set; }
            public InputSlope Slope { get; protected set; }

            public ChanelSetting()
            {
                //настройки канала по умолчаниюа 
                Attenuator = InputAttenuator.ATT1;
                Impedance = InputImpedance.IMP50Ohm;
                Couple = InputCouple.DC;
                Slope = InputSlope.POS;
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