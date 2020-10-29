using System;
using System.Reflection;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE.Fluke.CalibtatorOscilloscope;
using ASMC.Devices.IEEE.Tektronix.Oscilloscope;
using ASMC.Devices.IEEE.Tektronix.Oscilloscope.TDS_2022B;
using TDS_BasePlugin;

namespace TDS1012B
{
    public class TDS1012bPlugin : TDS_BasePlugin<Operation>
    {
        public TDS1012bPlugin(ServicePack servicePack) : base(servicePack)
        {
            Type = "TDS 1012B";
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
                {new Device {Devices = new IDeviceBase[] {new TDS_2022B()}, Description = "Цифровой осциллограф."}};

            UserItemOperation = new IUserItemOperationBase[]
            {
                new Oper1VisualTest(this),
                new Oper2Oprobovanie(this),
                new Oper3KoefOtkl(this, TDS_Oscilloscope.ChanelSet.CH1),
                new Oper4MeasureTimeIntervals(this, TDS_Oscilloscope.ChanelSet.CH1,
                                              Assembly.GetExecutingAssembly().GetName().Name),
                new Oper5MeasureRiseTime(this, TDS_Oscilloscope.ChanelSet.CH1),
                new Oper3KoefOtkl(this, TDS_Oscilloscope.ChanelSet.CH2),
                new Oper4MeasureTimeIntervals(this, TDS_Oscilloscope.ChanelSet.CH2,
                                              Assembly.GetExecutingAssembly().GetName().Name),
                new Oper5MeasureRiseTime(this, TDS_Oscilloscope.ChanelSet.CH2)
            };
        }

        #region Methods

        public override void FindDevice()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class Oper3KoefOtkl : TDS_BasePlugin.Oper3KoefOtkl
    {
        public Oper3KoefOtkl(IUserItemOperation userItemOperation, TDS_Oscilloscope.ChanelSet inTestingChanel) :
            base(userItemOperation, inTestingChanel, Assembly.GetExecutingAssembly().GetName().Name)
        {
            calibr9500B = new Calibr9500B();
            someTdsOscilloscope = new TDS_2022B();

            
        }
    }

    public class Oper4MeasureTimeIntervals : TDS10XXBOper4MeasureTimeIntervals
    {
        public Oper4MeasureTimeIntervals(IUserItemOperation userItemOperation,
            TDS_Oscilloscope.ChanelSet chanel, string inResourceDi) : base(userItemOperation, chanel, inResourceDi)
        {
            calibr9500B = new Calibr9500B();
            someTdsOscilloscope = new TDS_2022B();
        }
    }

    public class Oper5MeasureRiseTime : TDS_BasePlugin.Oper5MeasureRiseTime
    {
        public Oper5MeasureRiseTime(IUserItemOperation userItemOperation, TDS_Oscilloscope.ChanelSet chanel) :
            base(userItemOperation, chanel, Assembly.GetExecutingAssembly().GetName().Name)
        {
            calibr9500B = new Calibr9500B();
            someTdsOscilloscope = new TDS_2022B();
            horizontalScAleForTest = TDS_Oscilloscope.HorizontalSCAle.Scal_5nSec;
            RiseTimeTol = new MeasPoint<Time>(3.5M, UnitMultiplier.Nano);
        }
    }
}