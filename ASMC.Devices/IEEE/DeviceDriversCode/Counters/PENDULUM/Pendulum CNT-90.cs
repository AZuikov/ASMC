using System.Threading.Tasks;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.SourceAndMeter;
using static ASMC.Devices.HelpDeviceBase;

namespace ASMC.Devices.IEEE.PENDULUM
{
    /// <summary>
    /// Класс частотомера.
    /// </summary>
    public class Pendulum_CNT_90 : CounterAbstract
    {
        public Pendulum_CNT_90()
        {
            UserType = "CNT-90";

            InputA = new CNT90Input("1", this);
            InputB = new CNT90Input("2", this);
            InputC_HighFrequency = new CNT90InputC_HighFreq("3", this);
            DualChanelMeasure = new CNT90DualChanelMeasure(InputA, InputB);
        }

        
        #region Enums

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

        #endregion Enums
    }

    public class CNT90Input : CounterInputAbstract
    {
        public CNT90Input(string chanelName, CounterAbstract counter) : base(chanelName, counter)
        {
            MeasFrequency = new MeasFreq(this);
            MeasFrequencyBURSt = new MeasFreqBurst(this);
            MeasNumberOfCyclesInBurst = new MeasCyclesInBurst(this);
            MeasPulseRepetitionFrequencyBurstSignal = new MeasFreqPRF(this);
            MeasPositiveDutyCycle = new MeasPositiveDUTycycle(this);
            MeasNegativeDutyCycle = new MeasNegativeDUTycycle(this);
            MeasMaximum = new MeasMaxVolt(this);
            MeasMinimum = new MeasMinVolt(this);
            MeasPeakToPeak = new MeasVpp(this);
            MeasPeriod = new MeasPeriodTime(this);
            MeasPeriodAver = new MeasAverPeriodTime(this);
            MeasPositivePulseWidth = new MeasPosPulseWidth(this);
            MeasNegativePulseWidth = new MeasNegPulseWidth(this);
        }

        /// <summary>
        /// Измерение частоты.
        /// </summary>
        public class MeasFreq : MeasBase<Frequency>
        {
            public MeasFreq(CounterInputAbstract device) : base(device)
            {
            }

            #region Methods

            public new void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:FREQ (@{_device.NameOfChanel})");
            }

            #endregion
        }

        public class MeasFreqBurst : MeasBase<Frequency>
        {
            public MeasFreqBurst(CounterInputAbstract device) : base(device)
            {
            }

            public new void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:FREQ:BURSt (@{_device.NameOfChanel})");
                
            }
        }

        public class MeasCyclesInBurst : MeasBase<NoUnits>
        {
            public MeasCyclesInBurst(CounterInputAbstract device) : base(device)
            {
            }

            public new void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEASure:VOLT:NCYCles (@{_device.NameOfChanel})");

            }
        }

        public class MeasFreqPRF : MeasBase<Frequency>
        {
            public MeasFreqPRF(CounterInputAbstract device) : base(device)
            {
            }

            #region Methods

            public new void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:FREQ:PRF (@{_device.NameOfChanel})");
            }

            #endregion
        }

        public class MeasPositiveDUTycycle : MeasBase<Percent>
        {
            public MeasPositiveDUTycycle(CounterInputAbstract device) : base(device)
            {
            }

            #region Methods

            public new void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:PDUTycycle (@{_device.NameOfChanel})");
            }

            #endregion
        }

        public class MeasNegativeDUTycycle : MeasBase<Percent>
        {
            public MeasNegativeDUTycycle(CounterInputAbstract device) : base(device)
            {
            }

            #region Methods

            public new void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:NDUTycycle (@{_device.NameOfChanel})");
            }

            #endregion
        }

        public class MeasMaxVolt : MeasBase<Voltage>
        {
            public MeasMaxVolt(CounterInputAbstract device) : base(device)
            {
            }

            #region Methods

            public new void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:VOLT:MAXimum (@{_device.NameOfChanel})");
            }

            #endregion
        }

        public class MeasMinVolt : MeasBase<Voltage>
        {
            public MeasMinVolt(CounterInputAbstract device) : base(device)
            {
            }

            #region Methods

            public new void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:VOLT:MINimum (@{_device.NameOfChanel})");
            }

            #endregion
        }

        public class MeasVpp : MeasBase<Voltage>
        {
            public MeasVpp(CounterInputAbstract device) : base(device)
            {
            }

            #region Methods

            public new void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:VOLT:PTPeak (@{_device.NameOfChanel})");
            }

            #endregion
        }

        public class MeasPeriodTime : MeasBase<Time>
        {
            public MeasPeriodTime(CounterInputAbstract device) : base(device)
            {
            }

            #region Methods

            public new void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:PERiod (@{_device.NameOfChanel})");
            }

            #endregion
        }

        public class MeasAverPeriodTime : MeasBase<Time>
        {
            public MeasAverPeriodTime(CounterInputAbstract device) : base(device)
            {
            }

            #region Methods

            public new void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:PERiod:AVERage (@{_device.NameOfChanel})");
            }

            #endregion
        }

        public class MeasPosPulseWidth : MeasBase<Time>
        {
            public MeasPosPulseWidth(CounterInputAbstract device) : base(device)
            {
            }

            #region Methods

            public new void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:PWIDth (@{_device.NameOfChanel})");
            }

            #endregion
        }
        public class MeasNegPulseWidth : MeasBase<Time>
        {
            public MeasNegPulseWidth(CounterInputAbstract device) : base(device)
            {
            }

            #region Methods

            public new void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:NWIDth (@{_device.NameOfChanel})");
            }

            #endregion
        }

        public abstract class MeasBase<TPhysicalQuantity> : IMeterPhysicalQuantity<TPhysicalQuantity>
            where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
        {
            #region Fields

            protected CounterInputAbstract _device;

            #endregion

            public MeasBase(CounterInputAbstract device)
            {
                _device = device;
            }

            public void Setting()
            {
                //todo проверить проинициализированно ли к этому моменту устройство? задана ли строка подключения?
                CounterInput.WriteLine($"inp{_device.NameOfChanel}:imp {_device.InputSetting.GetImpedance()}");
                CounterInput.WriteLine($"inp{_device.NameOfChanel}:att {_device.InputSetting.GetAtt()}");
                CounterInput.WriteLine($"inp{_device.NameOfChanel}:coup {_device.InputSetting.GetCouple()}");
                CounterInput.WriteLine($"inp{_device.NameOfChanel}:slop {_device.InputSetting.GetSlope()}");
                CounterInput.WriteLine($"inp{_device.NameOfChanel}:filt {_device.InputSetting.GetFilterStatus()}");
            }

            public void Getting()
            {
            }

            public MeasPoint<TPhysicalQuantity> GetValue()
            {
                //считывание измеренного значения, сработает при условии, что перед эим был запущен метод Setting
                CounterInput.WriteLine(":FORMat ASCii"); //формат получаемых от прибора данных
                CounterInput.WriteLine(":INITiate:CONTinuous 0"); //выключаем многократный запуск
                CounterInput.WriteLine(":INITiate"); //взводим триггер
                var answer = CounterInput.QueryLine(":FETCh?"); //считываем ответ
                var value = (decimal) StrToDouble(answer);
                Value = new MeasPoint<TPhysicalQuantity>(value);
                return Value;
            }

            public MeasPoint<TPhysicalQuantity> Value { get; protected set; }
            public IRangePhysicalQuantity<TPhysicalQuantity> RangeStorage { get; }
        }
    }

    public class CNT90InputC_HighFreq : CounterInputAbstractHF
    {
        public CNT90InputC_HighFreq(string chanelName, CounterAbstract counter) : base(chanelName, counter)
        {
        }
    }

    public class CNT90DualChanelMeasure : CounterDualChanelMeasureAbstract
    {
        
        public CNT90DualChanelMeasure(Pendulum_CNT_90 counter):base(counter)
        {
            MeasFrequencyRatioAB = null;
            MeasRatioAB = null;
            MeasPhaseAB = null;
            MeasTimeIntervalAB = null;
            
        }

        public abstract class DualChanelBaseMeasure<TPhysicalQuantity> : IMeterPhysicalQuantity<TPhysicalQuantity>
            where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
        {
            protected CounterInputAbstract _chanelA;
            protected CounterInputAbstract _chanelB;

            public DualChanelBaseMeasure(CounterInputAbstract chnA, CounterInputAbstract chnB)
            {
                _chanelA = chnA;
                _chanelB = chnB;
            }
            public void Getting()
            {
                throw new System.NotImplementedException();
            }

            public void Setting()
            {
                //todo проверить проинициализированно ли к этому моменту устройство? задана ли строка подключения?
                _device.WriteLine($"inp{_chanelA.NameOfChanel}:imp { _chanelA.InputSetting.GetImpedance()}");
                _device.WriteLine($"inp{_chanelA.NameOfChanel}:att { _chanelA.InputSetting.GetAtt()}");
                _device.WriteLine($"inp{_chanelA.NameOfChanel}:coup {_chanelA.InputSetting.GetCouple()}");
                _device.WriteLine($"inp{_chanelA.NameOfChanel}:slop {_chanelA.InputSetting.GetSlope()}");
                _device.WriteLine($"inp{_chanelA.NameOfChanel}:filt {_chanelA.InputSetting.GetFilterStatus()}");

                _device.WriteLine($"inp{_chanelB.NameOfChanel}:imp { _chanelB.InputSetting.GetImpedance()}");
                _device.WriteLine($"inp{_chanelB.NameOfChanel}:att { _chanelB.InputSetting.GetAtt()}");
                _device.WriteLine($"inp{_chanelB.NameOfChanel}:coup {_chanelB.InputSetting.GetCouple()}");
                _device.WriteLine($"inp{_chanelB.NameOfChanel}:slop {_chanelB.InputSetting.GetSlope()}");
                _device.WriteLine($"inp{_chanelB.NameOfChanel}:filt {_chanelB.InputSetting.GetFilterStatus()}");
            }

            public MeasPoint<TPhysicalQuantity> GetValue()
            {
                throw new System.NotImplementedException();
            }

            public MeasPoint<TPhysicalQuantity> Value { get; }
            public IRangePhysicalQuantity<TPhysicalQuantity> RangeStorage { get; }
        }
    }
}