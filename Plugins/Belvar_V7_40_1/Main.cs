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
            Type = Название;
            Grsi = Горсреестр или методика;
            Range = Диапазон;
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
