using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.IEEE.Keysight.PowerSupplyes.E36XXa
{
    public class E3633ADevice : E36xxA_Device
    {
        public E3633ADevice()
        {
            UserType = "E3633A";
            outputs = new[] { E364xChanels.OUTP1 };
            Ranges = new[]
            {
                new MeasPoint<Voltage, Current>(8, 20),
                new MeasPoint<Voltage, Current>(20, 10)
            };
        }
    }

    public class E3632ADevice : E36xxA_Device
    {
        public E3632ADevice()
        {
            UserType = "E3632A";
            outputs = new[] { E364xChanels.OUTP1 };
            Ranges = new[]
            {
                new MeasPoint<Voltage, Current>(15, 7),
                new MeasPoint<Voltage, Current>(30, 4)
            };
        }
    }

    public class E3634ADevice : E36xxA_Device
    {
        public E3634ADevice()
        {
            UserType = "E3634A";
            outputs = new[] { E364xChanels.OUTP1 };
            Ranges = new[]
            {
                new MeasPoint<Voltage, Current>(25, 7),
                new MeasPoint<Voltage, Current>(50, 4)
            };
        }
    }
}