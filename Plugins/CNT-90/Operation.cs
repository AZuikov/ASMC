using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Data.Model;

namespace CNT_90
{

    public class OperationPrimary<TTestDevices> : Operation where TTestDevices : IUserType, new()
    {
        public OperationPrimary(string documentName, ServicePack servicePac) : base(servicePac)
        {
            ControlDevices = new IDeviceUi[]
            {
                new Device {Devices = new IUserType[] {/*Указать список 1 устройст с помошью которых происзводится поверка*/}},
                new Device {Devices = new IUserType[] {/*Указать список 2 устройст с помошью которых происзводится поверка*/}}
            };
            TestDevices = new IDeviceUi[]
            {
                new Device
                {
                    Devices = new IUserType[] {new TTestDevices()}, IsCanStringConnect = false,
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
