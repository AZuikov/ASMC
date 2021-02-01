using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Devices.WithoutInterface.HourIndicator.MC52415_13;

namespace DialIndicatorSeries801
{
    public class Main : Program<S801>
    {
        /// <inheritdoc />
        public Main(ServicePack service) : base(service)
        {
            Type = "Идикатор часового типа серия 801";
            Grsi = "МП 52415-13";
            Range = "(0...10 мм)";
        }
    }
    public class S801 : OperationMetrControlBase
    {

        public S801(ServicePack servicePac)
        {
            UserItemOperationPrimaryVerf = new OperationPrimary<DISeries801>("МП 52415-13", servicePac);
        }
    }

   
}
