using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.IEEE.Keysight.PowerSupplyes.E36XXa
{
    public class E3633ADeviceBasicFunction : E36xxA_DeviceBasicFunction
    {
        public E3633ADeviceBasicFunction()
        {
            UserType = "E3633A";
            outputs = new[] { E36xxChanels.OUTP1 };
            Ranges = new[]
            {
                new MeasPoint<Voltage, Current>(8, 20),
                new MeasPoint<Voltage, Current>(20, 10)
            };
        }
    }

    public class E3632ADeviceBasicFunction : E36xxA_DeviceBasicFunction
    {
        public E3632ADeviceBasicFunction()
        {
            UserType = "E3632A";
            outputs = new[] { E36xxChanels.OUTP1 };
            Ranges = new[]
            {
                new MeasPoint<Voltage, Current>(15, 7),
                new MeasPoint<Voltage, Current>(30, 4)
            };
        }
    }

    public class E3634ADeviceBasicFunction : E36xxA_DeviceBasicFunction
    {
        public E3634ADeviceBasicFunction()
        {
            UserType = "E3634A";
            outputs = new[] { E36xxChanels.OUTP1 };
            Ranges = new[]
            {
                new MeasPoint<Voltage, Current>(25, 7),
                new MeasPoint<Voltage, Current>(50, 4)
            };
        }
    }
}