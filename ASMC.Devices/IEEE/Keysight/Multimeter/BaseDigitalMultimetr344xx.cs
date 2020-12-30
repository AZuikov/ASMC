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
        public BaseDigitalMultimetr344xx()
        {
            AcVoltage = new AcVoltMeas(this);
            DcVoltage = new DcVoltMeas(this);
            DcCurrent = new DcCurrentMeas(this);
            AcCurrent = new AcCurrentMeas(this);
            Resistance4W = new Resistance4W(this);
            Resistance2W = new Resistance2W(this);
        }

        public IMeasureMode<MeasPoint<Resistance>> Resistance2W { get; set; }
        public IMeasureMode<MeasPoint<Resistance>> Resistance4W { get; set; }
        public IMeasureMode<MeasPoint<Voltage>> DcVoltage { get; set; }
        public IMeasureMode<MeasPoint<Voltage>> AcVoltage { get; set; }
        public IMeasureMode<MeasPoint<Data.Model.PhysicalQuantity.Current>> DcCurrent { get; set; }
        public IMeasureMode<MeasPoint<Data.Model.PhysicalQuantity.Current>> AcCurrent { get; set; }

        public void Getting()
        {
            AcVoltage.Getting();
            DcVoltage.Getting();
            DcCurrent.Getting();
            AcCurrent.Getting();
            Resistance4W.Getting();
            Resistance2W.Getting();
        }

        public void Setting()
        {
            AcVoltage.Setting();
            DcVoltage.Setting();
            DcCurrent.Setting();
            AcCurrent.Setting();
            Resistance4W.Setting();
            Resistance2W.Setting();
        }
    }

    public interface IDigitalMultimetr344xx : IMultimetr, IProtocolStringLine
    {
    }

    public abstract class MeasureFunction344xxBase
    {
        #region Fields

        protected IeeeBase _device;
        protected string ActivateThisModeCommand;
        protected string functionName;
        protected string RangeCommand;

        #endregion

        public MeasureFunction344xxBase(IeeeBase inDevice, string inFunctionName)
        {
            _device = inDevice;
            functionName = inFunctionName;
            ActivateThisModeCommand = $"FUNC \"{functionName}\"";
            RangeCommand = $"{functionName}:RANG";
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

    public class AcVoltMeas : MeasureFunction344xxBase, IMeasureMode<MeasPoint<Voltage>>, IAcFilter
    {
        #region Fields

        private readonly Command[] Filters =
        {
            new Command("Det:Band 3", "", 3),
            new Command("Det:Band 20", "", 20),
            new Command("Det:Band 200", "", 200)
        };

        private MeasPoint<Voltage> _value;

        #endregion

        #region Property

        public Command filterSet { get; protected set; }

        #endregion

        public AcVoltMeas(IeeeBase inDevice) : base(inDevice, "VOLT:AC")
        {
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
            _device.WriteLine($"{ActivateThisModeCommand}");
            var answer = _device.QueryLine($"{RangeCommand}?");
            var numb = (decimal) HelpDeviceBase.StrToDouble(answer);
            Range = new MeasPoint<Voltage>(numb);
            var val = GetActiveMeasuredValue();
            _value = new MeasPoint<Voltage>(val);
        }

        public void Setting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            _device.WriteLine($"{RangeCommand} {Range.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            _device.WriteLine(filterSet.StrCommand);
        }

        public bool IsEnable { get; }

        public MeasPoint<Voltage> Range { get; set; }

        MeasPoint<Voltage> IMeasureMode<MeasPoint<Voltage>>.Value
        {
            get
            {
                Getting();
                return _value;
            }
        }
    }

    public class AcCurrentMeas : MeasureFunction344xxBase, IMeasureMode<MeasPoint<Data.Model.PhysicalQuantity.Current>>,
                                 IAcFilter
    {
        #region Fields

        private readonly Command[] Filters =
        {
            new Command("Det:Band 3", "", 3),
            new Command("Det:Band 20", "", 20),
            new Command("Det:Band 200", "", 200)
        };

        private MeasPoint<Data.Model.PhysicalQuantity.Current> _range;

        private MeasPoint<Data.Model.PhysicalQuantity.Current> _value;

        #endregion

        #region Property

        public Command filterSet { get; protected set; }

        #endregion

        public AcCurrentMeas(IeeeBase inDevice) : base(inDevice, "CURR:AC")
        {
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
            _device.WriteLine($"{ActivateThisModeCommand}");
            var answer = _device.QueryLine($"{RangeCommand}?");
            var numb = (decimal) HelpDeviceBase.StrToDouble(answer);
            _range = new MeasPoint<Data.Model.PhysicalQuantity.Current>(numb);
            var val = GetActiveMeasuredValue();
            _value = new MeasPoint<Data.Model.PhysicalQuantity.Current>(val);
        }

        public void Setting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            _device.WriteLine($"{RangeCommand} {_range.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            _device.WriteLine(filterSet.StrCommand);
        }

        public bool IsEnable { get; set; }

        public MeasPoint<Data.Model.PhysicalQuantity.Current> Range
        {
            get
            {
                Getting();
                return _range;
            }
            set
            {
                _range = value;
                Setting();
            }
        }

        MeasPoint<Data.Model.PhysicalQuantity.Current> IMeasureMode<MeasPoint<Data.Model.PhysicalQuantity.Current>>.
            Value
        {
            get
            {
                Getting();
                return _value;
            }
        }
    }

    public class DcVoltMeas : MeasureFunction344xxBase, IMeasureMode<MeasPoint<Voltage>>
    {
        #region Fields

        private MeasPoint<Voltage> _range;
        private MeasPoint<Voltage> _value;

        #endregion

        public DcVoltMeas(IeeeBase inDevice) : base(inDevice, "VOLT:DC")
        {
        }

        public void Getting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            var answer = _device.QueryLine($"{RangeCommand}?");
            var numb = (decimal) HelpDeviceBase.StrToDouble(answer);
            _range = new MeasPoint<Voltage>(numb);

            var val = GetActiveMeasuredValue();
            _value = new MeasPoint<Voltage>(val);
        }

        public void Setting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            _device.WriteLine($"{RangeCommand} {Range.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            _device.WriteLine($"{functionName}:NPLC 100");
        }

        public bool IsEnable { get; set; }

        public MeasPoint<Voltage> Range
        {
            get
            {
                Getting();
                return _range;
            }
            set
            {
                _range = value;
                Setting();
            }
        }

        MeasPoint<Voltage> IMeasureMode<MeasPoint<Voltage>>.Value
        {
            get
            {
                Getting();
                return _value;
            }
        }
    }

    public class DcCurrentMeas : MeasureFunction344xxBase, IMeasureMode<MeasPoint<Data.Model.PhysicalQuantity.Current>>
    {
        #region Fields

        private MeasPoint<Data.Model.PhysicalQuantity.Current> _range;
        private MeasPoint<Data.Model.PhysicalQuantity.Current> _value;

        #endregion

        public DcCurrentMeas(IeeeBase inDevice) : base(inDevice, "CURR:DC")
        {
        }

        public void Getting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            var answer = _device.QueryLine($"{RangeCommand}?");
            var numb = (decimal) HelpDeviceBase.StrToDouble(answer);
            _range = new MeasPoint<Data.Model.PhysicalQuantity.Current>(numb);

            var val = GetActiveMeasuredValue();
            _value = new MeasPoint<Data.Model.PhysicalQuantity.Current>(val);
        }

        public void Setting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            _device.WriteLine($"{RangeCommand} {Range.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            _device.WriteLine($"{functionName}:NPLC 100");
        }

        public bool IsEnable { get; set; }

        public MeasPoint<Data.Model.PhysicalQuantity.Current> Range
        {
            get
            {
                Getting();
                return _range;
            }
            set
            {
                _range = value;
                Setting();
            }
        }

        MeasPoint<Data.Model.PhysicalQuantity.Current> IMeasureMode<MeasPoint<Data.Model.PhysicalQuantity.Current>>.
            Value
        {
            get
            {
                Getting();
                return _value;
            }
        }
    }

    public class Resistance2W : MeasureFunction344xxBase, IMeasureMode<MeasPoint<Resistance>>
    {
        #region Fields

        private MeasPoint<Resistance> _range;
        private MeasPoint<Resistance> _value;

        #endregion

        public Resistance2W(IeeeBase inDevice) : base(inDevice, "RES")
        {
        }

        public void Getting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            var answer = _device.QueryLine($"{RangeCommand}?");
            var numb = (decimal) HelpDeviceBase.StrToDouble(answer);
            _range = new MeasPoint<Resistance>(numb);

            var val = GetActiveMeasuredValue();
            _value = new MeasPoint<Resistance>(val);
        }

        public void Setting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            _device.WriteLine($"{RangeCommand} {Range.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            _device.WriteLine($"{functionName}:NPLC 100");
        }

        public bool IsEnable { get; set; }

        public MeasPoint<Resistance> Range
        {
            get
            {
                Getting();
                return _range;
            }
            set
            {
                _range = value;
                Setting();
            }
        }

        MeasPoint<Resistance> IMeasureMode<MeasPoint<Resistance>>.Value
        {
            get
            {
                Getting();
                return _value;
            }
        }
    }

    public class Resistance4W : MeasureFunction344xxBase, IMeasureMode<MeasPoint<Resistance>>
    {
        #region Fields

        private MeasPoint<Resistance> _range;
        private MeasPoint<Resistance> _value;

        #endregion

        public Resistance4W(IeeeBase inDevice) : base(inDevice, "FRES")
        {
        }

        public void Getting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            var answer = _device.QueryLine($"{RangeCommand}?");
            var numb = (decimal) HelpDeviceBase.StrToDouble(answer);
            _range = new MeasPoint<Resistance>(numb);

            var val = GetActiveMeasuredValue();
            _value = new MeasPoint<Resistance>(val);
        }

        public void Setting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            _device.WriteLine($"{RangeCommand} {Range.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            _device.WriteLine($"{functionName}:NPLC 100");
        }

        public bool IsEnable { get; set; }

        public MeasPoint<Resistance> Range
        {
            get
            {
                Getting();
                return _range;
            }
            set
            {
                _range = value;
                Setting();
            }
        }

        MeasPoint<Resistance> IMeasureMode<MeasPoint<Resistance>>.Value
        {
            get
            {
                Getting();
                return _value;
            }
        }
    }
}