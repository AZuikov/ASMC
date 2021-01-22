using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.Model;

namespace FrequencyCounter
{
    public class Class1: Program<Ch3_85_3>
    {
        /// <inheritdoc />
        public Class1(ServicePack service) : base(service)
        {
            Type = "ИЧ10";
            Grsi = "";
            Range = "(0...10) мм";
        }
    }
    public class Ch3_85_3 : OperationMetrControlBase
    {

        public Ch3_85_3(ServicePack servicePac)
        {
            //UserItemOperationPrimaryVerf = new OpertionFirsVerf<IchGost577SettingUi<Ich_10>>("ИЧ-10 Первичная МП 2192-92", servicePac);
            //UserItemOperationPeriodicVerf = new OpertionPeriodicVerf<IchGost577SettingUi<Ich_10>>("ИЧ-10 Периодическая МП 2192-92", servicePac);

        }

    }
}
