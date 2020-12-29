using System;
using System.Linq;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.Multimetr;
using ASMC.Devices.Interface.Multimetr.Mode;

namespace ASMC.Devices.IEEE.Keysight.Multimeter
{
    public class BaseDigitalMultimetr344xx : IeeeBase, IDigitalMultimetr344xx
    {
        public IMeasureMode<MeasPoint<Resistance>> Resistance2W { get; set; }
        public IMeasureMode<MeasPoint<Resistance>> Resistance4W { get; set; }
        public IMeasureMode<MeasPoint<Voltage>> DcVoltage { get; set; }
        public IMeasureMode<MeasPoint<Voltage, Frequency>> AcVoltage { get; set; }
        public IMeasureMode<MeasPoint<Data.Model.PhysicalQuantity.Current>> DcCurrent { get; set; }
        public IMeasureMode<MeasPoint<Data.Model.PhysicalQuantity.Current, Frequency>> AcCurrent { get; set; }

        public void Getting()
        {
            throw new NotImplementedException();
        }

        public void Setting()
        {
            throw new NotImplementedException();
        }
    }

    public interface IDigitalMultimetr344xx : IMultimetr, IProtocolStringLine
    {
    }

    public abstract class MeasureFunction
    {
        #region Fields

        protected IeeeBase _device;
        protected string ActivateThisModeCommand;
        protected string RangeCommand;

        #endregion

        public MeasureFunction(IeeeBase inDevice)
        {
            _device = inDevice;
        }

        #region Methods

        public decimal GetActiveMeasuredValue()
        {
            _device.WriteLine("SYST:REM;*CLS;*RST;:TRIG:SOUR BUS");
            _device.WriteLine("INIT");
            _device.WriteLine("*TRG");
            var answer = _device.QueryLine("FETCH?");
            var numb = (decimal) HelpDeviceBase.StrToDouble(answer);
            return numb;
        }

        #endregion
    }

    public class AcVoltMeas : MeasureFunction, IMeasureMode<MeasPoint<Voltage, Frequency>>, IAcFilter
    {
        #region Fields

        private readonly Command[] Filters =
        {
            new Command("Det:Band 3", "", 3),
            new Command("Det:Band 20", "", 20),
            new Command("Det:Band 200", "", 200)
        };

        #endregion

        #region Property

        public Command filterSet { get; protected set; }

        #endregion

        public AcVoltMeas(IeeeBase inDevice) : base(inDevice)
        {
            ActivateThisModeCommand = "FUNC \"VOLT: AC\"";
            RangeCommand = "VOLT:AC:RANG";
        }

        public void SetFilter(MeasPoint<Frequency> filterFreq)
        {
            Array.Sort(Filters);
            Array.Reverse(Filters);

            filterSet = Filters.FirstOrDefault(q => q.Value < (double) filterFreq
                                                                      .MainPhysicalQuantity.GetNoramalizeValueToSi());
        }

        public void Getting()
        {
            var answer = _device.QueryLine($"{RangeCommand}?");
            var numb = (decimal) HelpDeviceBase.StrToDouble(answer);
            Range = new MeasPoint<Voltage, Frequency>(numb, 50);
        }

        public void Setting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            _device.WriteLine($"{RangeCommand} {Range.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            _device.WriteLine(filterSet.StrCommand);
        }

        public bool IsEnable { get; }

        public MeasPoint<Voltage, Frequency> Range { get; set; }

        public MeasPoint<Voltage, Frequency> Value()
        {
            var numb = GetActiveMeasuredValue();
            return new MeasPoint<Voltage, Frequency>(numb, 50);
        }


       
    }

    public class AcCurrentMeas : IMeasureMode<MeasPoint<Data.Model.PhysicalQuantity.Current, Frequency>>
    {
        public AcCurrentMeas(IeeeBase inDevice)
        {
        }

        //public MeasPoint<Data.Model.PhysicalQuantity.Current, Frequency> Range
        //{
        //    get
        //    {
        //        string answer = _device.QueryLine("CURR:AC:RANG?");
        //        decimal numb = (decimal)HelpDeviceBase.StrToDouble(answer);
        //        return new MeasPoint<Data.Model.PhysicalQuantity.Current, Frequency>(numb, 50);
        //    }
        //    set
        //    {
        //        _device.WriteLine($"CURR:AC:RANG {value.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
        //    }
        //}

        //public void SetThisFunctionActive()
        //{
        //    _device.WriteLine("FUNC \"CURR: AC\"");

        //}

        //public MeasPoint<Data.Model.PhysicalQuantity.Current, Frequency> GetMeasureValue()
        //{
        //    decimal numb = GetActiveMeasuredValue();
        //    return new MeasPoint<Data.Model.PhysicalQuantity.Current, Frequency>(numb, 50);
        //}
        public void Getting()
        {
            throw new NotImplementedException();
        }

        public void Setting()
        {
            throw new NotImplementedException();
        }

        public bool IsEnable { get; set; }
        public MeasPoint<Data.Model.PhysicalQuantity.Current, Frequency> Range { get; set; }

        public MeasPoint<Data.Model.PhysicalQuantity.Current, Frequency> Value()
        {
            throw new NotImplementedException();
        }
    }

    public class DcVoltMeas : IMeasureMode<MeasPoint<Voltage>>
    {
        //public MeasPoint<Voltage> Range
        //{
        //        get
        //        {
        //            string answer = _device.QueryLine("VOLT:DC:RANG?");
        //            decimal numb = (decimal)HelpDeviceBase.StrToDouble(answer);
        //            return new MeasPoint<Voltage>(numb);
        //        }
        //        set
        //        {
        //            _device.WriteLine($"VOLT:DC:RANG {value.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',','.')}");
        //        }
        //}

        //public void SetThisFunctionActive()
        //{
        //        _device.WriteLine("FUNC \"VOLT: DC\"");
        //}

        //    public MeasPoint<Voltage> GetMeasureValue()
        //    {
        //        decimal numb = GetActiveMeasuredValue();
        //       return new MeasPoint<Voltage>(numb);
        //    }

        public void Getting()
        {
            throw new NotImplementedException();
        }

        public void Setting()
        {
            throw new NotImplementedException();
        }

        public bool IsEnable { get; set; }
        public MeasPoint<Voltage> Range { get; set; }

        public MeasPoint<Voltage> Value()
        {
            throw new NotImplementedException();
        }
    }

    public class DcCurrentMeas : IMeasureMode<MeasPoint<Data.Model.PhysicalQuantity.Current>>
    {
        //public MeasPoint<Data.Model.PhysicalQuantity.Current> Range
        //    {
        //        get
        //        {
        //            string answer = _device.QueryLine("CURR:DC:RANG?");
        //            decimal numb = (decimal)HelpDeviceBase.StrToDouble(answer);
        //            return new MeasPoint<Data.Model.PhysicalQuantity.Current>(numb);
        //        }
        //        set
        //        {
        //            _device.WriteLine($"CURR:DC:RANG {value.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
        //        }
        //    }

        //    public void SetThisFunctionActive()
        //    {
        //        _device.WriteLine("FUNC \"CURR: DC\"");
        //    }

        //    public MeasPoint<Data.Model.PhysicalQuantity.Current> GetMeasureValue()
        //    {
        //        decimal numb = GetActiveMeasuredValue();
        //        return new MeasPoint<Data.Model.PhysicalQuantity.Current>(numb);
        //    }

        public void Getting()
        {
            throw new NotImplementedException();
        }

        public void Setting()
        {
            throw new NotImplementedException();
        }

        public bool IsEnable { get; set; }
        public MeasPoint<Data.Model.PhysicalQuantity.Current> Range { get; set; }

        public MeasPoint<Data.Model.PhysicalQuantity.Current> Value()
        {
            throw new NotImplementedException();
        }
    }

    public class Resistance2W : MeasureFunction, IMeasureMode<MeasPoint<Resistance>>
    {
        public Resistance2W(IeeeBase inDevice) : base(inDevice)
        {
        }
        //public MeasPoint<Resistance> Range
        //  {
        //      get
        //      {
        //          string answer = _device.QueryLine("RES:RANG?");
        //          decimal numb = (decimal)(HelpDeviceBase.StrToDouble(answer));
        //          return new MeasPoint<Resistance>(numb);
        //      }
        //      set
        //      {
        //          _device.WriteLine($"RES:RANG {value.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')};nplc 100");
        //      }
        //  }
        //  public void SetThisFunctionActive()
        //  {
        //      _device.WriteLine("FUNC \"RES\"");
        //  }

        //  public MeasPoint<Resistance> GetMeasureValue()
        //  {
        //      decimal numb = GetActiveMeasuredValue();
        //      return new MeasPoint<Resistance>(numb);
        //  }
        public void Getting()
        {
            throw new NotImplementedException();
        }

        public void Setting()
        {
            throw new NotImplementedException();
        }

        public bool IsEnable { get; set; }
        public MeasPoint<Resistance> Range { get; set; }

        public MeasPoint<Resistance> Value()
        {
            throw new NotImplementedException();
        }
    }

    public class Resistance4W : IMeasureMode<MeasPoint<Resistance>>
    {
        //public MeasPoint<Resistance> Range
        //{
        //    get
        //    {
        //        string answer = _device.QueryLine("FRES:RANG?");
        //        decimal numb = (decimal)(HelpDeviceBase.StrToDouble(answer));
        //        return new MeasPoint<Resistance>(numb);
        //    }
        //    set
        //    {
        //        _device.WriteLine($"FRES:RANG {value.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')};nplc 100");
        //    }
        //}
        //public void SetThisFunctionActive()
        //{
        //    _device.WriteLine(" FUNC \"FRES\"");
        //}

        //public MeasPoint<Resistance> GetMeasureValue()
        //{
        //    decimal numb = GetActiveMeasuredValue();
        //    return new MeasPoint<Resistance>(numb);
        //}

        public void Getting()
        {
            throw new NotImplementedException();
        }

        public void Setting()
        {
            throw new NotImplementedException();
        }

        public bool IsEnable { get; set; }
        public MeasPoint<Resistance> Range { get; set; }

        public MeasPoint<Resistance> Value()
        {
            throw new NotImplementedException();
        }
    }
}