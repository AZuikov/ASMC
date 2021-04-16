using System;
using System.Threading.Tasks;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;

namespace ASMC.Devices.IEEE.PENDULUM
{
    /// <summary>
    /// Класс частотомера.
    /// </summary>
    public class Pendulum_CNT_90 : ICounter
    {
        public Pendulum_CNT_90()
        {
            UserType = "CNT-90";
            //todo нужно как то проверять наличие опций и создавать нужную конфигурацию
        }

        public void SetExternalReferenceClock()
        {
            //:ROSCillator:SOURce EXT
            throw new NotImplementedException();
        }

        public void SetInternalReferenceClock()
        {
            //:ROSCillator:SOURce INT
            throw new NotImplementedException();
        }

        public string UserType { get; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool IsTestConnect { get; }

        public async Task InitializeAsync()
        {
        }

        public string StringConnection { get; set; }

        private enum InstallTimebaseOption
        {
            [StringValue("Standard")] Standard,
            [StringValue("Option 19")] Option19,
            [StringValue("Option 30")] Option30,
            [StringValue("Option 40")] Option40,
            [StringValue("Rubidium")] Rubidium
        }

        private enum InstallPrescalerOption
        {
            [StringValue("0")] NullOption,
            [StringValue("Option 10")] Option10,
            [StringValue("Option 13")] Option13,
            [StringValue("Option 14")] Option14,
            [StringValue("Option 14B")] Option14B
        }

        private enum InstallMicrowaveConverter
        {
            [StringValue("27GHz")] Microwave27GHz,
            [StringValue("40GHz")] Microwave40GHz,
            [StringValue("46GHz")] Microwave46GHz,
            [StringValue("60GHz")] Microwave60GHz
        }
    }

    public class Counter : IProtocolStringLine
    {
        public IeeeBase Device { get; }
        public string UserType { get; }

        public Counter()
        {
            Device = new IeeeBase();
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

        public string StringConnection { get; set; }
    }

    public  class CounterInput: ICounterInput
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

        private Counter _counter;
       public CounterInput(string chanelName, Counter counter)
        {
            NameOfChanel = chanelName;
            InputSetting = new ChanelSetting();
            _counter = counter;
        }


        public string NameOfChanel { get; }
        public ITypicalCounterInputSettings InputSetting { get; }

        public class ChanelSetting : ITypicalCounterInputSettings
        {
            public InputAttenuator Attenuator { get; protected set; }
            public InputImpedance Impedance { get; protected set; }
            public InputCouple Couple { get; protected set; }
            public InputSlope Slope { get; protected set; }

            public ChanelSetting()
            {
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