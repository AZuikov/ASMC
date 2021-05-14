using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;
using static ASMC.Devices.HelpDeviceBase;

namespace ASMC.Devices.IEEE.PENDULUM
{
    public class CNT90InputMeasureFunction: ICounterStandartMeasureOperation
    {
        public CNT90InputMeasureFunction(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase)
        {
            MeasFrequency = new MeasFreq(counterAbstr, deviceIeeeBase);
            MeasPeriod = new MeasPeriodTime(counterAbstr, deviceIeeeBase);
            
        }
        /// <summary>
        /// Измерение частоты.
        /// </summary>
        public class MeasFreq : MeasBase<Frequency>
        {
            public MeasFreq(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) : base(counterAbstr,
                                                                                               deviceIeeeBase)
            {
                FunctionName = "FREQ";
            }

        }

        public class MeasFreqBurst : MeasBase<Frequency>
        {
            public MeasFreqBurst(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) :
                base(counterAbstr, deviceIeeeBase)
            {
                FunctionName = "FREQ:BURSt";
            }

            
        }

        public class MeasCyclesInBurst : MeasBase<NoUnits>
        {
            public MeasCyclesInBurst(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) :
                base(counterAbstr, deviceIeeeBase)
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
            public MeasFreqPRF(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) : base(counterAbstr,
                                                                                                  deviceIeeeBase)
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
            public MeasPositiveDUTycycle(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) :
                base(counterAbstr, deviceIeeeBase)
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
            public MeasNegativeDUTycycle(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) :
                base(counterAbstr, deviceIeeeBase)
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
            public MeasMaxVolt(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) : base(counterAbstr,
                                                                                                  deviceIeeeBase)
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
            public MeasMinVolt(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) : base(counterAbstr,
                                                                                                  deviceIeeeBase)
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
            public MeasVpp(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) : base(counterAbstr,
                                                                                              deviceIeeeBase)
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
            public MeasPeriodTime(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) :
                base(counterAbstr, deviceIeeeBase)
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
            public MeasAverPeriodTime(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) :
                base(counterAbstr, deviceIeeeBase)
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
            public MeasPosPulseWidth(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) :
                base(counterAbstr, deviceIeeeBase)
            {
            }

            #region Methods

            public void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($":CONF:MEAS:PWIDth (@{CounterAbstr.NameOfChanel})");
            }

            #endregion Methods
        }

        public class MeasNegPulseWidth : MeasBase<Time>
        {
            public MeasNegPulseWidth(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) :
                base(counterAbstr, deviceIeeeBase)
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

            public MeasBase(CounterInputAbstract counterAbstr, IeeeBase devIeeeBase) : base(counterAbstr.NameOfChanel, devIeeeBase)
            {
                CounterAbstr = counterAbstr;
                
            }

            /// <inheritdoc />
            public override void Setting()
            {
                //todo проверить проинициализированно ли к этому моменту устройство? задана ли строка подключения?
                CounterInput.WriteLine($":CONF:MEAS:{FunctionName} (@{CounterAbstr.NameOfChanel})");
                CounterInput.WriteLine($"inp{CounterAbstr.NameOfChanel}:slop {CounterAbstr.SettingSlope.GetSlope()}");


                //CounterInput.WriteLine($"inp{CounterAbstr.NameOfChanel}:imp {CounterAbstr.InputSetting.GetImpedance()}");
                //CounterInput.WriteLine($"inp{CounterAbstr.NameOfChanel}:att {CounterAbstr.InputSetting.GetAtt()}");
                //CounterInput.WriteLine($"inp{CounterAbstr.NameOfChanel}:coup {CounterAbstr.InputSetting.GetCouple()}");
                //CounterInput.WriteLine($"inp{CounterAbstr.NameOfChanel}:filt {CounterAbstr.InputSetting.GetFilterStatus()}");
            }

            public override void Getting()
            {
                throw new System.NotImplementedException();
            }
        }

        public IMeterPhysicalQuantity<Frequency> MeasFrequency { get; set; }
        public IMeterPhysicalQuantity<Time> MeasPeriod { get; set; }
    }

    public abstract class MeasReadValue<TPhysicalQuantity> : IMeterPhysicalQuantity<TPhysicalQuantity>
        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
    {
        protected IeeeBase CounterInput;
        protected string FunctionName { get; set; }

        public MeasReadValue(int chanelNumb, IeeeBase inDevice)
        {
            CounterInput = inDevice;
        }
        public MeasPoint<TPhysicalQuantity> GetValue()
        {
            //считывание измеренного значения, сработает при условии, что перед этим был запущен метод Setting
            CounterInput.WriteLine(":FORMat ASCii"); //формат получаемых от прибора данных
            CounterInput.WriteLine(":INITiate:CONTinuous 0"); //выключаем многократный запуск
            //CounterInput.WriteLine(":INITiate"); //взводим триггер
            //var answer = CounterInput.QueryLine(":FETCh?"); //считываем ответ
            var answer = CounterInput.QueryLine($":Measure:{FunctionName}?"); //считываем ответ
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