using System;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Devices.IEEE.Tektronix.Oscilloscope.TDS_2022B;
using TDS_BasePlugin;


namespace TDS2022B
{
    public class TDS2022BPlugin : TDS_BasePlugin.TDS_BasePlugin<Operation>
    {
        public TDS2022BPlugin(ServicePack servicePack) : base(servicePack)
        {
            Type = "TDS 2022B";
            Range = "no range";
            Accuracy = "no accuracy";

        }
    }

    public class Operation : OperationMetrControlBase
    {
        public Operation(ServicePack servicePack)
        {

            UserItemOperationPrimaryVerf = new OpertionFirsVerf(servicePack);
            //здесь периодическая поверка, но набор операций такой же
            UserItemOperationPeriodicVerf = UserItemOperationPrimaryVerf;
        }
    }

    public class OpertionFirsVerf : TDS_BasePlugin.OpertionFirsVerf
    {
        public OpertionFirsVerf(ServicePack servicePack) : base(servicePack)
        {
            
            TestDevices = new IDeviceUi[]
                {new Device { Devices = new IDeviceBase[] { new TDS_2022B()}, Description = "Цифровой портативный мультиметр"}};

            UserItemOperation = new IUserItemOperationBase[]
            {
                new Oper1VisualTest(this),
                new Oper2Oprobovanie(this)
            };
        }

        public override void FindDevice()
        {
            throw new NotImplementedException();
        }
    }

}
