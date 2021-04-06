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
                new Device {Devices = new ICalibratorMultimeterFlukeBase[] {/*new Calibr_9100(), new Calib_5522A(),*/ new Calib_5720A() }},

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
                new VisualTest(this),
                new Oprobovanie(this),
                new IsolationTest1(this), 
                new IsolationTest2(this), 
                new DcvTest(this), 
                new AcvTest(this), 
                new Resist2WTest(this), 
                new DciTest(this), 
                new AciTest(this),
                new DCV_DNV_ZIP(this),
                new DCV_DNV_K2_ZIP(this), 
                new DCV_DNV_K3_ZIP(this), 
                new Zip_AcvTest_DN(this), 
                new Zip_AcvTest_HighFreqProbe(this),
                new ZIP_DCI_10A(this), 



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
