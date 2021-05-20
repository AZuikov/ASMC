using System;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;
using static ASMC.Devices.HelpDeviceBase;

namespace ASMC.Devices.IEEE.PENDULUM
{
    public class CNT90InputMeasureFunction: ICounterStandartMeasureOperation
    {
        public CNT90InputMeasureFunction(CNT90Input cnt90Abstr,int ChanelNum, IeeeBase deviceIeeeBase)
        {
            MeasFrequency = new MeasFreq(cnt90Abstr, ChanelNum, deviceIeeeBase);
            MeasPeriod = new MeasPeriodTime(cnt90Abstr, ChanelNum, deviceIeeeBase);
            
        }
        /// <summary>
        /// Измерение частоты.
        /// </summary>
        public class MeasFreq : MeasReadValue<Frequency>
        {
            public MeasFreq(CNT90Input cnt90Abstr, int chanelNumb, IeeeBase deviceIeeeBase) :
                base(cnt90Abstr, chanelNumb, deviceIeeeBase)
            {
                FunctionName = "FREQ";
            }

            
        }

        public class MeasFreqBurst : MeasReadValue<Frequency>
        {
            public MeasFreqBurst(CNT90Input cnt90Abstr, int chanelNumb, IeeeBase deviceIeeeBase) :
                base(cnt90Abstr, chanelNumb, deviceIeeeBase)
            {
                FunctionName = "FREQ:BURSt";
            }

            
        }

        public class MeasCyclesInBurst : MeasReadValue<NoUnits>
        {
            public MeasCyclesInBurst(CNT90Input cnt90Abstr, int chanelNumb, IeeeBase deviceIeeeBase) :
                base(cnt90Abstr, chanelNumb, deviceIeeeBase)
            {
            }

            public override void Setting()
            {
                base.Setting();
                device.WriteLine($":CONF:MEASure:VOLT:NCYCles (@{Cnt90Abstr.NameOfChanel})");
            }
        }

        public class MeasFreqPRF : MeasReadValue<Frequency>
        {
            public MeasFreqPRF(CNT90Input cnt90Abstr, int chanelNumb, IeeeBase deviceIeeeBase) :
                base(cnt90Abstr, chanelNumb, deviceIeeeBase)
            {
            }

            #region Methods

            public override void Setting()
            {
                base.Setting();
                device.WriteLine($":CONF:MEAS:FREQ:PRF (@{Cnt90Abstr.NameOfChanel})");
            }

            #endregion Methods
        }

        public class MeasPositiveDUTycycle : MeasReadValue<Percent>
        {
            public MeasPositiveDUTycycle(CNT90Input cnt90Abstr, int chanelNumb, IeeeBase deviceIeeeBase) :
                base(cnt90Abstr, chanelNumb, deviceIeeeBase)
            {
            }

            #region Methods

            public override void Setting()
            {
                base.Setting();
                device.WriteLine($":CONF:MEAS:PDUTycycle (@{Cnt90Abstr.NameOfChanel})");
            }

            #endregion Methods
        }

        public class MeasNegativeDUTycycle : MeasReadValue<Percent>
        {
            public MeasNegativeDUTycycle(CNT90Input cnt90Abstr, int chanelNumb, IeeeBase deviceIeeeBase) :
                base(cnt90Abstr, chanelNumb, deviceIeeeBase)
            {
            }

            #region Methods

            public override void Setting()
            {
                base.Setting();
                device.WriteLine($":CONF:MEAS:NDUTycycle (@{Cnt90Abstr.NameOfChanel})");
            }

            #endregion Methods
        }

        public class MeasMaxVolt : MeasReadValue<Voltage>
        {
            public MeasMaxVolt(CNT90Input cnt90Abstr, int chanelNumb, IeeeBase deviceIeeeBase) :
                base(cnt90Abstr, chanelNumb, deviceIeeeBase)
            {
            }

            #region Methods

            public override void Setting()
            {
                base.Setting();
                device.WriteLine($":CONF:MEAS:VOLT:MAXimum (@{Cnt90Abstr.NameOfChanel})");
            }

            #endregion Methods
        }

        public class MeasMinVolt : MeasReadValue<Voltage>
        {
            public MeasMinVolt(CNT90Input cnt90Abstr, int chanelNumb, IeeeBase deviceIeeeBase) :
                base(cnt90Abstr, chanelNumb, deviceIeeeBase)
            {
            }

            #region Methods

            public override void Setting()
            {
                base.Setting();
                device.WriteLine($":CONF:MEAS:VOLT:MINimum (@{Cnt90Abstr.NameOfChanel})");
            }

            #endregion Methods
        }

        public class MeasVpp : MeasReadValue<Voltage>
        {
            public MeasVpp(CNT90Input cnt90Abstr, int chanelNumb, IeeeBase deviceIeeeBase) :
                base(cnt90Abstr, chanelNumb, deviceIeeeBase)
            {
            }

            #region Methods

            public override void Setting()
            {
                base.Setting();
                device.WriteLine($":CONF:MEAS:VOLT:PTPeak (@{Cnt90Abstr.NameOfChanel})");
            }

            #endregion Methods
        }

        public class MeasPeriodTime : MeasReadValue<Time>
        {
            public MeasPeriodTime(CNT90Input cnt90Abstr, int chanelNumb, IeeeBase deviceIeeeBase) :
                base(cnt90Abstr, chanelNumb, deviceIeeeBase)
            {
            }

            #region Methods

            public override void Setting()
            {
                base.Setting();
                device.WriteLine($":CONF:MEAS:PERiod (@{Cnt90Abstr.NameOfChanel})");
            }

            #endregion Methods
        }

        public class MeasAverPeriodTime : MeasReadValue<Time>
        {
            public MeasAverPeriodTime(CNT90Input cnt90Abstr, int chanelNumb, IeeeBase deviceIeeeBase) :
                base(cnt90Abstr, chanelNumb, deviceIeeeBase)
            {
            }

            #region Methods

            public override void Setting()
            {
                base.Setting();
                device.WriteLine($":CONF:MEAS:PERiod:AVERage (@{Cnt90Abstr.NameOfChanel})");
            }

            #endregion Methods
        }

        public class MeasPosPulseWidth : MeasReadValue<Time>
        {
            public MeasPosPulseWidth(CNT90Input cnt90Abstr, int chanelNumb, IeeeBase deviceIeeeBase) :
                base(cnt90Abstr, chanelNumb, deviceIeeeBase)
            {
            }

            #region Methods

            public void Setting()
            {
                base.Setting();
                device.WriteLine($":CONF:MEAS:PWIDth (@{Cnt90Abstr.NameOfChanel})");
            }

            #endregion Methods
        }

        public class MeasNegPulseWidth : MeasReadValue<Time>
        {
            public MeasNegPulseWidth(CNT90Input cnt90Abstr,int chanelNumb, IeeeBase deviceIeeeBase) :
                base(cnt90Abstr, chanelNumb,deviceIeeeBase)
            {
            }

            #region Methods

            public override void Setting()
            {
                base.Setting();
                device.WriteLine($":CONF:MEAS:NWIDth (@{Cnt90Abstr.NameOfChanel})");
            }

            #endregion Methods
        }

        

        public IMeterPhysicalQuantity<Frequency> MeasFrequency { get; set; }
        public IMeterPhysicalQuantity<Time> MeasPeriod { get; set; }


        public virtual void Getting()
        {
            throw new NotImplementedException();
        }

        public virtual void Setting()
        {
            throw new NotImplementedException();
        }
    }

    public abstract class MeasReadValue<TPhysicalQuantity> :  IMeterPhysicalQuantity<TPhysicalQuantity>
        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
    {
        protected IeeeBase device;
        protected string FunctionName { get; set; }
        protected CNT90Input Cnt90Abstr { get; }

        public MeasReadValue(CNT90Input cnt90Abstr, int chanelNumb, IeeeBase inDevice)
        {
            device = inDevice;
            Cnt90Abstr = cnt90Abstr;
        }
        public MeasPoint<TPhysicalQuantity> GetValue()
        {
            //считывание измеренного значения, сработает при условии, что перед этим был запущен метод Setting
            device.WriteLine(":FORMat ASCii"); //формат получаемых от прибора данных
            device.WriteLine(":INITiate:CONTinuous 0"); //выключаем многократный запуск
            device.WriteLine(":INITiate"); //взводим триггер
            var answer = device.QueryLine(":CALC:DATA?"); //считываем ответ (через усреденение)
            //var answer = device.QueryLine(":FETCh?"); //считываем ответ
            //var answer = device.QueryLine($":Measure:{FunctionName}?"); //считываем ответ
            var value = (decimal)StrToDouble(answer);
            Value = new MeasPoint<TPhysicalQuantity>(value);
            return Value;
        }

        public MeasPoint<TPhysicalQuantity> Value { get; protected set; }

        public virtual void Getting()
        {
            throw  new NotImplementedException();
        }
        public virtual void Setting()
        {
            device.WriteLine($":CONF:MEAS:{FunctionName} (@{Cnt90Abstr.NameOfChanel})");
        }
        public IRangePhysicalQuantity<TPhysicalQuantity> RangeStorage { get; }
    }
}