using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AP.Extension;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Fluke.Calibrator;

namespace Belvar_V7_40_1
{

    public class OperationPrimary<TTestDevices> : Operation where TTestDevices : IUserType, new()
    {
        public OperationPrimary(string documentName, ServicePack servicePac) : base(servicePac)
        {
            ControlDevices = new IDeviceUi[]
            {
                new Device {Devices = new ICalibratorMultimeterFlukeBase[] {/*new Calibr_9100(), new Calib5522A(),*/ new Calib_5720A() }},

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
                //new VisualInspection(this),
                //new Testing(this),
                new DcvTest(this), 
                new AcvTest(this), 
                new DciTest(this), 
                /*Остальная часть методики*/
            };
            DocumentName = documentName;
            Accessories = new string[]
            {
                /*Указать перечень*/
            };
             string path = Directory.GetCurrentDirectory() + "\\Plugins\\" + Assembly.GetExecutingAssembly().GetName().Name + "\\Resources\\pointsV7-40_1.asmc";
             //todo пользователь должен иметь возможность выбрать файл с поверяемыми точками
            
            OperationExtension.FillTestPoint(this, path);
            
        }


        /// <inheritdoc />
        public override async void RefreshDevice()
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
