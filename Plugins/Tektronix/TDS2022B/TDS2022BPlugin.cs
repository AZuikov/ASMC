using System;
using System.Data;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Devices.IEEE.Fluke.CalibtatorOscilloscope;
using ASMC.Devices.IEEE.Tektronix.Oscilloscope;
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
                {new Device { Devices = new IDeviceBase[] { new TDS_2022B()}, Description = "Цифровой осциллограф."}};

            UserItemOperation = new IUserItemOperationBase[]
            {
                new Oper1VisualTest(this),
                new Oper2Oprobovanie(this),
                new Oper3KoefOtkl(this, TDS_Oscilloscope.ChanelSet.CH1),
                new Oper4MeasureTimeIntervals(this, TDS_Oscilloscope.ChanelSet.CH1)
            };
        }

        public override void FindDevice()
        {
            throw new NotImplementedException();
        }
    }

    public class Oper3KoefOtkl : TDS_BasePlugin.Oper3KoefOtkl
    {
        public Oper3KoefOtkl(IUserItemOperation userItemOperation, TDS_Oscilloscope.ChanelSet inTestingChanel) : base(userItemOperation, inTestingChanel)
        {
            calibr9500B  = new Calibr9500B();
            someTdsOscilloscope = new TDS_2022B();
        }
    }

    public class Oper4MeasureTimeIntervals : TDS20XXBOper4MeasureTimeIntervals
    {
        public Oper4MeasureTimeIntervals(IUserItemOperation userItemOperation, TDS_Oscilloscope.ChanelSet inTestingChanel) : base(userItemOperation, inTestingChanel)
        {
            calibr9500B = new Calibr9500B();
            someTdsOscilloscope = new TDS_2022B();
            
        }
    }

}
