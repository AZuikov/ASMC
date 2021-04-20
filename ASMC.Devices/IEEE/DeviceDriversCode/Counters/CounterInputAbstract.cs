﻿using System.Threading.Tasks;
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

        public IeeeBase Input;
        public CounterInputAbstract(string chanelName, CounterAbstract counter)
        {
            Input = new IeeeBase();
            NameOfChanel = chanelName;
            InputSetting = ChanelSetting.getInstance();
            UserType = counter.UserType+" chanel";
        }


        public string NameOfChanel { get; }
        public ITypicalCounterInputSettings InputSetting { get; set; }
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

            private InputAttenuator Attenuator;
            private InputImpedance Impedance;
            private InputCouple Couple;
            private InputSlope Slope;

            
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

        public string UserType { get; }
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public bool IsTestConnect { get; }
        public async Task InitializeAsync()
        {
            throw new System.NotImplementedException();
        }

        public string StringConnection
        {
            get => Input.StringConnection;
            set
            {
                Input.StringConnection = value;
            }
        }
    }
}