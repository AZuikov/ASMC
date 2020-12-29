using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using DevExpress.Mvvm.Native;

namespace ASMC.Devices.IEEE.Keysight.Multimeter
{
   public class BaseDigitalMultimetr344xx: IeeeBase, IDigitalMultimetr344xx
    {
        

        public BaseDigitalMultimetr344xx()
        {
            Resistance = new w4();
        }


        public IDigitalMultimetrModeResistance Resistance { get; set; }
        public IDigitalMultimetrMeasureMode<MeasPoint<Voltage, Frequency>> AcVoltage { get; set; }
    }

   public interface  IDigitalMultimetr344xx : , IProtocolStringLine
   {

   }

   public class w4: IDigitalMultimetrModeResistance4W, IDigitalMultimetrModeResistance
    {
        public IDigitalMultimetrMeasureMode<MeasPoint<Resistance>> Resistance4W { get; set; }
        public IDigitalMultimetrMeasureMode<MeasPoint<Resistance>> Resistance2W { get; set; }
    }
    public abstract class MeasureFunction
    {
        protected IeeeBase _device;

        public MeasureFunction(IeeeBase inDevice)
        {
            IDigitalMultimetr344xx dasd;
            dasd.Resistance;
                 if(dasd.Resistance is IDigitalMultimetrModeResistance4W s)
                 {
                     s.Resistance4W.GetMeasureValue();
                     
                 }
                 else
                 {
                     dasd.Resistance.Resistance2W.GetMeasureValue();
                 }
            _device = inDevice;
        }

        public decimal GetActiveMeasuredValue()
        {
            _device.WriteLine("SYST:REM;*CLS;*RST;:TRIG:SOUR BUS");
            _device.WriteLine("INIT");
            _device.WriteLine("*TRG");
            string answer = _device.QueryLine("FETCH?");
            decimal numb = (decimal)(HelpDeviceBase.StrToDouble(answer));
            return numb;
        }
    }

    public class AcMeasureGroup: IDigitalMultimetrModeAc
    {
       

        public AcMeasureGroup(IeeeBase inDevice)
        {
            
            
            AcVoltage = new AcVoltMeas(inDevice);
            AcCurrent = new AcCurrentMeas(inDevice);
        }

        public class AcVoltMeas : MeasureFunction, IDigitalMultimetrMeasureMode<MeasPoint<Voltage, Frequency>>
        {
            public IDigitalMultimetrMeasureMode<MeasPoint<Voltage, Frequency>> AcVoltage { get; set; }

            public AcVoltMeas(IeeeBase inDevice) : base(inDevice)
            {
            }
            
            public MeasPoint<Voltage, Frequency> Range
            {
                get
                {
                    string answer = _device.QueryLine("VOLT:AC:RANG?");
                    decimal numb = (decimal)HelpDeviceBase.StrToDouble(answer);
                    return new MeasPoint<Voltage, Frequency>(numb, 50);
                }
                set
                {
                    _device.WriteLine($"VOLT:AC:RANG {value.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')};:DET:BAND 3");
                }
            }
            public void SetThisFunctionActive()
            {
                _device.WriteLine("FUNC \"VOLT: AC\"");
            }

            public MeasPoint<Voltage, Frequency> GetMeasureValue()
            {
                decimal numb = GetActiveMeasuredValue();
                return new MeasPoint<Voltage, Frequency>(numb, 50);
            }
        }

        public class AcCurrentMeas : MeasureFunction, IDigitalMultimetrMeasureMode<MeasPoint<Data.Model.PhysicalQuantity.Current, Frequency>>
        {
           public AcCurrentMeas(IeeeBase inDevice) : base(inDevice)
           {
                
           }
            
            public MeasPoint<Data.Model.PhysicalQuantity.Current, Frequency> Range
            {
                get
                {
                    string answer = _device.QueryLine("CURR:AC:RANG?");
                    decimal numb = (decimal)HelpDeviceBase.StrToDouble(answer);
                    return new MeasPoint<Data.Model.PhysicalQuantity.Current, Frequency>(numb, 50);
                }
                set
                {
                    _device.WriteLine($"CURR:AC:RANG {value.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
                }
            }



            public void SetThisFunctionActive()
            {
                _device.WriteLine("FUNC \"CURR: AC\"");

            }

            public MeasPoint<Data.Model.PhysicalQuantity.Current, Frequency> GetMeasureValue()
            {
                decimal numb = GetActiveMeasuredValue();
                return new MeasPoint<Data.Model.PhysicalQuantity.Current, Frequency>(numb, 50);
            }
        }

        public IDigitalMultimetrMeasureMode<MeasPoint<Voltage, Frequency>> AcVoltage { get; set; }
        public IDigitalMultimetrMeasureMode<MeasPoint<Data.Model.PhysicalQuantity.Current, Frequency>> AcCurrent
        {
            get;
            set;
        }
    }
    public class DcMeasureGroup: IDigitalMultimetrGroupMode
    {
        public object[] mode { get; set; }

        public DcMeasureGroup(IeeeBase inDevice)
        {
            mode = new object[]{new DcVoltMeas(inDevice), new DcCurrentMeas(inDevice) };
        }

        public class DcVoltMeas : MeasureFunction, IDigitalMultimetrModeDcVoltage
        {
            public IDigitalMultimetrMeasureMode<MeasPoint<Voltage>> DcVoltage { get; set; }
            

            public DcVoltMeas(IeeeBase inDevice) : base(inDevice)
            {
                DcVoltage = new DcVoltageMeasureModeMethods(inDevice);
            }

            class DcVoltageMeasureModeMethods :MeasureFunction, IDigitalMultimetrMeasureMode<MeasPoint<Voltage>>
            {
                public DcVoltageMeasureModeMethods(IeeeBase inDevice) : base(inDevice)
                {
                }

                public MeasPoint<Voltage> Range {
                    get
                    {
                        string answer = _device.QueryLine("VOLT:DC:RANG?");
                        decimal numb = (decimal)HelpDeviceBase.StrToDouble(answer);
                        return new MeasPoint<Voltage>(numb);
                    }
                    set
                    {
                        _device.WriteLine($"VOLT:DC:RANG {value.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',','.')}");
                    }
                }
                private IeeeBase _device;

                

                public void SetThisFunctionActive()
                {
                    _device.WriteLine("FUNC \"VOLT: DC\"");
                }

                public MeasPoint<Voltage> GetMeasureValue()
                {

                    decimal numb = GetActiveMeasuredValue();
                   return new MeasPoint<Voltage>(numb);
                }
            }
        }

        public class DcCurrentMeas : MeasureFunction, IDigitalMultimetrModeDcCurrent
        {
            public IDigitalMultimetrMeasureMode<MeasPoint<Data.Model.PhysicalQuantity.Current>> DcCurrent { get; set; }

            public DcCurrentMeas(IeeeBase inDevice) : base(inDevice)
            {
                DcCurrent = new DcCurrentMeasureModeMethods(inDevice);
            }

            class DcCurrentMeasureModeMethods : MeasureFunction, IDigitalMultimetrMeasureMode<MeasPoint<Data.Model.PhysicalQuantity.Current>>
            {
                private IeeeBase _device;
                public DcCurrentMeasureModeMethods(IeeeBase inDevice) : base(inDevice)
                {
                    _device = inDevice;
                }

                public MeasPoint<Data.Model.PhysicalQuantity.Current> Range
                {
                    get
                    {
                        string answer = _device.QueryLine("CURR:DC:RANG?");
                        decimal numb = (decimal)HelpDeviceBase.StrToDouble(answer);
                        return new MeasPoint<Data.Model.PhysicalQuantity.Current>(numb);
                    }
                    set
                    {
                        _device.WriteLine($"CURR:DC:RANG {value.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
                    }
                }

                public void SetThisFunctionActive()
                {
                    _device.WriteLine("FUNC \"CURR: DC\"");
                }

                public MeasPoint<Data.Model.PhysicalQuantity.Current> GetMeasureValue()
                {
                   
                    decimal numb = GetActiveMeasuredValue();
                    return new MeasPoint<Data.Model.PhysicalQuantity.Current>(numb);
                }
            }
        }

        
    }

    public class ResistanceGroup : IDigitalMultimetrModeResistance
    {
       

        public IDigitalMultimetrMeasureMode<MeasPoint<Resistance>> Resistance2W { get; set; }
        public IDigitalMultimetrMeasureMode<MeasPoint<Resistance>> Resistance4W { get; set; }

        public ResistanceGroup(IeeeBase inDevice)
        {
           Resistance2W = new Resist2W(inDevice);
        }

        class Resist2W:MeasureFunction, IDigitalMultimetrMeasureMode<MeasPoint<Resistance>>
        {
            public Resist2W(IeeeBase inDevice):base(inDevice)
            {
                
            }
            
          public MeasPoint<Resistance> Range
            {
                get
                {
                    string answer = _device.QueryLine("RES:RANG?");
                    decimal numb = (decimal)(HelpDeviceBase.StrToDouble(answer));
                    return new MeasPoint<Resistance>(numb);
                }
                set
                {
                    _device.WriteLine($"RES:RANG {value.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')};nplc 100");
                }
            }
            public void SetThisFunctionActive()
            {
                _device.WriteLine("FUNC \"RES\"");
            }

            public MeasPoint<Resistance> GetMeasureValue()
            {
                decimal numb = GetActiveMeasuredValue();
                return new MeasPoint<Resistance>(numb);
            }
        }

        class Resist4W: MeasureFunction, IDigitalMultimetrMeasureMode<MeasPoint<Resistance>>
        {
            public Resist4W(IeeeBase inDevice):base(inDevice)
            {
                
            }

            public MeasPoint<Resistance> Range
            {
                get
                {
                    string answer = _device.QueryLine("FRES:RANG?");
                    decimal numb = (decimal)(HelpDeviceBase.StrToDouble(answer));
                    return new MeasPoint<Resistance>(numb);
                }
                set
                {
                    _device.WriteLine($"FRES:RANG {value.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')};nplc 100");
                }
            }
            public void SetThisFunctionActive()
            {
                _device.WriteLine(" FUNC \"FRES\"");
            }

            public MeasPoint<Resistance> GetMeasureValue()
            {
                decimal numb = GetActiveMeasuredValue();
                return new MeasPoint<Resistance>(numb);
            }


        }

       
    }
}
