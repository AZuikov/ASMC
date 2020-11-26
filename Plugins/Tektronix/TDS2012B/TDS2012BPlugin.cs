using System;
using System.Reflection;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE.Fluke.CalibtatorOscilloscope;
using ASMC.Devices.IEEE.Tektronix.Oscilloscope;
using TDS_BasePlugin;

namespace TDS2012B
{
    public class TDS2012BPlugin : TDS_BasePlugin<Operation>
    {
        public TDS2012BPlugin(ServicePack servicePack) : base(servicePack)
        {
            Type = "TDS 2012B";
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
                {new Device {Devices = new IDeviceBase[] {new TDS_Oscilloscope(){UserType = "TDS2012B"}}, Description = "Цифровой осциллограф."}};

            UserItemOperation = new IUserItemOperationBase[]
            {
                new Oper1VisualTest(this),
                new Oper2Oprobovanie(this),
                new Oper3KoefOtkl(this, TDS_Oscilloscope.ChanelSet.CH1),
                new Oper4MeasureTimeIntervals(this, TDS_Oscilloscope.ChanelSet.CH1,
                                              Assembly.GetExecutingAssembly().GetName().Name),
                new Oper5MeasureRiseTime(this, TDS_Oscilloscope.ChanelSet.CH1),
                new Oper3KoefOtkl(this, TDS_Oscilloscope.ChanelSet.CH2),
                
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
            someTdsOscilloscope = new TDS_Oscilloscope();
        }
    }

    public class Oper4MeasureTimeIntervals : TDS20XXBOper4MeasureTimeIntervals
    {
        public Oper4MeasureTimeIntervals(IUserItemOperation userItemOperation,
            TDS_Oscilloscope.ChanelSet oscillosocopeChanel, string inResourceDi) : base(userItemOperation, oscillosocopeChanel, inResourceDi)
        {
            calibr9500B = new Calibr9500B();
            someTdsOscilloscope = new TDS_Oscilloscope();
        }
    }

    public class Oper5MeasureRiseTime : TDS_BasePlugin.Oper5MeasureRiseTime
    {
        public Oper5MeasureRiseTime(IUserItemOperation userItemOperation, TDS_Oscilloscope.ChanelSet chanel) :
            base(userItemOperation, chanel, Assembly.GetExecutingAssembly().GetName().Name)
        {
            calibr9500B = new Calibr9500B();
            someTdsOscilloscope = new TDS_Oscilloscope();
            horizontalScAleForTest = TDS_Oscilloscope.HorizontalSCAle.Scal_2_5nSec;
            RiseTimeTol = new MeasPoint<Time>(3.5M, UnitMultiplier.Nano);
        }
    }
}