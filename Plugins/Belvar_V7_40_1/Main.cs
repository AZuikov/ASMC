using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.Model;

namespace Belvar_V7_40_1
{
    public class Main : Program<S801>
    {
        /// <inheritdoc />
        public Main(ServicePack service) : base(service)
        {
            Type = "В7-40/1";
            Grsi = "39075-08";
            Range = "Uпост. 0 - 2000 В, Uпер. до 1000 В, R до 20 МОм, Iпост. до 10 А, Iпер. до 10 А";
        }
    }
    public class S801 : OperationMetrControlBase
    {

        public S801(ServicePack servicePac)
        {
            UserItemOperationPrimaryVerf = new OperationPrimary<Тип устройства>(Название методики МК, servicePac);
        }

    }


}
