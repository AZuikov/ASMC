using ASMC.Core.Model;

using ASMC.Devices.IEEE.Keysight.PowerSupplyes.E36XXa;


namespace E364xAPlugin
{
    public class E3640A : Program<Operation<E3640ADeviceBasicFunction>>
    {
        public E3640A(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3640A";

            var testPowerSupply = new E3640ADeviceBasicFunction();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";

        }
    }

    public class E3641A_Plugin : Program<Operation<E3641ADeviceBasicFunction>>
    {
        public E3641A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3641A";

            var testPowerSupply = new E3641ADeviceBasicFunction();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }


    public class E3642A_Plugin : ASMC.Core.Model.Program<Operation<E3642ADeviceBasicFunction>>
    {
        public E3642A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3642A";

            var testPowerSupply = new E3642ADeviceBasicFunction();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }

    public class E3643A_Plugin : ASMC.Core.Model.Program<Operation<E3643ADeviceBasicFunction>>
    {
        public E3643A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3643A";

            var testPowerSupply = new E3643ADeviceBasicFunction();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }

    public class E3644A_Plugin : ASMC.Core.Model.Program<Operation<E3644ADeviceBasicFunction>>
    {
        public E3644A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3644A";

            var testPowerSupply = new E3644ADeviceBasicFunction();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }
    public class E3645A_Plugin : ASMC.Core.Model.Program<Operation<E3645ADeviceBasicFunction>>
    {
        public E3645A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3645A";

            var testPowerSupply = new E3645ADeviceBasicFunction();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }


    public class E3646A_Plugin : ASMC.Core.Model.Program<Operation<E3646ADeviceBasicFunction>>
    {
        public E3646A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3646A";

            var testPowerSupply = new E3646ADeviceBasicFunction();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }

    public class E3647A_Plugin : ASMC.Core.Model.Program<Operation<E3647ADeviceBasicFunction>>
    {
        public E3647A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3647A";

            var testPowerSupply = new E3647ADeviceBasicFunction();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }

    public class E3648A_Plugin : ASMC.Core.Model.Program<Operation<E3648ADeviceBasicFunction>>
    {
        public E3648A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3648A";

            var testPowerSupply = new E3648ADeviceBasicFunction();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";
        }
    }

    public class E3649A_Plugin : ASMC.Core.Model.Program<Operation<E3649ADeviceBasicFunction>>
    {
        public E3649A_Plugin(ServicePack service) : base(service)
        {
            Grsi = "26951-04";
            Type = "E3649A";
            
            var testPowerSupply = new E3649ADeviceBasicFunction();
            var ranges = testPowerSupply.Ranges;
            foreach (var range in ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {testPowerSupply.outputs.Length}";

        }
    }

}
