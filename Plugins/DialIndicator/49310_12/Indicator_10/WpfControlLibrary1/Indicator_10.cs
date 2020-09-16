using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.MVision;

namespace Indicator_10
{
    public class Indicator_10 : Program
    {
        /// <inheritdoc />
        public Indicator_10(ServicePack service) : base(service)
        {
            this.Type = "Тест";
            Operation = new Verefication(service);
        }
    }

    public class Verefication : OperationMetrControlBase
    {

        public Verefication(ServicePack servicePac)
        {
            this.UserItemOperationPrimaryVerf = new OpertionFirsVerf(servicePac);
            this.UserItemOperationPeriodicVerf = new OpertionPeriodicVerf(servicePac);
        }
    }

    public class OpertionPeriodicVerf : Operation
    {
        public OpertionPeriodicVerf(ServicePack servicePac):base(servicePac)
        {
            ControlDevices = new IDeviceUi[]
            {
                new DeviceInterface {Name = new[] {"ППИ-50"}, Description = "Прибор ППИ"},
                new DeviceInterface{Description = "Веб камера", Name = new []{"Веб камера"}}
            };
            var a = new Operation1(this);
            a.Nodes.Add(new Operation2(this));

            a.Nodes.Add(new Operation1(this));
            this.UserItemOperation = new IUserItemOperationBase[] { new Operation1(this), new Operation1(this), a };
            Accessories = new[]
            {
                "Мультиметр цифровой Agilent/Keysight 34401A",
                "Кабель banana"
            };
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

    public class OpertionFirsVerf : Operation
    {
        public OpertionFirsVerf(ServicePack servicePac)
        {
            throw new NotImplementedException();
        }
    }
}
