using System;
using System.Linq;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
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

    public abstract class MeasureFunction344xxBase<T> where T : class, IPhysicalQuantity<T>, new()
    {
        #region Fields

        protected IeeeBase _device;
        protected MeasPoint<T> _range;

        protected MeasPoint<T> _value;
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

        protected decimal? GetMeasuredValue()
        {
            _device.WriteLine("TRIG:SOUR BUS");
            _device.WriteLine("INIT");
            _device.WriteLine("*TRG");
            var answer = _device.QueryLine("FETCH?");

            var numb = (decimal) HelpDeviceBase.StrToDouble(answer);
            return numb;
        }

        #endregion

        
    }

    public class AcVoltMeas : MeasureFunction344xxBase<Voltage>, IMeasureMode<MeasPoint<Voltage>>, IAcFilter
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

        public AcVoltMeas(IeeeBase inDevice) : base(inDevice, "VOLT:AC")
        {
            _range = new MeasPoint<Voltage>(1);
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
            _range = new MeasPoint<Voltage>(numb);
            var val = GetMeasuredValue();
            if (val != null)
                _value = new MeasPoint<Voltage>((decimal) val);
            else
                _value = null;
        }

        public void Setting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            if (AutoRange)
            {
                _device.WriteLine($"{RangeCommand}:auto on");
            }
            else
            {
                _device.WriteLine($"{RangeCommand}:auto off");
                _device.WriteLine($"{RangeCommand} {Range.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            }

            _device.WriteLine(filterSet.StrCommand);
        }

        public bool IsEnable { get; }

        public MeasPoint<Voltage> Range
        {
            get => _range;
            set
            {
                AutoRange = false;
                _range = value;
            }
        }

        public bool AutoRange { get; set; }

        MeasPoint<Voltage> IMeasureMode<MeasPoint<Voltage>>.Value => _value;
        public MeasPoint<Voltage> GetActiveMeasuredValue()
        {
            var val = GetMeasuredValue();
            _value = val != null ? new MeasPoint<Voltage>((decimal)val) : null;
            return _value;
        }
    }

    public class AcCurrentMeas : MeasureFunction344xxBase<Data.Model.PhysicalQuantity.Current>,
                                 IMeasureMode<MeasPoint<Data.Model.PhysicalQuantity.Current>>,
                                 IAcFilter
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

        public AcCurrentMeas(IeeeBase inDevice) : base(inDevice, "CURR:AC")
        {
            _range = new MeasPoint<Data.Model.PhysicalQuantity.Current>(1);
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
            Range = new MeasPoint<Data.Model.PhysicalQuantity.Current>(numb);
            var val = GetMeasuredValue();
            if (val != null)
                _value = new MeasPoint<Data.Model.PhysicalQuantity.Current>((decimal) val);
            else
                _value = null;
        }

        public void Setting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            if (AutoRange)
            {
                _device.WriteLine($"{RangeCommand}:auto on");
            }
            else
            {
                _device.WriteLine($"{RangeCommand}:auto off");
                _device.WriteLine($"{RangeCommand} {Range.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            }

            _device.WriteLine(filterSet.StrCommand);
        }

        public bool IsEnable { get; set; }

        public MeasPoint<Data.Model.PhysicalQuantity.Current> Range
        {
            get => _range;
            set
            {
                AutoRange = false;
                _range = value;
            }
        }

        public bool AutoRange { get; set; }

        MeasPoint<Data.Model.PhysicalQuantity.Current> IMeasureMode<MeasPoint<Data.Model.PhysicalQuantity.Current>>.
            Value =>
            _value;

        public MeasPoint<Data.Model.PhysicalQuantity.Current> GetActiveMeasuredValue()
        {
            var val = GetMeasuredValue();
            _value = val != null ? new MeasPoint<Data.Model.PhysicalQuantity.Current>((decimal)val) : null;
            return _value;
        }
    }

    public class DcVoltMeas : MeasureFunction344xxBase<Voltage>, IMeasureMode<MeasPoint<Voltage>>
    {
        #region Fields


        #endregion

        public DcVoltMeas(IeeeBase inDevice) : base(inDevice, "VOLT:DC")
        {
            _range = new MeasPoint<Voltage>(1);
        }

        public void Getting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            var answer = _device.QueryLine($"{RangeCommand}?");
            var numb = (decimal) HelpDeviceBase.StrToDouble(answer);
            _range = new MeasPoint<Voltage>(numb);
            
        }

        public void Setting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");

            if (AutoRange)
            {
                _device.WriteLine($"{RangeCommand}:auto on");
            }
            else
            {
                _device.WriteLine($"{RangeCommand}:auto off");
                _device.WriteLine($"{RangeCommand} {_range.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            }

            _device.WriteLine($"{functionName}:NPLC 100");
        }

        public MeasPoint<Voltage> GetActiveMeasuredValue()
        {
            var val = GetMeasuredValue();
            _value = val != null ? new MeasPoint<Voltage>((decimal)val) : null;
            return _value;
        }

        public bool IsEnable { get; set; }

        public MeasPoint<Voltage> Range
        {
            get => _range;
            set
            {
                AutoRange = false;
                _range = value;
            }
        }

        public bool AutoRange { get; set; }

        MeasPoint<Voltage> IMeasureMode<MeasPoint<Voltage>>.Value => _value;
    }

    public class DcCurrentMeas : MeasureFunction344xxBase<Data.Model.PhysicalQuantity.Current>,
                                 IMeasureMode<MeasPoint<Data.Model.PhysicalQuantity.Current>>
    {
        
        public DcCurrentMeas(IeeeBase inDevice) : base(inDevice, "CURR:DC")
        {
            _range = new MeasPoint<Data.Model.PhysicalQuantity.Current>(1);
        }

        public void Getting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            var answer = _device.QueryLine($"{RangeCommand}?");
            var numb = (decimal) HelpDeviceBase.StrToDouble(answer);
            Range = new MeasPoint<Data.Model.PhysicalQuantity.Current>(numb);

            var val = GetMeasuredValue();
            if (val != null)
                _value = new MeasPoint<Data.Model.PhysicalQuantity.Current>((decimal) val);
            else
                _value = null;
        }

        public void Setting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            if (AutoRange)
            {
                _device.WriteLine($"{RangeCommand}:auto on");
            }
            else
            {
                _device.WriteLine($"{RangeCommand}:auto off");
                _device.WriteLine($"{RangeCommand} {Range.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            }

            _device.WriteLine($"{functionName}:NPLC 100");
        }

        public bool IsEnable { get; set; }

        public MeasPoint<Data.Model.PhysicalQuantity.Current> Range
        {
            get => _range;
            set
            {
                AutoRange = false;
                _range = value;
            }
        }

        public bool AutoRange { get; set; }

        MeasPoint<Data.Model.PhysicalQuantity.Current> IMeasureMode<MeasPoint<Data.Model.PhysicalQuantity.Current>>.
            Value =>
            _value;

        public MeasPoint<Data.Model.PhysicalQuantity.Current> GetActiveMeasuredValue()
        {
            var val = GetMeasuredValue();
            _value = val != null ? new MeasPoint<Data.Model.PhysicalQuantity.Current>((decimal)val) : null;
            return _value;
        }
    }

    public class Resistance2W : MeasureFunction344xxBase<Resistance>, IMeasureMode<MeasPoint<Resistance>>
    {
        public Resistance2W(IeeeBase inDevice) : base(inDevice, "RES")
        {
            _range = new MeasPoint<Resistance>(1);
        }

        public void Getting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            var answer = _device.QueryLine($"{RangeCommand}?");
            var numb = (decimal) HelpDeviceBase.StrToDouble(answer);
            Range = new MeasPoint<Resistance>(numb);

            var val = GetMeasuredValue();
            if (val != null)
                _value = new MeasPoint<Resistance>((decimal) val);
            else
                _value = null;
        }

        public void Setting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            if (AutoRange)
            {
                _device.WriteLine($"{RangeCommand}:auto on");
            }
            else
            {
                _device.WriteLine($"{RangeCommand}:auto off");
                _device.WriteLine($"{RangeCommand} {Range.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            }

            _device.WriteLine($"{functionName}:NPLC 100");
        }

        public bool IsEnable { get; set; }

        public MeasPoint<Resistance> Range
        {
            get => _range;
            set
            {
                AutoRange = false;
                _range = value;
            }
        }

        public bool AutoRange { get; set; }

        MeasPoint<Resistance> IMeasureMode<MeasPoint<Resistance>>.Value => _value;
        public MeasPoint<Resistance> GetActiveMeasuredValue()
        {
            var val = GetMeasuredValue();
            _value = val != null ? new MeasPoint<Resistance>((decimal)val) : null;
            return _value;
        }
    }

    public class Resistance4W : MeasureFunction344xxBase<Resistance>, IMeasureMode<MeasPoint<Resistance>>
    {
        public Resistance4W(IeeeBase inDevice) : base(inDevice, "FRES")
        {
            _range = new MeasPoint<Resistance>(1);
        }

        public void Getting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            var answer = _device.QueryLine($"{RangeCommand}?");
            var numb = (decimal) HelpDeviceBase.StrToDouble(answer);
            Range = new MeasPoint<Resistance>(numb);

            var val = GetMeasuredValue();
            if (val != null)
                _value = new MeasPoint<Resistance>((decimal) val);
            else
                _value = null;
        }

        public void Setting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            if (AutoRange)
            {
                _device.WriteLine($"{RangeCommand}:auto on");
            }
            else
            {
                _device.WriteLine($"{RangeCommand}:auto off");
                _device.WriteLine($"{RangeCommand} {Range.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            }

            _device.WriteLine($"{functionName}:NPLC 100");
        }

        public bool IsEnable { get; set; }

        public MeasPoint<Resistance> Range
        {
            get => _range;
            set
            {
                AutoRange = false;
                _range = value;
            }
        }

        public bool AutoRange { get; set; }

        MeasPoint<Resistance> IMeasureMode<MeasPoint<Resistance>>.Value => _value;
        public MeasPoint<Resistance> GetActiveMeasuredValue()
        {
            var val = GetMeasuredValue();
            _value = val != null ? new MeasPoint<Resistance>((decimal)val) : null;
            return _value;
        }
    }
}