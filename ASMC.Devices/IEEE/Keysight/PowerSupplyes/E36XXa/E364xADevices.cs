using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.IEEE.Keysight.PowerSupplyes.E36XXa
{
    public class E3640ADevice : E36xxA_Device
    {
        public E3640ADevice()
        {
            UserType = "E3640A";
            outputs = new[] { E364xChanels.OUTP1 };
            Ranges = new[]
            {
                new MeasPoint<Voltage, Current>(8, 3),
                new MeasPoint<Voltage, Current>(20M, 1.5M)
            };
        }
    }

    public class E3641ADevice : E36xxA_Device
    {
        public E3641ADevice()
        {
            UserType = "E3641A";
            outputs = new[] { E364xChanels.OUTP1 };
            Ranges = new[]
            {
                new MeasPoint<Voltage, Current>(35, 0.8M),
                new MeasPoint<Voltage, Current>(60M, 0.5M)
            };
        }
    }

    public class E3642ADevice : E36xxA_Device
    {
        public E3642ADevice()
        {
            UserType = "E3642A";
            outputs = new[] { E364xChanels.OUTP1 };
            Ranges = new[]
            {
                new MeasPoint<Voltage, Current>(8, 5),
                new MeasPoint<Voltage, Current>(20M, 2.5M)
            };
        }
    }

    public class E3643ADevice : E36xxA_Device
    {
        public E3643ADevice()
        {
            UserType = "E3643A";
            outputs = new[] { E364xChanels.OUTP1 };
            Ranges = new[]
            {
                new MeasPoint<Voltage, Current>(35, 1.4M),
                new MeasPoint<Voltage, Current>(60M, 0.8M)
            };
        }
    }

    public class E3644ADevice : E36xxA_Device
    {
        public E3644ADevice()
        {
            UserType = "E3644A";
            outputs = new[] { E364xChanels.OUTP1 };
            Ranges = new[]
            {
                new MeasPoint<Voltage, Current>(8, 8),
                new MeasPoint<Voltage, Current>(20M, 4M)
            };
        }
    }

    public class E3645ADevice : E36xxA_Device
    {
        public E3645ADevice()
        {
            UserType = "E3645A";
            outputs = new[] { E364xChanels.OUTP1 };
            Ranges = new[]
            {
                new MeasPoint<Voltage, Current>(35, 2.2M),
                new MeasPoint<Voltage, Current>(60M, 1.3M)
            };
        }
    }

    public class E3646ADevice : E36xxA_Device
    {
        public E3646ADevice()
        {
            UserType = "E3646A";
            outputs = new[] { E364xChanels.OUTP1, E364xChanels.OUTP2 };
            Ranges = new[]
            {
                new MeasPoint<Voltage, Current>(8, 3),
                new MeasPoint<Voltage, Current>(20M, 1.5M)
            };
        }
    }

    public class E3647ADevice : E36xxA_Device
    {
        public E3647ADevice()
        {
            UserType = "E3647A";
            outputs = new[] { E364xChanels.OUTP1, E364xChanels.OUTP2 };
            Ranges = new[]
            {
                new MeasPoint<Voltage, Current>(35, 0.8M),
                new MeasPoint<Voltage, Current>(60M, 0.5M)
            };
        }
    }

    public class E3648ADevice : E36xxA_Device
    {
        public E3648ADevice()
        {
            UserType = "E3648A";
            outputs = new[] { E364xChanels.OUTP1, E364xChanels.OUTP2 };
            Ranges = new[]
            {
                new MeasPoint<Voltage, Current>(8, 5),
                new MeasPoint<Voltage, Current>(20M, 2.5M)
            };
        }
    }

    public class E3649ADevice : E36xxA_Device
    {
        public E3649ADevice()
        {
            UserType = "E3649A";
            outputs = new[] { E364xChanels.OUTP1, E364xChanels.OUTP2 };
            Ranges = new[]
            {
                new MeasPoint<Voltage, Current>(35, 1.4M),
                new MeasPoint<Voltage, Current>(60M, 0.8M)
            };
        }
    }
}
