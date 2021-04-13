using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Utils.Data;
using ASMC.Devices.Interface;

namespace ASMC.Devices.IEEE.PENDULUM
{
    class Pendulum_CNT_90: ICounter
    {
        enum InstallTimebaseOption
        {
            [StringValue("Standard")]Standard,
            [StringValue("Option 19")]Option19,
            [StringValue("Option 30")]Option30,
            [StringValue("Option 40")]Option40,
            [StringValue("Rubidium")]Rubidium
        }

        enum  InstallPrescalerOption
        {
            [StringValue("0")]NullOption,
            [StringValue("Option 10")]Option10,
            [StringValue("Option 13")]Option13,
            [StringValue("Option 14")]Option14,
            [StringValue("Option 14B")]Option14B
        }

        enum InstallMicrowaveConverter
        {
        [StringValue("27GHz")]Microwave27GHz,
        [StringValue("40GHz")]Microwave40GHz,
        [StringValue("46GHz")]Microwave46GHz,
        [StringValue("60GHz")]Microwave60GHz
        }

        public Pendulum_CNT_90()
        {
            UserType = "CNT-90";
            //todo нужно как то проверять наличие опций и создавать нужную конфигурацию
        }
        public void SetExternalReferenceClock()
        {
            //:ROSCillator:SOURce EXT
            throw new NotImplementedException();
        }

        public void SetInternalReferenceClock()
        {
            //:ROSCillator:SOURce INT
            throw new NotImplementedException();
        }

        public string UserType { get; }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool IsTestConnect { get; }
        public async Task InitializeAsync()
        {
            throw new NotImplementedException();
        }

        public string StringConnection { get; set; }
    }
}
