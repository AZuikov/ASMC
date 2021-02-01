using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Devices.USB_Device.SKBIS.Lir917;
using ASMC.Devices.USB_Device.WebCam;

namespace DialIndicatorSeries801
{

    public class OperationPrimary<TTestDevices> : Operation where TTestDevices : IUserType, new()
    {
        public OperationPrimary(string documentName, ServicePack servicePac) : base(servicePac)
        {
            ControlDevices = new IDeviceUi[]
            {
                new Device {Devices = new IUserType[] {new Ppi()}},
                new Device {Devices = new IUserType[] {new WebCam()}}
            };
            TestDevices = new Device
                {
                    Devices = new IUserType[] {new TTestDevices()}, IsCanStringConnect = false,
                    Description = $@"{new TTestDevices().UserType}"
                };
            UserItemOperation = new IUserItemOperationBase[]
            {
                new VisualInspection(this),
                new Testing(this),
                /*Остальная часть методики*/
            };
            DocumentName = documentName;
            Accessories = new string[]
            {
                /*Указать перечень*/
            };
        }


        /// <inheritdoc />
        public override async void RefreshDevice()
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc />
        public override void FindDevice()
        {
            throw new NotImplementedException();
        }
    }
}
