using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Devices.IEEE.Keysight.PowerSupplyes.E36XXa;

namespace E363xAPlugin
{
    public class E3633A : Program<Operation<E3633ADeviceBasicFunction>>
    {
        public E3633A(ServicePack servicePack) : base(servicePack)
        {
            Grsi = "26951-04";
            Type = "E3633A";

            var testPowerSupply = new E3633ADeviceBasicFunction();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }

    public class E3632A : Program<Operation<E3633ADeviceBasicFunction>>
    {
        public E3632A(ServicePack servicePack) : base(servicePack)
        {
            Grsi = "26951-04";
            Type = "E3632A";

            var testPowerSupply = new E3632ADeviceBasicFunction();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }

    public class E3634A : Program<Operation<E3633ADeviceBasicFunction>>
    {
        public E3634A(ServicePack servicePack) : base(servicePack)
        {
            Grsi = "26951-04";
            Type = "E3634A";

            var testPowerSupply = new E3634ADeviceBasicFunction();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }
}
