using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Devices.IEEE.Fluke.CalibtatorOscilloscope;
using ASMC.Devices.IEEE.Tektronix.Oscilloscope;
using ASMC.Devices.IEEE.Tektronix.Oscilloscope.TDS_2022B;
using TDS_BasePlugin;


namespace TDS2014B
{
    public class TDS2014BPlugin : TDS_BasePlugin<Operation>
    {
        public TDS2014BPlugin(ServicePack servicePack) : base(servicePack)
        {
            Type = "TDS 2014B";
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
                new Oper5MeasureRiseTime(this, TDS_Oscilloscope.ChanelSet.CH2),
                new Oper3KoefOtkl(this, TDS_Oscilloscope.ChanelSet.CH3),
                new Oper4MeasureTimeIntervals(this, TDS_Oscilloscope.ChanelSet.CH3,
                                              Assembly.GetExecutingAssembly().GetName().Name),
                new Oper5MeasureRiseTime(this, TDS_Oscilloscope.ChanelSet.CH3),
                new Oper3KoefOtkl(this, TDS_Oscilloscope.ChanelSet.CH4),
                new Oper4MeasureTimeIntervals(this, TDS_Oscilloscope.ChanelSet.CH4,
                                              Assembly.GetExecutingAssembly().GetName().Name),
                new Oper5MeasureRiseTime(this, TDS_Oscilloscope.ChanelSet.CH4)
            };
        }

        public override void FindDevice()
        {
            throw new NotImplementedException();
        }
    }


    public class Oper3KoefOtkl : TDS_BasePlugin.Oper3KoefOtkl
    {
        public Oper3KoefOtkl(IUserItemOperation userItemOperation, TDS_Oscilloscope.ChanelSet inTestingChanel) :
            base(userItemOperation, inTestingChanel, Assembly.GetExecutingAssembly().GetName().Name)
        {
            calibr9500B = new Calibr9500B();
            someTdsOscilloscope = new TDS_2022B();

            verticalScalesList.Add(TDS_Oscilloscope.VerticalScale.Scale_2mV);
            verticalScalesList.Add(TDS_Oscilloscope.VerticalScale.Scale_5mV);
            verticalScalesList.Add(TDS_Oscilloscope.VerticalScale.Scale_10mV);
            verticalScalesList.Add(TDS_Oscilloscope.VerticalScale.Scale_20mV);
            verticalScalesList.Add(TDS_Oscilloscope.VerticalScale.Scale_50mV);
            verticalScalesList.Add(TDS_Oscilloscope.VerticalScale.Scale_100mV);
            verticalScalesList.Add(TDS_Oscilloscope.VerticalScale.Scale_200mV);
            verticalScalesList.Add(TDS_Oscilloscope.VerticalScale.Scale_500mV);
            verticalScalesList.Add(TDS_Oscilloscope.VerticalScale.Scale_1V);
            verticalScalesList.Add(TDS_Oscilloscope.VerticalScale.Scale_2V);
            verticalScalesList.Add(TDS_Oscilloscope.VerticalScale.Scale_5V);
        }
    }

    public class Oper4MeasureTimeIntervals : TDS20XXBOper4MeasureTimeIntervals
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
            horizontalScAleForTest = TDS_Oscilloscope.HorizontalSCAle.Scal_2_5nSec;
            RiseTimeTol = new MeasPoint(MeasureUnits.Time, UnitMultiplier.Nano, (decimal)3.5M);
        }
    }
}
