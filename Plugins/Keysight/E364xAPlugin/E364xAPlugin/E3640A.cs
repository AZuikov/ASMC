using System;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE.Keysight.PowerSupplyes;
using ASMC.Devices.IEEE.Keysight.PowerSupplyes.E36XXa;


namespace E364xAPlugin
{
    public abstract class E364XBaseProcedure<T> : Program<Operation<T>> where T : E36xxA_DeviceBasicFunction
    {
        public E364XBaseProcedure(ServicePack service) : base(service)
        {
            
            Grsi = "26951-04";
            var device = Activator.CreateInstance<T>();
            Type = device.UserType;

            foreach (var range in device.Ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {device.outputs.Length}";

        }
    }
    public class E3640A : E364XBaseProcedure<E3640ADevice>
    {
        public E3640A(ServicePack service) : base(service)
        {
           
            
        }
    }

    public class E3641A_Plugin : E364XBaseProcedure<E3641ADevice>
    {
        public E3641A_Plugin(ServicePack service) : base(service)
        {
           
        }
    }


    public class E3642A_Plugin : E364XBaseProcedure<E3642ADevice>
    {
        public E3642A_Plugin(ServicePack service) : base(service)
        {
            
            
        }
    }

    public class E3643A_Plugin : E364XBaseProcedure<E3643ADevice>
    {
        public E3643A_Plugin(ServicePack service) : base(service)
        {
           
        }
    }

    public class E3644A_Plugin : E364XBaseProcedure<E3644ADevice>
    {
        public E3644A_Plugin(ServicePack service) : base(service)
        {
           
        }
    }
    public class E3645A_Plugin :E364XBaseProcedure<E3645ADevice>
    {
        public E3645A_Plugin(ServicePack service) : base(service)
        {
           
        }
    }


    public class E3646A_Plugin :E364XBaseProcedure<E3646ADevice>
    {
        public E3646A_Plugin(ServicePack service) : base(service)
        {
            
        }
    }

    public class E3647A_Plugin :E364XBaseProcedure<E3647ADevice>
    {
        public E3647A_Plugin(ServicePack service) : base(service)
        {
           
        }
    }

    public class E3648A_Plugin :E364XBaseProcedure<E3648ADevice>
    {
        public E3648A_Plugin(ServicePack service) : base(service)
        {
           
        }
    }

    public class E3649A_Plugin :E364XBaseProcedure<E3649ADevice>
    {
        public E3649A_Plugin(ServicePack service) : base(service)
        {
           

        }
    }

}
