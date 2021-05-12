using System.Threading.Tasks;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;
using static ASMC.Devices.HelpDeviceBase;

namespace ASMC.Devices.IEEE.PENDULUM
{
    /// <summary>
    /// Класс частотомера.
    /// </summary>
    public class Pendulum_CNT_90 : CounterAbstract
    {
        private IeeeBase deviceIeeBase = new IeeeBase();
        
        public Pendulum_CNT_90()
        {
            UserType = "CNT-90";

            InputA = new CNT90Input(1, this, deviceIeeBase);
            InputB = new CNT90Input(2, this, deviceIeeBase);

            DualChanelMeasure = new CNT90DualChanelMeasure(this, deviceIeeBase);
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

        public override async Task InitializeAsync()
        {
            var options = counter.GetOption();
            //todo инициализация опций
            
            InputC_HighFrequency = new CNT90InputC_HighFreq(3, this);
        }
    }

    public class CNT90Input : CounterInputAbstract
    {
        public CNT90Input(int chanelName, CounterAbstract counter, IeeeBase deviceIeeeBase) : base(chanelName, counter)
        {
            MeasFrequency = new MeasFreq(this, deviceIeeeBase);
            MeasFrequencyBURSt = new MeasFreqBurst(this, deviceIeeeBase);
            MeasNumberOfCyclesInBurst = new MeasCyclesInBurst(this, deviceIeeeBase);
            MeasPulseRepetitionFrequencyBurstSignal = new MeasFreqPRF(this, deviceIeeeBase);
            MeasPositiveDutyCycle = new MeasPositiveDUTycycle(this, deviceIeeeBase);
            MeasNegativeDutyCycle = new MeasNegativeDUTycycle(this, deviceIeeeBase);
            MeasMaximum = new MeasMaxVolt(this, deviceIeeeBase);
            MeasMinimum = new MeasMinVolt(this, deviceIeeeBase);
            MeasPeakToPeak = new MeasVpp(this, deviceIeeeBase);
            MeasPeriod = new MeasPeriodTime(this, deviceIeeeBase);
            MeasPeriodAver = new MeasAverPeriodTime(this, deviceIeeeBase);
            MeasPositivePulseWidth = new MeasPosPulseWidth(this, deviceIeeeBase);
            MeasNegativePulseWidth = new MeasNegPulseWidth(this, deviceIeeeBase);
        }

        /// <summary>
        /// Измерение частоты.
        /// </summary>
        public class MeasFreq : MeasBase<Frequency>
        {
            public MeasFreq(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) : base(counterAbstr, deviceIeeeBase)
            {
            }

            #region Methods

            public override void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:FREQ (@{CounterAbstr.NameOfChanel})");
            }

            #endregion Methods
        }

        public class MeasFreqBurst : MeasBase<Frequency>
        {
            public MeasFreqBurst(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) : base(counterAbstr, deviceIeeeBase)
            {
            }

            public override void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:FREQ:BURSt (@{CounterAbstr.NameOfChanel})");
            }
        }

        public class MeasCyclesInBurst : MeasBase<NoUnits>
        {
            public MeasCyclesInBurst(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) : base(counterAbstr, deviceIeeeBase)
            {
            }

            public override void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEASure:VOLT:NCYCles (@{CounterAbstr.NameOfChanel})");
            }
        }

        public class MeasFreqPRF : MeasBase<Frequency>
        {
            public MeasFreqPRF(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) : base(counterAbstr, deviceIeeeBase)
            {
            }

            #region Methods

            public override void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:FREQ:PRF (@{CounterAbstr.NameOfChanel})");
            }

            #endregion Methods
        }

        public class MeasPositiveDUTycycle : MeasBase<Percent>
        {
            public MeasPositiveDUTycycle(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) : base(counterAbstr, deviceIeeeBase)
            {
            }

            #region Methods

            public override void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:PDUTycycle (@{CounterAbstr.NameOfChanel})");
            }

            #endregion Methods
        }

        public class MeasNegativeDUTycycle : MeasBase<Percent>
        {
            public MeasNegativeDUTycycle(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) : base(counterAbstr, deviceIeeeBase)
            {
            }

            #region Methods

            public override void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:NDUTycycle (@{CounterAbstr.NameOfChanel})");
            }

            #endregion Methods
        }

        public class MeasMaxVolt : MeasBase<Voltage>
        {
            public MeasMaxVolt(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) : base(counterAbstr, deviceIeeeBase)
            {
            }

            #region Methods

            public override void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:VOLT:MAXimum (@{CounterAbstr.NameOfChanel})");
            }

            #endregion Methods
        }

        public class MeasMinVolt : MeasBase<Voltage>
        {
            public MeasMinVolt(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) : base(counterAbstr, deviceIeeeBase)
            {
            }

            #region Methods

            public override void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:VOLT:MINimum (@{CounterAbstr.NameOfChanel})");
            }

            #endregion Methods
        }

        public class MeasVpp : MeasBase<Voltage>
        {
            public MeasVpp(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) : base(counterAbstr, deviceIeeeBase)
            {
            }

            #region Methods

            public override void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:VOLT:PTPeak (@{CounterAbstr.NameOfChanel})");
            }

            #endregion Methods
        }

        public class MeasPeriodTime : MeasBase<Time>
        {
            public MeasPeriodTime(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) : base(counterAbstr, deviceIeeeBase)
            {
            }

            #region Methods

            public override void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:PERiod (@{CounterAbstr.NameOfChanel})");
            }

            #endregion Methods
        }

        public class MeasAverPeriodTime : MeasBase<Time>
        {
            public MeasAverPeriodTime(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) : base(counterAbstr, deviceIeeeBase)
            {
            }

            #region Methods

            public override void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:PERiod:AVERage (@{CounterAbstr.NameOfChanel})");
            }

            #endregion Methods
        }

        public class MeasPosPulseWidth : MeasBase<Time>
        {
            public MeasPosPulseWidth(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) : base(counterAbstr, deviceIeeeBase)
            {
            }

            #region Methods

            public  void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:PWIDth (@{CounterAbstr.NameOfChanel})");
            }

            #endregion Methods
        }

        public class MeasNegPulseWidth : MeasBase<Time>
        {
            public MeasNegPulseWidth(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) : base(counterAbstr, deviceIeeeBase)
            {
            }

            #region Methods

            public override void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:NWIDth (@{CounterAbstr.NameOfChanel})");
            }

            #endregion Methods
        }

        public abstract class MeasBase<TPhysicalQuantity> : MeasReadValue<TPhysicalQuantity>
            where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
        {
            #region Fields

            protected CounterInputAbstract CounterAbstr;

            #endregion Fields

            public MeasBase(CounterInputAbstract counterAbstr, IeeeBase devIeeeBase) : base(devIeeeBase)
            {
                CounterAbstr = counterAbstr;
            }

            /// <inheritdoc />
            public override void Setting()
            {
                //todo проверить проинициализированно ли к этому моменту устройство? задана ли строка подключения?
                CounterInput.WriteLine($"inp{CounterAbstr.NameOfChanel}:imp {CounterAbstr.InputSetting.GetImpedance()}");
                CounterInput.WriteLine($"inp{CounterAbstr.NameOfChanel}:att {CounterAbstr.InputSetting.GetAtt()}");
                CounterInput.WriteLine($"inp{CounterAbstr.NameOfChanel}:coup {CounterAbstr.InputSetting.GetCouple()}");
                CounterInput.WriteLine($"inp{CounterAbstr.NameOfChanel}:slop {CounterAbstr.InputSetting.GetSlope()}");
                CounterInput.WriteLine($"inp{CounterAbstr.NameOfChanel}:filt {CounterAbstr.InputSetting.GetFilterStatus()}");
            }

            public override void Getting()
            {
                throw new System.NotImplementedException();
            }
        }
    }

    public class CNT90InputC_HighFreq : CounterInputAbstractHF
    {
        public CNT90InputC_HighFreq(int chanelName, CounterAbstract counter) : base(chanelName, counter)
        {
        }
    }

    public class CNT90DualChanelMeasure : CounterDualChanelMeasureAbstract
    {
        public CNT90DualChanelMeasure(Pendulum_CNT_90 counter, IeeeBase deviceIeeeBase) : base(counter)
        {
            MeasFrequencyRatioAB = new FreqRatio(_counter.InputA, _counter.InputB,  deviceIeeeBase);
            MeasFrequencyRatioBA = new FreqRatio(_counter.InputB, _counter.InputA,  deviceIeeeBase);
            MeasRatioAB = null;
            MeasRatioBA = null;
            MeasPhaseAB = null;
            MeasPhaseBA = null;
            MeasTimeIntervalAB = null;
            MeasTimeIntervalBA = null;
        }

        public class FreqRatio : DualChanelBaseMeasure<NoUnits>
        {
            public FreqRatio(ICounterInput chnA, ICounterInput chnB, IeeeBase deviceIeeeBase) : base(chnA, chnB, deviceIeeeBase)
            {
            }
        }

        public abstract class DualChanelBaseMeasure<TPhysicalQuantity> : MeasReadValue<TPhysicalQuantity>
            where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
        {
            protected ICounterInput _chanelA;
            protected ICounterInput _chanelB;

            public DualChanelBaseMeasure(ICounterInput chnA, ICounterInput chnB, IeeeBase deviceIeeeBase) : base(deviceIeeeBase)
            {
                _chanelA = chnA;
                _chanelB = chnB;
                
            }

            public override void Getting()
            {
                throw new System.NotImplementedException();
            }

            public override void Setting()
            {
                //todo проверить проинициализированно ли к этому моменту устройство? задана ли строка подключения?
                CounterInput.WriteLine($"inp{_chanelA.NameOfChanel}:imp { _chanelA.InputSetting.GetImpedance()}");
                CounterInput.WriteLine($"inp{_chanelA.NameOfChanel}:att { _chanelA.InputSetting.GetAtt()}");
                CounterInput.WriteLine($"inp{_chanelA.NameOfChanel}:coup {_chanelA.InputSetting.GetCouple()}");
                CounterInput.WriteLine($"inp{_chanelA.NameOfChanel}:slop {_chanelA.InputSetting.GetSlope()}");
                CounterInput.WriteLine($"inp{_chanelA.NameOfChanel}:filt {_chanelA.InputSetting.GetFilterStatus()}");

                CounterInput.WriteLine($"inp{_chanelB.NameOfChanel}:imp { _chanelB.InputSetting.GetImpedance()}");
                CounterInput.WriteLine($"inp{_chanelB.NameOfChanel}:att { _chanelB.InputSetting.GetAtt()}");
                CounterInput.WriteLine($"inp{_chanelB.NameOfChanel}:coup {_chanelB.InputSetting.GetCouple()}");
                CounterInput.WriteLine($"inp{_chanelB.NameOfChanel}:slop {_chanelB.InputSetting.GetSlope()}");
                CounterInput.WriteLine($"inp{_chanelB.NameOfChanel}:filt {_chanelB.InputSetting.GetFilterStatus()}");
            }

            public MeasPoint<TPhysicalQuantity> GetValue()
            {
                throw new System.NotImplementedException();
            }

            public MeasPoint<TPhysicalQuantity> Value { get; }
            public IRangePhysicalQuantity<TPhysicalQuantity> RangeStorage { get; }
        }
    }


    public abstract class MeasReadValue<TPhysicalQuantity> : IMeterPhysicalQuantity<TPhysicalQuantity>
        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
    {
        protected IeeeBase CounterInput;

        public MeasReadValue(IeeeBase inDevice)
        {
            CounterInput = inDevice;
        }
        public MeasPoint<TPhysicalQuantity> GetValue()
        {
            //считывание измеренного значения, сработает при условии, что перед эим был запущен метод Setting
            CounterInput.WriteLine(":FORMat ASCii"); //формат получаемых от прибора данных
            CounterInput.WriteLine(":INITiate:CONTinuous 0"); //выключаем многократный запуск
            CounterInput.WriteLine(":INITiate"); //взводим триггер
            var answer = CounterInput.QueryLine(":FETCh?"); //считываем ответ
            var value = (decimal)StrToDouble(answer);
            Value = new MeasPoint<TPhysicalQuantity>(value);
            return Value;
        }

        public MeasPoint<TPhysicalQuantity> Value { get; protected set; }

        public abstract void Getting();
        

        public abstract void Setting();
        

        public IRangePhysicalQuantity<TPhysicalQuantity> RangeStorage { get; }
    }
}