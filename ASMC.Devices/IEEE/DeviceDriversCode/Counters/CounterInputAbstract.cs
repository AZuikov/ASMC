using System;
using System.Threading.Tasks;
using AP.Utils.Data;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;

namespace ASMC.Devices.IEEE.PENDULUM
{
    public abstract class CounterInputAbstract : ICounterInput
    {
        public static IeeeBase CounterInput = new IeeeBase();

        public CounterInputAbstract(string chanelName, CounterAbstract counter)
        {
            NameOfChanel = chanelName;
            InputSetting = ChanelSetting.getInstance();
            UserType = counter.UserType + " chanel";
        }

        public string NameOfChanel { get; }
        public ITypicalCounterInputSettings InputSetting { get; set; }
        public IMeterPhysicalQuantity<Frequency> MeasFrequency { get; set; }
        public IMeterPhysicalQuantity<Frequency> MeasFrequencyBURSt { get; set; }
        public IMeterPhysicalQuantity<NoUnits> MeasNumberOfCyclesInBurst { get; set; }
       public IMeterPhysicalQuantity<Frequency> MeasPulseRepetitionFrequencyBurstSignal { get; set; }
        public IMeterPhysicalQuantity<Percent> MeasPositiveDutyCycle { get; set; }
        public IMeterPhysicalQuantity<Percent> MeasNegativeDutyCycle { get; set; }
       public IMeterPhysicalQuantity<Voltage> MeasMaximum { get; set; }
        public IMeterPhysicalQuantity<Voltage> MeasMinimum { get; set; }
        public IMeterPhysicalQuantity<Voltage> MeasPeakToPeak { get; set; }
       public IMeterPhysicalQuantity<Time> MeasPeriod { get; set; }
        public IMeterPhysicalQuantity<Time> MeasPeriodAver { get; set; }
       public IMeterPhysicalQuantity<Time> MeasPositivePulseWidth { get; set; }
        public IMeterPhysicalQuantity<Time> MeasNegativePulseWidth { get; set; }
       public string UserType { get; }
        
       public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool IsTestConnect { get; }

        public async Task InitializeAsync()
        {
            throw new NotImplementedException();
        }

        public string StringConnection
        {
            get => CounterInput.StringConnection;
            set => CounterInput.StringConnection = value;
        }

        /// <summary>
        /// Настройка канала.
        /// </summary>
        public class ChanelSetting : ITypicalCounterInputSettings
        {
            private static ChanelSetting instance;

            #region Fields

            /*Настройки у канала должны быть одни, поэтому будет синглтон!!!*/

            private InputAttenuator Attenuator;
            private InputCouple Couple;
            private InputImpedance Impedance;
            private Status InputFilter;
            private InputSlope Slope;

            #endregion

            private ChanelSetting()
            {
                //настройки канала по умолчаниюа
                Attenuator = InputAttenuator.ATT1;
                Impedance = InputImpedance.IMP50Ohm;
                Couple = InputCouple.DC;
                Slope = InputSlope.POS;
                InputFilter = Status.OFF;
            }

            #region Methods

            public static ChanelSetting getInstance()
            {
                if (instance == null) instance = new ChanelSetting();

                return instance;
            }

            #endregion

            public virtual void SetAtt_1()
            {
                Attenuator = InputAttenuator.ATT1;
            }

            public virtual void SetAtt_10()
            {
                Attenuator = InputAttenuator.ATT10;
            }

            public string GetAtt()
            {
                return Attenuator.ToString();
            }

            public virtual void SetHightImpedance()
            {
                Impedance = InputImpedance.IMPMegaOhm;
            }

            public virtual void SetLowImpedance()
            {
                Impedance = InputImpedance.IMP50Ohm;
            }

            public string GetImpedance()
            {
                return Impedance.ToString().Replace(',', '.');
            }

            public virtual void SetCoupleAC()
            {
                Couple = InputCouple.AC;
            }

            public virtual void SetCoupleDC()
            {
                Couple = InputCouple.DC;
            }

            public string GetCouple()
            {
                return Couple.ToString();
            }

            public void SetFilterOn()
            {
                InputFilter = Status.ON;
            }

            public void SetFilterOff()
            {
                InputFilter = Status.OFF;
            }

            public string GetFilterStatus()
            {
                return InputFilter.ToString();
            }

            public virtual void SetInputSlopePositive()
            {
                Slope = InputSlope.POS;
            }

            public virtual void SetInputSlopeNegative()
            {
                Slope = InputSlope.NEG;
            }

            public string GetSlope()
            {
                return Slope.ToString();
            }
        }

        #region Enums

        public enum InputCouple
        {
            AC,
            DC
        }

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
        {
            ON,
            OFF
        }

        #endregion Enums
    }

    public abstract class CounterInputAbstractHF : CounterInputAbstract, ICOunterInputHighFrequency
    {
        protected CounterInputAbstractHF(string chanelName, CounterAbstract counter) : base(chanelName, counter)
        {
        }

        public IMeterPhysicalQuantity<Power> MeasPowerAC_Signal { get; set; }
    }

    public abstract class CounterDualChanelMeasureAbstract: ICounterInputDualChanelMeasure
    {
        protected Pendulum_CNT_90 _counter;
        public static IeeeBase _device = new IeeeBase();
        public IMeterPhysicalQuantity<NoUnits> MeasFrequencyRatioAB { get; set; }
        public IMeterPhysicalQuantity<NoUnits> MeasFrequencyRatioBA { get; set; }
        public IMeterPhysicalQuantity<Voltage> MeasRatioAB { get; set; }
        public IMeterPhysicalQuantity<Voltage> MeasRatioBA { get; set; }
        public IMeterPhysicalQuantity<Degreas> MeasPhaseAB { get; set; }
        public IMeterPhysicalQuantity<Degreas> MeasPhaseBA { get; set; }
        public IMeterPhysicalQuantity<Time> MeasTimeIntervalAB { get; set; }
        public IMeterPhysicalQuantity<Time> MeasTimeIntervalBA { get; set; }
        public string UserType { get; }

        public CounterDualChanelMeasureAbstract(Pendulum_CNT_90 counter)
        {
            _counter = counter;
        }
        
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool IsTestConnect { get; }
        public async Task InitializeAsync()
        {
            throw new NotImplementedException();
        }

        public string StringConnection
        {
            get => _device.StringConnection;
            set
            {
                _device.StringConnection = value;
            } }
    }
}