﻿using System;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Devices.UInterface.RemoveDevice.ViewModel;
using ASMC.Devices.USB_Device.SKBIS.Lir917;
using ASMC.Devices.WithoutInterface.HourIndicator;

namespace Indicator_10.Periodic
{
    public class OpertionPeriodicVerf<T> : Operation where T : IchGost577, new()
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
                    Devices = new IUserType[]
                    {

                        new T()
                    }
                }
            };

            UserItemOperation = new IUserItemOperationBase[]
            {
                new MeasuringForce(this), 
                new PerpendicularPressure(this),
                new RangeIndications(this),
                new  VariationReading(this)
            };
            Accessories = new[]
            {
                "Весы настольные циферблатные РН-3Ц13У",
                "Приспособление для определения измерительного усилия и его колебаний мод. 253",
                "Граммометр часового типа Г 3,0",
                "Прибор для поверки индикаторов ППИ-50"
            };
            DocumentName = "ИЧ-10 Периодическая МП 2192-92";
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