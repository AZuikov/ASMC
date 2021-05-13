using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.IEEE.PENDULUM
{
    public class CNT90InputMeasureFunction
    {
        public CNT90InputMeasureFunction()
        {
            //MeasFrequency = new MeasFreq(this, deviceIeeeBase);
            //MeasFrequencyBURSt = new MeasFreqBurst(this, deviceIeeeBase);
            //MeasNumberOfCyclesInBurst = new MeasCyclesInBurst(this, deviceIeeeBase);
            //MeasPulseRepetitionFrequencyBurstSignal = new MeasFreqPRF(this, deviceIeeeBase);
            //MeasPositiveDutyCycle = new MeasPositiveDUTycycle(this, deviceIeeeBase);
            //MeasNegativeDutyCycle = new MeasNegativeDUTycycle(this, deviceIeeeBase);
            //MeasMaximum = new MeasMaxVolt(this, deviceIeeeBase);
            //MeasMinimum = new MeasMinVolt(this, deviceIeeeBase);
            //MeasPeakToPeak = new MeasVpp(this, deviceIeeeBase);
            //MeasPeriod = new MeasPeriodTime(this, deviceIeeeBase);
            //MeasPeriodAver = new MeasAverPeriodTime(this, deviceIeeeBase);
            //MeasPositivePulseWidth = new MeasPosPulseWidth(this, deviceIeeeBase);
            //MeasNegativePulseWidth = new MeasNegPulseWidth(this, deviceIeeeBase);
        }
        /// <summary>
        /// Измерение частоты.
        /// </summary>
        public class MeasFreq : MeasBase<Frequency>
        {
            public MeasFreq(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) : base(counterAbstr,
                                                                                               deviceIeeeBase)
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
            public MeasFreqBurst(CounterInputAbstract counterAbstr, IeeeBase deviceIeeeBase) :
                base(counterAbstr, deviceIeeeBase)
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

            public MeasBase(CounterInputAbstract counterAbstr, IeeeBase devIeeeBase) : base(devIeeeBase)
            {
                CounterAbstr = counterAbstr;
            }

            /// <inheritdoc />
            public override void Setting()
            {
                //todo проверить проинициализированно ли к этому моменту устройство? задана ли строка подключения?
                CounterInput
                   .WriteLine($"inp{CounterAbstr.NameOfChanel}:imp {CounterAbstr.InputSetting.GetImpedance()}");
                CounterInput.WriteLine($"inp{CounterAbstr.NameOfChanel}:att {CounterAbstr.InputSetting.GetAtt()}");
                CounterInput.WriteLine($"inp{CounterAbstr.NameOfChanel}:coup {CounterAbstr.InputSetting.GetCouple()}");
                CounterInput.WriteLine($"inp{CounterAbstr.NameOfChanel}:slop {CounterAbstr.InputSetting.GetSlope()}");
                CounterInput
                   .WriteLine($"inp{CounterAbstr.NameOfChanel}:filt {CounterAbstr.InputSetting.GetFilterStatus()}");
            }

            public override void Getting()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}