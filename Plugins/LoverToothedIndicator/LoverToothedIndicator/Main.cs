using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.Model;

namespace LTI
{
    public class Main : Program<Lti192888>
    {
        /// <inheritdoc />
        public Main(ServicePack service) : base(service)
        {
            Type = "ИРБ";
            Grsi = "МИ 1928-88";
            Range = "(0...0,8) мм";
        }
    }
    public class Lti192888 : OperationMetrControlBase
    {

        public Lti192888(ServicePack servicePac)
        {
            //UserItemOperationPrimaryVerf = new OpertionFirsVerf<IchGost577SettingUi<Ich_10>>("ИЧ-10 Первичная МП 2192-92", servicePac);
            //UserItemOperationPeriodicVerf = new OpertionPeriodicVerf<IchGost577SettingUi<Ich_10>>("ИЧ-10 Периодическая МП 2192-92", servicePac);
        }

    }
}
