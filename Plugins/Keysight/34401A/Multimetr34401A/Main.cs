using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Devices.IEEE.Keysight.Multimeter;

namespace Multimetr34401A
{
    public class Main : Program<Mult34401A>
    {
        /// <inheritdoc />
        public Main(ServicePack service) : base(service)
        {
            Type = "34401А";
            Grsi = "16500-97";
            Range = "-";
        }
    }
    public class Mult34401A : OperationMetrControlBase
    {

        public Mult34401A(ServicePack servicePac)
        {
            UserItemOperationPrimaryVerf = new OperationPrimary<Keysight34401A>("34401A Поверка 54848-13 и 16500-97.docx", servicePac);
            UserItemOperationPeriodicVerf = UserItemOperationPrimaryVerf;
        }

    }


}
