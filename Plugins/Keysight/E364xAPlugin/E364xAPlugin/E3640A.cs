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
    public class E3640A : Program<Operation<ASMC.Devices.IEEE.Keysight.PowerSupplyes.E3640A>>
    {
        public E3640A(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3640A";

            var testPowerSupply = new ASMC.Devices.IEEE.Keysight.PowerSupplyes.E3640A();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";

        }
    }

    public class E3641A_Plugin : Program<Operation<E3641A>>
    {
        public E3641A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3641A";

            var testPowerSupply = new E3641A();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }


    public class E3642A_Plugin : ASMC.Core.Model.Program<Operation<E3642A>>
    {
        public E3642A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3642A";

            var testPowerSupply = new E3642A();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }

    public class E3643A_Plugin : ASMC.Core.Model.Program<Operation<E3643A>>
    {
        public E3643A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3643A";

            var testPowerSupply = new E3643A();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }

    public class E3644A_Plugin : ASMC.Core.Model.Program<Operation<E3644A>>
    {
        public E3644A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3644A";

            var testPowerSupply = new E3644A();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }
    public class E3645A_Plugin : ASMC.Core.Model.Program<Operation<E3645A>>
    {
        public E3645A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3645A";

            var testPowerSupply = new E3645A();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }


    public class E3646A_Plugin : ASMC.Core.Model.Program<Operation<E3646A>>
    {
        public E3646A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3646A";

            var testPowerSupply = new E3646A();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }

    public class E3647A_Plugin : ASMC.Core.Model.Program<Operation<E3647A>>
    {
        public E3647A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3647A";

            var testPowerSupply = new E3647A();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }

    public class E3648A_Plugin : ASMC.Core.Model.Program<Operation<E3648A>>
    {
        public E3648A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3648A";

            var testPowerSupply = new E3648A();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }

    public class E3649A_Plugin : ASMC.Core.Model.Program<Operation<E3649A>>
    {
        public E3649A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3649A";
            
            var testPowerSupply = new E3649A();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";

        }
    }

}
