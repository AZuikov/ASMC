using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AP.Utils.Helps;
using ASMC.Common.UI;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.UInterface.AnalogDevice.ViewModel;
using ASMC.Devices.UInterface.RemoveDevice.ViewModel;
using ASMC.Devices.USB_Device.SKBIS.Lir917;
using ASMC.Devices.WithoutInterface.HourIndicator;

namespace Indicator_10.Periodic
{
    public class OpertionPeriodicVerf : Operation
    {
        public OpertionPeriodicVerf(ServicePack servicePac):base(servicePac)
        {
            ControlDevices = new IDeviceUi[]
            {
                new Device {Devices = new IUserType[] {new Ppi()}},
                new Device {Devices = new IUserType[] {new WebCamUi()}}
            };
            TestDevices = new IDeviceUi[]
            {
                new Device
                {
                    Devices = new IUserType[] {new IchBaseSettingUi {Range = new MeasPoint<Length>(10, UnitMultipliers.Mili), UserType="ИЧ, мод. ИЧ10" } },
                    IsCanStringConnect=false, 
                    Description = "Индикатор часового типа",

                }
            };
            UserItemOperation = new IUserItemOperationBase[]
            {
                new MeasuringForce(this)
            };
            Accessories = new[]
            {
                "Весы настольные циферблатные РН-3Ц13У",
                "Приспособление для определения измерительного усилия и его колебаний мод. 253",
                "Граммометр часового типа Г 3,0",
                "Прибор для поверки индикаторов ППИ-50"
            };
        }

        /// <inheritdoc />
        public override void RefreshDevice()
        {
           // AddresDevice = ASMC.Devices.USB_Device.SiliconLabs.UsbExpressWrapper.FindAllDevice.Select(q => q.Number.ToString()).Concat(WebCam.GetVideoInputDevice.Select(q => q.MonikerString)).ToArray();
        }

        /// <inheritdoc />
        public override void FindDevice()
        {
            throw new NotImplementedException();
        }
    }

}