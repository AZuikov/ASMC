using ASMC.Core.Model;
using ASMC.Devices.UInterface.AnalogDevice.ViewModel;
using ASMC.Devices.WithoutInterface.HourIndicator;
using mp2192_92.DialIndicator.First;
using mp2192_92.DialIndicator.Periodic;

namespace mp2192_92.DialIndicator
{
    /// <inheritdoc />
    // ReSharper disable once UnusedMember.Global
    public class DialIndicator10 : Program<Verefication>
    {
        /// <inheritdoc />
        public DialIndicator10(ServicePack service) : base(service)
        {
            Type = "ИЧ10";
            Grsi = "318-49, 32512-06, 33841-07, 40149-08, 42499-09, 49310-12, 54058-13, 57937-14, 64188-16, 69534-17, До 26 декабря 1991 года";
            Range = "(0...10) мм";
        }
    }

    public class Verefication : OperationMetrControlBase
    {

        public Verefication(ServicePack servicePac)
        {
            UserItemOperationPrimaryVerf = new OpertionFirsVerf<IchGost577SettingUi<Ich_10>>(servicePac);
            UserItemOperationPeriodicVerf = new OpertionPeriodicVerf<IchGost577SettingUi<Ich_10>>(servicePac);
        }
    }


}
