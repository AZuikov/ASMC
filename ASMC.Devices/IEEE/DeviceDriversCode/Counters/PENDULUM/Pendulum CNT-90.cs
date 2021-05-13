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
        
        
        public Pendulum_CNT_90()
        {
            UserType = "CNT-90";

            InputA = new CNT90Input(1, counter);
            InputB = new CNT90Input(2, counter);

            DualChanelMeasure = new CNT90DualChanelMeasure(InputA, InputB,counter);
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
            //если опция на второй позиции есть, значит точно есть третий выход
            if (!options[1].Equals(InstallPrescalerOption.NullOption.GetStringValue()))
            {
                InputC_HighFrequency = new CNT90InputC_HighFreq(3, counter);
            }
        }
    }

    public class CNT90Input : CounterInputAbstract
    {
        public CNT90Input(int chanelName,  IeeeBase deviceIeeeBase) : base(chanelName)
        {
            MeasFrequency = new CNT90InputMeasureFunction.MeasFreq(this, deviceIeeeBase);
            MeasFrequencyBURSt = new CNT90InputMeasureFunction.MeasFreqBurst(this, deviceIeeeBase);
            MeasNumberOfCyclesInBurst = new CNT90InputMeasureFunction.MeasCyclesInBurst(this, deviceIeeeBase);
            MeasPulseRepetitionFrequencyBurstSignal = new CNT90InputMeasureFunction.MeasFreqPRF(this, deviceIeeeBase);
            MeasPositiveDutyCycle = new CNT90InputMeasureFunction.MeasPositiveDUTycycle(this, deviceIeeeBase);
            MeasNegativeDutyCycle = new CNT90InputMeasureFunction.MeasNegativeDUTycycle(this, deviceIeeeBase);
            MeasMaximum = new CNT90InputMeasureFunction.MeasMaxVolt(this, deviceIeeeBase);
            MeasMinimum = new CNT90InputMeasureFunction.MeasMinVolt(this, deviceIeeeBase);
            MeasPeakToPeak = new CNT90InputMeasureFunction.MeasVpp(this, deviceIeeeBase);
            MeasPeriod = new CNT90InputMeasureFunction.MeasPeriodTime(this, deviceIeeeBase);
            MeasPeriodAver = new CNT90InputMeasureFunction.MeasAverPeriodTime(this, deviceIeeeBase);
            MeasPositivePulseWidth = new CNT90InputMeasureFunction.MeasPosPulseWidth(this, deviceIeeeBase);
            MeasNegativePulseWidth = new CNT90InputMeasureFunction.MeasNegPulseWidth(this, deviceIeeeBase);
        }
    }

    public class CNT90InputC_HighFreq : CounterInputAbstractHF
    {
        public CNT90InputC_HighFreq(int chanelName, IeeeBase deviceIeeeBase) : base(chanelName)
        {
            MeasFrequency = new CNT90InputMeasureFunction.MeasFreq(this, deviceIeeeBase);
            MeasFrequencyBURSt = new CNT90InputMeasureFunction.MeasFreqBurst(this, deviceIeeeBase);
            MeasNumberOfCyclesInBurst = new CNT90InputMeasureFunction.MeasCyclesInBurst(this, deviceIeeeBase);
            MeasPulseRepetitionFrequencyBurstSignal = new CNT90InputMeasureFunction.MeasFreqPRF(this, deviceIeeeBase);
            MeasPositiveDutyCycle = new CNT90InputMeasureFunction.MeasPositiveDUTycycle(this, deviceIeeeBase);
            MeasNegativeDutyCycle = new CNT90InputMeasureFunction.MeasNegativeDUTycycle(this, deviceIeeeBase);
            MeasMaximum = new CNT90InputMeasureFunction.MeasMaxVolt(this, deviceIeeeBase);
            MeasMinimum = new CNT90InputMeasureFunction.MeasMinVolt(this, deviceIeeeBase);
            MeasPeakToPeak = new CNT90InputMeasureFunction.MeasVpp(this, deviceIeeeBase);
            MeasPeriod = new CNT90InputMeasureFunction.MeasPeriodTime(this, deviceIeeeBase);
            MeasPeriodAver = new CNT90InputMeasureFunction.MeasAverPeriodTime(this, deviceIeeeBase);
            MeasPositivePulseWidth = new CNT90InputMeasureFunction.MeasPosPulseWidth(this, deviceIeeeBase);
            MeasNegativePulseWidth = new CNT90InputMeasureFunction.MeasNegPulseWidth(this, deviceIeeeBase);
        }
    }

    public class CNT90DualChanelMeasure : CounterDualChanelMeasureAbstract
    {
        public CNT90DualChanelMeasure(ICounterInput chnA, ICounterInput chnB,IeeeBase deviceIeeeBase) 
        {
            MeasFrequencyRatioAB = new FreqRatio(chnA, chnB,  deviceIeeeBase);
            MeasFrequencyRatioBA = new FreqRatio(chnB, chnA,  deviceIeeeBase);
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