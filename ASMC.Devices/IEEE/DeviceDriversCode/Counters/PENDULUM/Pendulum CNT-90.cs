using System;
using System.Threading.Tasks;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;
using NLog.LayoutRenderers.Wrappers;
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
            
            
            InputA = new CNT90Input("1",this);
            InputB = new CNT90Input("2",this);
            InputE = new CNT90Input("3",this);
        }

        public override Task InitializeAsync()
        {
            //todo нужно как то проверять наличие опций и создавать нужную конфигурацию
            //получим список опций
            //var options= Device.GetOption();
            return InitializeAsync();

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

        #endregion
    }

    public class CNT90Input : CounterInputAbstract
    {
        
        public CNT90Input(string chanelName, CounterAbstract counter) :base (chanelName,counter)
        {
            
        }

        /// <summary>
        /// Измерение частоты.
        /// </summary>
        public class MeasFreq :MeasBase<Frequency>
        {
            
            public MeasFreq(CounterInputAbstract device):base(device)
            {
                
            }
            

            public void Setting()
            {
                base.Setting();
                CounterInput.WriteLine($"CONF:MEAS:FREQ (@{_device.NameOfChanel})");
               
            }
            

            public MeasPoint<Frequency> Value { get; }
            public IRangePhysicalQuantity<Frequency> RangeStorage { get; }
        }

        public class MeasBase<TPhysicalQuantity> : IMeterPhysicalQuantity<TPhysicalQuantity> where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
        {
            protected CounterInputAbstract _device;
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
                CounterInput.WriteLine($":FORMat ASCii");//формат получаемых от прибора данных
                CounterInput.WriteLine($":INITiate:CONTinuous 0");//выключаем многократный запуск
                CounterInput.WriteLine($":INITiate");//взводим триггер
                string answer = CounterInput.QueryLine($":FETCh?");//считываем ответ
                decimal value = (decimal)StrToDouble(answer);
                Value = new MeasPoint<TPhysicalQuantity>(value);
                return Value;
            }

            public MeasPoint<TPhysicalQuantity> Value { get; protected set; }
            public IRangePhysicalQuantity<TPhysicalQuantity> RangeStorage { get; }
        }

        
    }
}