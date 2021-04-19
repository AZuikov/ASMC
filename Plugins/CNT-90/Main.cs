using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Devices.IEEE.PENDULUM;

namespace CNT_90
{
    public class Main : Program<S801>
    {
        /// <inheritdoc />
        public Main(ServicePack service) : base(service)
        {
            Type = "CNT-90";
            Grsi = "41567-09";
            Range = "";
        }
    }
    public class S801 : OperationMetrControlBase
    {

        public S801(ServicePack servicePac)
        {
            UserItemOperationPrimaryVerf = new OperationPrimary<Pendulum_CNT_90>("CNT-90", servicePac);
        }

    }


}
