using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Devices.IEEE.Keysight.PowerSupplyes;
using ASMC.Devices.IEEE.Keysight.PowerSupplyes.E36XXa;

namespace E363xAPlugin
{
    public abstract class E363XBaseProcedure<T> : Program<Operation<T>> where T : E36xxA_DeviceBasicFunction
    {
        public E363XBaseProcedure(ServicePack service) : base(service)
        {
            
            Grsi = "26950-04";
            var device = Activator.CreateInstance<T>();
            Type = device.UserType;

            foreach (var range in device.Ranges)
                Range = Range + $"{range.Description}; ";
            Range = Range + $"Число каналов: {device.outputs.Length}";

        }
    }

    public class E3633A : E363XBaseProcedure<E3633ADevice>
    {
        public E3633A(ServicePack servicePack) : base(servicePack)
        {
           
        }
    }

    public class E3632A : E363XBaseProcedure<E3632ADevice>
    {
        public E3632A(ServicePack servicePack) : base(servicePack)
        {
            
        }
    }

    public class E3634A : E363XBaseProcedure<E3634ADevice>
    {
        public E3634A(ServicePack servicePack) : base(servicePack)
        {
           
        }
    }
}
