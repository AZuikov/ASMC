using System;
using System.Collections.Generic;
using System.Data;
using AP.Utils.Helps;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE.Fluke.CalibtatorOscilloscope;
using ASMC.Devices.IEEE.Tektronix.Oscilloscope;
using ASMC.Devices.IEEE.Tektronix.Oscilloscope.TDS_2022B;
using TDS_BasePlugin;

namespace TDS2022B
{
    public class TDS2022BPlugin : TDS_BasePlugin<Operation>
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
                {new Device {Devices = new IDeviceBase[] {new TDS_2022B()}, Description = "Цифровой осциллограф."}};

            var Cnanel1 = new ChanelOsciloscope(this,"1");
            Cnanel1.Nodes.Add(new Oper3KoefOtkl(this, TDS_Oscilloscope.ChanelSet.CH1));
            Cnanel1.Nodes.Add(new Oper4MeasureTimeIntervals(this, TDS_Oscilloscope.ChanelSet.CH1));
            Cnanel1.Nodes.Add(new Oper5MeasureRiseTime(this, TDS_Oscilloscope.ChanelSet.CH1));

            var Cnanel2 = new ChanelOsciloscope(this,"2");
            Cnanel2.Nodes.Add(new Oper3KoefOtkl(this, TDS_Oscilloscope.ChanelSet.CH2));
            Cnanel2.Nodes.Add(new Oper4MeasureTimeIntervals(this, TDS_Oscilloscope.ChanelSet.CH2));
            Cnanel2.Nodes.Add(new Oper5MeasureRiseTime(this, TDS_Oscilloscope.ChanelSet.CH2));

            UserItemOperation = new IUserItemOperationBase[]
            {
                new Oper1VisualTest(this),
                new Oper2Oprobovanie(this),
                Cnanel1,
                Cnanel2

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
            base(userItemOperation, inTestingChanel)
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
            TDS_Oscilloscope.ChanelSet chanel) : base(userItemOperation, chanel)
        {
            calibr9500B = new Calibr9500B();
            someTdsOscilloscope = new TDS_2022B();
        }
    }

    public class Oper5MeasureRiseTime : TDS_BasePlugin.Oper5MeasureRiseTime
    {
        public Oper5MeasureRiseTime(IUserItemOperation userItemOperation, TDS_Oscilloscope.ChanelSet chanel) :
            base(userItemOperation, chanel)
        {
            calibr9500B = new Calibr9500B();
            someTdsOscilloscope = new TDS_2022B();
            horizontalScAleForTest = TDS_Oscilloscope.HorizontalSCAle.Scal_2_5nSec;
            RiseTimeTol = new MeasPoint(MeasureUnits.sec, UnitMultipliers.Nano, (decimal) 2.1);

            

        }
    }

    public class ChanelOsciloscope : ParagraphBase, IUserItemOperation<MeasPoint>
    {
        public ChanelOsciloscope(IUserItemOperation userItemOperation, string ChanelNameStr) : base(userItemOperation)
        {
            Name = $"Канал {ChanelNameStr}";
        }


        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        protected override void InitWork()
        {
            throw new NotImplementedException();
        }

        public List<IBasicOperation<MeasPoint>> DataRow { get; set; }
    }
}