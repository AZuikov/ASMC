using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Devices.IEEE.Keysight.PowerSupplyes;
using E364xAPlugin;

namespace E364xAPlugin
{
    public class E3640A : Program<Operation<ASMC.Devices.IEEE.Keysight.PowerSupplyes.E3640ADevice>>
    {
        public E3640A(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3640A";

            var testPowerSupply = new ASMC.Devices.IEEE.Keysight.PowerSupplyes.E3640ADevice();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";

        }
    }

    public class E3641A_Plugin : Program<Operation<E3641ADevice>>
    {
        public E3641A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3641A";

            var testPowerSupply = new E3641ADevice();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }


    public class E3642A_Plugin : ASMC.Core.Model.Program<Operation<E3642ADevice>>
    {
        public E3642A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3642A";

            var testPowerSupply = new E3642ADevice();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }

    public class E3643A_Plugin : ASMC.Core.Model.Program<Operation<E3643ADevice>>
    {
        public E3643A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3643A";

            var testPowerSupply = new E3643ADevice();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }

    public class E3644A_Plugin : ASMC.Core.Model.Program<Operation<E3644ADevice>>
    {
        public E3644A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3644A";

            var testPowerSupply = new E3644ADevice();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }
    public class E3645A_Plugin : ASMC.Core.Model.Program<Operation<E3645ADevice>>
    {
        public E3645A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3645A";

            var testPowerSupply = new E3645ADevice();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }


    public class E3646A_Plugin : ASMC.Core.Model.Program<Operation<E3646ADevice>>
    {
        public E3646A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3646A";

            var testPowerSupply = new E3646ADevice();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }

    public class E3647A_Plugin : ASMC.Core.Model.Program<Operation<E3647ADevice>>
    {
        public E3647A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3647A";

            var testPowerSupply = new E3647ADevice();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }

    public class E3648A_Plugin : ASMC.Core.Model.Program<Operation<E3648ADevice>>
    {
        public E3648A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3648A";

            var testPowerSupply = new E3648ADevice();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }

    public class E3649A_Plugin : ASMC.Core.Model.Program<Operation<E3649ADevice>>
    {
        public E3649A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3649A";
            
            var testPowerSupply = new E3649ADevice();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";

        }
    }

}
