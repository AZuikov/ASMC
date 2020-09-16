using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Devices.USB_Device.SKBIS.Lir917;
using ASMC.MVision;

namespace Indicator_10
{
    public class Indicator_10 : Program<Verefication>
    {
        /// <inheritdoc />
        public Indicator_10(ServicePack service) : base(service)
        {
            this.Type = "ИЧ10";
            this.Grsi = "318-49, 32512-06, 33841-07, 40149-08, 42499-09, 49310-12, 54058-13, 57937-14, 64188-16, 69534-17, До 26 декабря 1991 года";
            this.Range = "(0...10) мм";
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
                new Device {Devices = new IUserType[] {new Ppi()}},
                new Device {Devices = new IUserType[] {new WebCam()}}
            };
            //var a = new Operation1(this);
            //a.Nodes.Add(new Operation2(this));

            //a.Nodes.Add(new Operation1(this));
            //this.UserItemOperation = new IUserItemOperationBase[] { new Operation1(this), new Operation1(this), a };
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
