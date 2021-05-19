using System;
using System.Windows.Forms;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;

namespace ASMC.Devices.IEEE.PENDULUM
{
    public  class CNT90Input : ICounterInput
    {
        #region Fields

        private MeasPoint<Voltage> triggerLevel;

        private PhysicalRange<Voltage> _triggerRange = new PhysicalRange<Voltage>();
        private MeasPoint<Time> measTime = new MeasPoint<Time>(1);
        private IeeeBase deviceInputCounter { get; }

        #endregion

        public CNT90Input(int chanelName, IeeeBase deviceIeeeBase)
        {
            deviceInputCounter = deviceIeeeBase;
            NameOfChanel = chanelName;
            SettingSlope = new ChanelSlopeSetting();
            Measure = new CNT90InputMeasureFunction(this, NameOfChanel, deviceIeeeBase);
            //настройки по умолчанию, будут залиты в прибор если их не изменить
            Set50OhmInput();
            Coupling = CounterCoupling.DC;
            Attenuator = CounterAttenuator.Att1;
            CounterOnOffState = CounterOnOffState.OFF;

            _triggerRange.Start = new MeasPoint<Voltage>(-5);
            _triggerRange.End = new MeasPoint<Voltage>(-5);
            
            MeasureTimeRange.Start = new MeasPoint<Time>(20, UnitMultiplier.Nano);
            MeasureTimeRange.End = new MeasPoint<Time>(1000);

        }

        public int NameOfChanel { get; }
        public MeasPoint<Resistance> InputImpedance { get; private set; }
        public ICounterInputSlopeSetting SettingSlope { get; set; }
        public ICounterSingleChanelMeasure Measure { get; set; }
       
        public MeasPoint<Voltage> TriggerLeve
        {
            get => triggerLevel * (int)Attenuator;
            set
            {
                if (value <((MeasPoint<Voltage>)TriggerRange.Start* (int)Attenuator))
                    triggerLevel = (MeasPoint<Voltage>)TriggerRange.Start;
                else if (value > ((MeasPoint<Voltage>) TriggerRange.End * (int)Attenuator))
                    triggerLevel = (MeasPoint<Voltage>) TriggerRange.End;
                else
                    triggerLevel = value;
                
            }
        }

        public PhysicalRange<Voltage> TriggerRange
        {
            get
            {
                var range = new PhysicalRange<Voltage>
                {
                    Start = (MeasPoint<Voltage>) _triggerRange.Start * (int) Attenuator,
                    End = (MeasPoint<Voltage>) _triggerRange.End * (int) Attenuator
                };
                return range;
            }
        }

        public CounterAttenuator Attenuator { get; set; }
        public CounterCoupling Coupling { get; set; }
        public MeasPoint<Time> MeasureTime
        {
            get => measTime;
            set
            {
                if (value < (MeasPoint<Time>) MeasureTimeRange.Start)
                    measTime = (MeasPoint<Time>) MeasureTimeRange.Start;
                else if (value > (MeasPoint<Time>) MeasureTimeRange.End)
                    measTime = (MeasPoint<Time>) MeasureTimeRange.End;
                else
                {
                    measTime = value;
                }
            }
        }

        public PhysicalRange<Time> MeasureTimeRange { get; } = new PhysicalRange<Time>();

        public void Set50OhmInput()
        {
            InputImpedance = new MeasPoint<Resistance>(50);
        }

        public void Set1MOhmInput()
        {
            InputImpedance = new MeasPoint<Resistance>(1, UnitMultiplier.Mega);
        }


        public void Getting()
        {
            throw new NotImplementedException();
        }

        public void Setting()
        {
            //TriggerLeve
            deviceInputCounter.WriteLine($":INPut{NameOfChanel}:LEVel {TriggerLeve.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            //Coupling 
            deviceInputCounter.WriteLine($":INPut{NameOfChanel}:COUPling {Coupling.ToString()}");
            //Attenuator
            deviceInputCounter.WriteLine($":INPut{NameOfChanel}:ATTenuation {(int)Attenuator}");
            //FilterState
            deviceInputCounter.WriteLine($":INPut{NameOfChanel}:FILTer {CounterOnOffState}");//аналоговый фильтр
            deviceInputCounter.WriteLine($":INPut{NameOfChanel}:FILTer:DIGital {CounterOnOffState}");//цифровой фильтр
            //InputImpedance
            deviceInputCounter.WriteLine($":INPut{NameOfChanel}:IMPedance {InputImpedance.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',','.')}");
            //MeasureTime
            deviceInputCounter.WriteLine($":ACQuisition:APERture {MeasureTime.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',','.')}");
            //SettingSlope
            deviceInputCounter.WriteLine($":INPut{NameOfChanel}:SLOPe {SettingSlope.ToString()}");
        }

        public CounterOnOffState CounterOnOffState { get; set; }
    }

    public abstract class CounterDualChanelMeasureAbstract : ICounterInputDualChanelMeasure
    {
        public MeasPoint<NoUnits> MeasFrequencyRatio(ICounterInput input, ICounterInput input2)
        {
            throw new NotImplementedException();
        }

        public MeasPoint<Degreas> MeasPhase(ICounterInput input, ICounterInput input2)
        {
            throw new NotImplementedException();
        }

        public MeasPoint<Time> MeasTimeInterval(ICounterInput input, ICounterInput input2)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Позволяет выбрать передний или задний фронт сигнала для синхронизации.
    /// </summary>
    public class ChanelSlopeSetting : ICounterInputSlopeSetting
    {
        #region Fields

        public InputSlope Slope { get; private set; }

        #endregion

        public ChanelSlopeSetting()
        {
            Slope = InputSlope.POS;
        }

        public void SetInputSlopePositive()
        {
            Slope = InputSlope.POS;
        }

        public void SetInputSlopeNegative()
        {
            Slope = InputSlope.NEG;
        }

      
    }

   

}