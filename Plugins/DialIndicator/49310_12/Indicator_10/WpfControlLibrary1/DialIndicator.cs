using ASMC.Core.Model;
using ASMC.Devices.UInterface.AnalogDevice.ViewModel;
using ASMC.Devices.WithoutInterface.HourIndicator;
using ASMC.Devices.WithoutInterface.HourIndicator.IchGost577;
using mp2192_92.DialIndicator.First;
using mp2192_92.DialIndicator.Periodic;

namespace mp2192_92.DialIndicator
{
    #region DialIndicator_Range_10

    /// <inheritdoc />
    public class DialIndicator_10 : Program<Verefication_10>
    {
        /// <inheritdoc />
        public DialIndicator_10(ServicePack service) : base(service)
        {
            Type = "ИЧ10";
            Grsi = "МП 2192-92";
            Range = "(0...10) мм";
        }
    }

    public class Verefication_10 : OperationMetrControlBase
    {

        public Verefication_10(ServicePack servicePac)
        {
            UserItemOperationPrimaryVerf = new OpertionFirsVerf<IchGost577SettingUi<Ich_10>>("ИЧ-10 Первичная МП 2192-92", servicePac);
            UserItemOperationPeriodicVerf = new OpertionPeriodicVerf<IchGost577SettingUi<Ich_10>>("ИЧ-10 Периодическая МП 2192-92",servicePac);
            
        }
        
    }

    #endregion

    #region DialIndicator_Range_2
    /// <inheritdoc />
    public class DialIndicator2 : Program<Verefication_2>
    {
        /// <inheritdoc />
        public DialIndicator2(ServicePack service) : base(service)
        {
            Type = "ИЧ02";
            Grsi = "МП 2192-92";
            Range = "(0...2) мм";
        }
    }

    public class Verefication_2 : OperationMetrControlBase
    {

        public Verefication_2(ServicePack servicePac)
        {
            UserItemOperationPrimaryVerf = new OpertionFirsVerf<IchGost577SettingUi<Ich_02>>("ИЧ-02 Первичная МП 2192-92", servicePac);
            UserItemOperationPeriodicVerf = new OpertionPeriodicVerf<IchGost577SettingUi<Ich_02>>("ИЧ-02 Периодическая МП 2192-92", servicePac);

        }

    }


    #endregion

    #region DialIndicator_Range_3

    /// <inheritdoc />
    public class DialIndicator3 : Program<Verefication_3>
    {
        /// <inheritdoc />
        public DialIndicator3(ServicePack service) : base(service)
        {
            Type = "ИЧ03";
            Grsi = "МП 2192-92";
            Range = "(0...3) мм";
        }
    }

    public class Verefication_3 : OperationMetrControlBase
    {

        public Verefication_3(ServicePack servicePac)
        {
            UserItemOperationPrimaryVerf = new OpertionFirsVerf<IchGost577SettingUi<Ich_03>>("ИЧ-03 Первичная МП 2192-92", servicePac);
            UserItemOperationPeriodicVerf = new OpertionPeriodicVerf<IchGost577SettingUi<Ich_03>>("ИЧ-03 Периодическая МП 2192-92", servicePac);

        }

    }

    #endregion
}
