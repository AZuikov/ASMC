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
        private IeeeBase device { get; }

        public int NameOfChanel { get; }
        public MeasPoint<Resistance> InputImpedance { get; private set; }

        private MeasPoint<Voltage> triggerLevel = new MeasPoint<Voltage>(0);
        private PhysicalRange<Voltage> _triggerVoltRange = new PhysicalRange<Voltage>

        {
            //взято из документации по программированию
            Start = new MeasPoint<Voltage>(-5),
            End = new MeasPoint<Voltage>(5)
        };
        /// <summary>
        /// Устанавливает уровень срабатывания триггера и позволяет его считать. Учитывает значение установленного аттенюатора.
        /// </summary>
        public MeasPoint<Voltage> TriggerLeve
        {
            get => triggerLevel * (int)Attenuator;
            set
            {
                if (value < ((MeasPoint<Voltage>)TriggerRange.Start * (int)Attenuator))
                    triggerLevel = (MeasPoint<Voltage>)TriggerRange.Start;
                else if (value > ((MeasPoint<Voltage>)TriggerRange.End * (int)Attenuator))
                    triggerLevel = (MeasPoint<Voltage>)TriggerRange.End;
                else
                    triggerLevel = value;

            }
        }

        private MeasPoint<Time> measTime = new MeasPoint<Time>(1);
        public PhysicalRange<Time> MeasureTimeRange { get; } = new PhysicalRange<Time>
        {
            //взято из документации по программированию
            Start = new MeasPoint<Time>(20, UnitMultiplier.Nano),
            End = new MeasPoint<Time>(1000)
        };

        public CounterAttenuator Attenuator { get; set; }
        public CounterCoupling Coupling { get; set; }
        public CounterOnOffState CounterOnOffState { get; set; }

        
        public ICounterInputSlopeSetting SettingSlope { get; set; }
        public ICounterSingleChanelMeasure Measure { get; set; }
        public IDeviceSettingsControl CurrentMeasFunction { get; protected set; }
        private ICounterAverageMeasure _average { get; }
        #endregion

        public CNT90Input(int chanelName, IeeeBase deviceIeeeBase, ICounterAverageMeasure average)
        {
            device = deviceIeeeBase;
            NameOfChanel = chanelName;
            SettingSlope = new ChanelSlopeSetting();
            Measure = new CNT90InputMeasureFunction(this, NameOfChanel, deviceIeeeBase);
            //настройки по умолчанию, будут залиты в прибор если их не изменить
            Set50OhmInput();
            Coupling = CounterCoupling.DC;
            Attenuator = CounterAttenuator.Att1;
            CounterOnOffState = CounterOnOffState.OFF;
            _average = average;
        }

       public void SetCurrentMeasFunction(IDeviceSettingsControl currentSignal)
        {
            CurrentMeasFunction = currentSignal;
        }

        

        public PhysicalRange<Voltage> TriggerRange
        {
            get
            {
                var range = new PhysicalRange<Voltage>
                {
                    Start = (MeasPoint<Voltage>) _triggerVoltRange.Start * (int) Attenuator,
                    End = (MeasPoint<Voltage>) _triggerVoltRange.End * (int) Attenuator
                };
                return range;
            }
        }

        
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
            //усреднение
            device.WriteLine($":CALCulate:AVERage:STATe {(_average.isAverageOn? CounterOnOffState.ON: CounterOnOffState.OFF)}");
            device.WriteLine($":CALCulate:AVERage:COUNt {_average.averageCount}");
            device.WriteLine($":CALCulate:AVERage:TYPE MEAN");//будем считывать среднее значение

            device.WriteLine($"inp{NameOfChanel}:slop {SettingSlope.Slope}");
            device.WriteLine($"inp{NameOfChanel}:imp {InputImpedance.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            device.WriteLine($"inp{NameOfChanel}:att {(int)Attenuator}");
            device.WriteLine($"inp{NameOfChanel}:coup {Coupling}");
            device.WriteLine($"inp{NameOfChanel}:filt {CounterOnOffState}");
            device.WriteLine($"inp{NameOfChanel}:filt:digital {CounterOnOffState}");
            //MeasureTime
            device.WriteLine($":ACQuisition:APERture {MeasureTime.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            //TriggerLeve
            if (TriggerLeve != null)
            {
                device.WriteLine($":INPut{NameOfChanel}:LEVel {TriggerLeve.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            }
            else
            {
                device.WriteLine($":INPut{NameOfChanel}:LEVel:Auto ONCE");
                /*
               ONCE means that the counter makes one automatic calculation of the trigger level
              at the beginning of a measurement. 
                This value is then fixed until another level-setting command is sent to the counter, 
                or until a new measurement is initiated. 
                Автоматом подбирает уровень триггера, в зависимости от параметров сигнала на входе.
                */

            }

            this.CurrentMeasFunction.Setting();
        }

       
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