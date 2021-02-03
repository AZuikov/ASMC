using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Fluke.Calibrator;
using ASMC.Devices.Interface;

namespace Multimetr34401A
{

    public class OperationPrimary<TTestDevices> : Operation where TTestDevices : IUserType, new()
    {
        public OperationPrimary(string documentName, ServicePack servicePac) : base(servicePac)
        {
            ControlDevices = new IDeviceUi[]
            {
                new Device {Devices = new ICalibrator[] { new Calib5522A() }},
            };
            TestDevices = new IDeviceUi[]
            {
                new Device
                {
                    Devices = new IUserType[] {new TTestDevices()},
                    Description = $@"{new TTestDevices().UserType}"
                }
            };
            UserItemOperation = new IUserItemOperationBase[]
            {
                new VisualInspection(this),
                new Testing(this),
                /*Остальная часть методики*/
            };
            DocumentName = documentName;
            Accessories = new[]
            {
                "Калибратор многофункциональный Fluke 5522A"
            };
        }


        /// <inheritdoc />
        public override void RefreshDevice()
        {
            AddresDevice = IeeeBase.AllStringConnect;
        }
        /// <inheritdoc />
        public override void FindDevice()
        {
            throw new NotImplementedException();
        }
    }
}
