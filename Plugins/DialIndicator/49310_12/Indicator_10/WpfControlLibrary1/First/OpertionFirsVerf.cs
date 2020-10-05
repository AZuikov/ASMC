﻿using System;
using System.Linq;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Devices.USB_Device.SKBIS.Lir917;
using ASMC.MVision;

namespace Indicator_10.First
{
    public class OpertionFirsVerf : Operation
    {
        public OpertionFirsVerf(ServicePack servicePac):base(servicePac)
        {
            ControlDevices = new IDeviceUi[]
            {
                new Device {Devices = new IUserType[] {new Ppi()}},
                new Device {Devices = new IUserType[] {new WebCam()}}
            };
            Accessories = new[]
            {
                "Весы настольные циферблатные РН-3Ц13У",
                "Приспособление для определения измерительного усилия и его колебаний мод. 253",
                "Граммометр часового типа Г 3,0",
                "Прибор для поверки индикаторов ППИ-50"
            };
            this.UserItemOperation = new IUserItemOperationBase[]{new };
        }
        /// <inheritdoc />
        public override void RefreshDevice()
        {
            AddresDevice = ASMC.Devices.USB_Device.SiliconLabs.UsbExpressWrapper.FindAllDevice.Select(q => q.Number.ToString()).Concat(WebCam.GetVideoInputDevice.Select(q => q.MonikerString)).ToArray();
        }
        /// <inheritdoc />
        public override void FindDevice()
        {
            throw new NotImplementedException();
        }
    }
}