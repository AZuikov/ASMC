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

        private CounterAbstract _counterAbstract;
        public CounterInputAbstract(string chanelName, CounterAbstract counterAbstract)
        {
            NameOfChanel = chanelName;
            InputSetting = new ChanelSetting();
            _counterAbstract = counterAbstract;
        }


        public string NameOfChanel { get; }
        public ITypicalCounterInputSettings InputSetting { get; }
        public IMeterPhysicalQuantity<Frequency> Frequency { get; }
        public IMeterPhysicalQuantity<Frequency> FrequencyBURSt { get; }
        public IMeterPhysicalQuantity<NoUnits> NumberOfCyclesInBurst { get; }
        public IMeterPhysicalQuantity<Power> PowerAC_Signal { get; }
        public IMeterPhysicalQuantity<Percent> PulseRepetitionFrequencyBurstSignal { get; }
        public IMeterPhysicalQuantity<Percent> PositiveDutyFactor { get; }
        public IMeterPhysicalQuantity<Percent> NegativeDutyFactor { get; }
        public IMeterPhysicalQuantity<NoUnits> FrequencyRatio { get; }
        public IMeterPhysicalQuantity<Voltage> Maximum { get; }
        public IMeterPhysicalQuantity<Voltage> Minimum { get; }
        public IMeterPhysicalQuantity<Voltage> PeakToPeak { get; }
        public IMeterPhysicalQuantity<Voltage> Ratio { get; }
        public IMeterPhysicalQuantity<Time> Period { get; }
        public IMeterPhysicalQuantity<Time> PeriodAver { get; }
        public IMeterPhysicalQuantity<Time> TimeInterval { get; }
        public IMeterPhysicalQuantity<Time> PositivePulseWidth { get; }
        public IMeterPhysicalQuantity<Time> NegativePulseWidth { get; }
        public IMeterPhysicalQuantity<Degreas> Phase { get; }

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