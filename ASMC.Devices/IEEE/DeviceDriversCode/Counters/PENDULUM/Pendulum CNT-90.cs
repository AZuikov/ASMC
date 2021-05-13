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

            InputA = new CNT90Input(1, DeviceIeeeBase);
            InputB = new CNT90Input(2, DeviceIeeeBase);

           // DualChanelMeasure = new CNT90DualChanelMeasure(InputA, InputB,DeviceIeeeBase);
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
            var options = DeviceIeeeBase.GetOption();
            //если опция на второй позиции есть, значит точно есть третий выход
            if (!options[1].Equals(InstallPrescalerOption.NullOption.GetStringValue()))
            {
                //InputC_HighFrequency = new CNT90InputC_HighFreq(3, DeviceIeeeBase);
            }
        }
    }

    public class CNT90Input : CounterInputAbstract
    {
        public CNT90Input(int chanelName,  IeeeBase deviceIeeeBase) : base(chanelName, deviceIeeeBase)
        {
            
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