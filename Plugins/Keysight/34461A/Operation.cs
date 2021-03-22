using System;
using System.IO;
using System.Reflection;
using AP.Extension;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Fluke.Calibrator;

namespace ProgramFor34461A
{
    public class OperationPrimary<TTestDevices> : Operation where TTestDevices : IUserType, new()
    {
        public OperationPrimary(string documentName, ServicePack servicePac) : base(servicePac)
        {
            ControlDevices = new IDeviceUi[]
            {
                new Device {Devices = new IUserType[] {new Calib_5522A(), new Calib_5720A()}}
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
                new Testing(this)
                /*Остальная часть методики*/
            };
            DocumentName = documentName;
            Accessories = new string[]
            {
                /*Указать перечень*/
            };

            var path = Directory.GetCurrentDirectory() + "\\Plugins\\" +
                       Assembly.GetExecutingAssembly().GetName().Name + "\\Resources\\pointsV7-40_1.asmc";
            //todo пользователь должен иметь возможность выбрать файл с поверяемыми точками

            this.FillTestPoint(path);
        }

        #region Methods

        /// <inheritdoc />
        public override void FindDevice()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override async void RefreshDevice()
        {
            AddresDevice = IeeeBase.AllStringConnect;
        }

        #endregion
    }
}