using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;

namespace ASMC.Devices.IEEE.PENDULUM
{
    public abstract class CounterInputAbstract : ICounterInput
    {
        public int NameOfChanel { get; }
        public ICounterInputSlopeSetting SettingSlope { get; set; }
        public ICounterStandartMeasureOperation MeasureStandart { get; set; }
    }

    public abstract class CounterInputAbstractHF 
    {
       
    }

    public abstract class CounterDualChanelMeasureAbstract : ICounterInputDualChanelMeasure
    {
        public MeasPoint<NoUnits> MeasFrequencyRatio(ICounterInput input, ICounterInput input2)
        {
            throw new System.NotImplementedException();
        }

        public MeasPoint<Degreas> MeasPhase(ICounterInput input, ICounterInput input2)
        {
            throw new System.NotImplementedException();
        }

        public MeasPoint<Time> MeasTimeInterval(ICounterInput input, ICounterInput input2)
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// Настройка канала.
    /// </summary>
    public class ChanelStandartSetting :  ITypicalCounterInputSettings
    {
        #region Fields

        private InputAttenuator Attenuator;
        private InputCouple Couple;
        private InputImpedance Impedance;
        private Status InputFilter;
        private InputSlope Slope;

        #endregion

        public ChanelStandartSetting()
        {
            //настройки канала по умолчанию
            Attenuator = InputAttenuator.ATT1;
            Impedance = InputImpedance.IMP50Ohm;
            Couple = InputCouple.DC;
            Slope = InputSlope.POS;
            InputFilter = Status.OFF;
        }

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


        public void SetInputSlopePositive()
        {
            Slope = InputSlope.POS;
        }

        public void SetInputSlopeNegative()
        {
            Slope = InputSlope.NEG;
        }

        public string GetSlope()
        {
            return Slope.GetStringValue();
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

    public enum Status
    {
        ON,
        OFF
    }

    #endregion Enums
}