using System;
using System.Threading.Tasks;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Devices.Interface;

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
            
            Outputs = new ICounterInput[2]
            {
                new CNT90Input("1",this), 
                new CNT90Input("2",this), 
            };
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
    }
}