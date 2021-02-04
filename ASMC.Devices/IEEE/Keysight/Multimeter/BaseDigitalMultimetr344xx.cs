using System;
using System.Linq;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.Multimetr.Mode;
using MathNet.Numerics;

namespace ASMC.Devices.IEEE.Keysight.Multimeter
{
    public class BaseDigitalMultimetr344xx : IeeeBase, IDigitalMultimetr344xx
    {

        /// <summary>
        /// Вызов самотестирования.
        /// </summary>
        /// <returns></returns>
        public bool SelfTest()
        {
            return this.SelfTest("1");
        }
        public BaseDigitalMultimetr344xx()
        {
            AcVoltage = new AcVoltMeas(this);
            DcVoltage = new DcVoltMeas(this);
            DcCurrent = new DcCurrentMeas(this);
            AcCurrent = new AcCurrentMeas(this);
            Resistance4W = new Resistance4W(this);
            Resistance2W = new Resistance2W(this);
        }

        public IMeterPhysicalQuantity<Resistance> Resistance2W { get; set; }
        public IMeterPhysicalQuantity<Resistance> Resistance4W { get; set; }
        public IMeterPhysicalQuantity<Voltage> DcVoltage { get; set; }
        public IMeterPhysicalQuantity<Voltage> AcVoltage { get; set; }
        public IMeterPhysicalQuantity<Data.Model.PhysicalQuantity.Current> DcCurrent { get; set; }
        public IMeterPhysicalQuantity<Data.Model.PhysicalQuantity.Current> AcCurrent { get; set; }

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
        protected T> _range;

        protected T> _value;
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

    public class AcVoltMeas : MeasureFunction344xxBase<Voltage>, IMeterPhysicalQuantity<Voltage>, IAcFilter
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

        public Command FilterSet { get; protected set; }

        #endregion

        public AcVoltMeas(IeeeBase inDevice) : base(inDevice, "VOLT:AC")
        {
            RangeStorage = new Range();
        }

        public void SetFilter(MeasPoint<Frequency> filterFreq)
        {
            Array.Sort(Filters);
            Array.Reverse(Filters);
            FilterSet = Filters.FirstOrDefault(q => q.Value < (double) filterFreq
                                                                      .MainPhysicalQuantity.GetNoramalizeValueToSi());
        }
        public class Range: RangeBaseDevice<Voltage>
        {
            public Range()
            {
                Ranges = new RangeStorage<PhysicalRange<Voltage>>();
                var ary = new RangeStorage<PhysicalRange<Voltage>>
                {
                    Ranges = new[]
                    {
                        new PhysicalRange<Voltage>(new MeasPoint<Voltage>(0),
                            new MeasPoint<Voltage>(100, UnitMultiplier.Mili))
                    }
                };

            }
        }
        public void Getting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            var answer = _device.QueryLine($"{RangeCommand}?");
            var numb = (decimal) HelpDeviceBase.StrToDouble(answer);
            RangeStorage.SetRange(new MeasPoint<Voltage>(numb));
            Value = GetValue();
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
                _device.WriteLine($"{RangeCommand} {RangeStorage.SelectRange.End.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            }

            _device.WriteLine(FilterSet.StrCommand);
        }

        public bool AutoRange { get; set; }


        /// <inheritdoc />
        public IRangePhysicalQuantity<Voltage> RangeStorage { get; set; }

        /// <inheritdoc />
        public MeasPoint<Voltage> GetValue()
        {
            var val = GetMeasuredValue();
            return val != null ? new MeasPoint<Voltage> ((decimal)val) : null;
        }

        /// <inheritdoc />
        public MeasPoint<Voltage> Value { get; set; }
    }

    public class AcCurrentMeas : MeasureFunction344xxBase<Data.Model.PhysicalQuantity.Current>,
                                 IMeterPhysicalQuantity<Data.Model.PhysicalQuantity.Current>,
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

        public void SetFilter(Frequency> filterFreq)
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

        MeasPoint<Data.Model.PhysicalQuantity.Current> IMeterPhysicalQuantity<Data.Model.PhysicalQuantity.Current>.
            Value =>
            _value;

        public MeasPoint<Data.Model.PhysicalQuantity.Current> GetActiveMeasuredValue()
        {
            var val = GetMeasuredValue();
            _value = val != null ? new Data.Model.PhysicalQuantity.Current>((decimal)val) : null;
            return _value;
        }

        /// <inheritdoc />
        public IRangePhysicalQuantity<Data.Model.PhysicalQuantity.Current> RangeStorage { get; set; }

        /// <inheritdoc />
        public MeasPoint<Data.Model.PhysicalQuantity.Current> GetValue()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public MeasPoint<Data.Model.PhysicalQuantity.Current> Value { get; }

        /// <inheritdoc />
        public void SetFilter(MeasPoint<Frequency> filterFreq)
        {
            throw new NotImplementedException();
        }
    }

    public class DcVoltMeas : MeasureFunction344xxBase<Voltage>, IMeterPhysicalQuantity<Voltage>
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
            _value = val != null ? new Voltage>((decimal)val) : null;
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

        MeasPoint<Voltage> IMeterPhysicalQuantity<Voltage>.Value => _value;
    }

    public class DcCurrentMeas : MeasureFunction344xxBase<Data.Model.PhysicalQuantity.Current>,
                                 IMeterPhysicalQuantity<Data.Model.PhysicalQuantity.Current>
    {
        
        public DcCurrentMeas(IeeeBase inDevice) : base(inDevice, "CURR:DC")
        {
            _range = new Data.Model.PhysicalQuantity.Current>(1);
        }

        public void Getting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            var answer = _device.QueryLine($"{RangeCommand}?");
            var numb = (decimal) HelpDeviceBase.StrToDouble(answer);
            Range = new Data.Model.PhysicalQuantity.Current>(numb);

            var val = GetMeasuredValue();
            if (val != null)
                _value = new Data.Model.PhysicalQuantity.Current>((decimal) val);
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

        public Data.Model.PhysicalQuantity.Current> Range
        {
            get => _range;
            set
            {
                AutoRange = false;
                _range = value;
            }
        }

        public bool AutoRange { get; set; }

        Data.Model.PhysicalQuantity.Current> IMeterPhysicalQuantity<Data.Model.PhysicalQuantity.Current>.
            Value =>
            _value;

        public Data.Model.PhysicalQuantity.Current> GetActiveMeasuredValue()
        {
            var val = GetMeasuredValue();
            _value = val != null ? new Data.Model.PhysicalQuantity.Current>((decimal)val) : null;
            return _value;
        }
    }

    public class Resistance2W : MeasureFunction344xxBase<Resistance>, IMeterPhysicalQuantity<Resistance>
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

        MeasPoint<Resistance> IMeterPhysicalQuantity<Resistance>.Value => _value;
        public MeasPoint<Resistance> GetActiveMeasuredValue()
        {
            var val = GetMeasuredValue();
            _value = val != null ? new Resistance>((decimal)val) : null;
            return _value;
        }
    }

    public class Resistance4W : MeasureFunction344xxBase<Resistance>, IMeterPhysicalQuantity<Resistance>
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

        MeasPoint<Resistance> IMeterPhysicalQuantity<Resistance>.Value => _value;
        public MeasPoint<Resistance> GetActiveMeasuredValue()
        {
            var val = GetMeasuredValue();
            _value = val != null ? new MeasPoint<Resistance>((decimal)val) : null;
            return _value;
        }
    }
}