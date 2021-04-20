using System;
using System.Threading.Tasks;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;

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

        
        public class MeasFreq :MeasBase,  IMeterPhysicalQuantity<Frequency>
        {
            
            public MeasFreq(CounterInputAbstract device):base(device)
            {
                
            }
            public void Getting()
            {
                throw new NotImplementedException();
            }

            public void Setting()
            {
                
            }

            public MeasPoint<Frequency> GetValue()
            {
                throw new NotImplementedException();
            }

            public MeasPoint<Frequency> Value { get; }
            public IRangePhysicalQuantity<Frequency> RangeStorage { get; }
        }

        public class MeasBase
        {
            
            public MeasBase(CounterInputAbstract device)
            {
                
            }

            public void Setting()
            {
                
                

            }
        }

        
    }
}