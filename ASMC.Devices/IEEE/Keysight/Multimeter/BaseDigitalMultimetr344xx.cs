using System;
using System.Linq;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
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

        #region IDigitalMultimetr344xx Members

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

        #endregion

        /// <summary>
        ///     Вызов самотестирования.
        /// </summary>
        /// <returns></returns>
        public bool SelfTest()
        {
            return SelfTest("1");
        }
    }

    public abstract class MeasureFunction344XxBase<T> : IMeterPhysicalQuantity<T>
        where T : class, IPhysicalQuantity<T>, new()
    {
        #region Field

        protected IeeeBase _device;
        protected string ActivateThisModeCommand;
        protected string FunctionName;
        protected string RangeCommand;

        #endregion

        protected MeasureFunction344XxBase(IeeeBase inDevice, string inFunctionName)
        {
            _device = inDevice;
            FunctionName = inFunctionName;
            ActivateThisModeCommand = $"FUNC \"{FunctionName}\"";
            RangeCommand = $"{FunctionName}:RANG";
        }

        #region IMeterPhysicalQuantity<T> Members

        /// <inheritdoc />
        public virtual void Getting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            var answer = _device.QueryLine($"{RangeCommand}?");
            RangeStorage.SetRange(ConvertStringToMeasPoint(answer));
            GetValue();
        }

        /// <inheritdoc />
        public virtual void Setting()
        {
            _device.WriteLine($"{ActivateThisModeCommand}");
            if (RangeStorage.IsAutoRange)
            {
                _device.WriteLine($"{RangeCommand}:auto on");
            }
            else
            {
                _device.WriteLine($"{RangeCommand}:auto off");
                _device.WriteLine(
                    $"{RangeCommand} {RangeStorage.SelectRange.End.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            }
        }

        /// <inheritdoc />
        public IRangePhysicalQuantity<T> RangeStorage { get; set; }

        /// <inheritdoc />
        public MeasPoint<T> GetValue()
        {
            _device.WriteLine("TRIG:SOUR BUS");
            _device.WriteLine("INIT");
            _device.WriteLine("*TRG");
            var answer = _device.QueryLine("FETCH?");
            Value = ConvertStringToMeasPoint(answer);
            return Value;
        }

        /// <inheritdoc />
        public MeasPoint<T> Value { get; private set; }

        #endregion

        #region Methods

        protected MeasPoint<T> ConvertStringToMeasPoint(string value)
        {
            var numb = (decimal) HelpDeviceBase.StrToDouble(value);
            return new MeasPoint<T>();
        }

        #endregion Methods
    }

    public class AcVoltMeas : MeasureFunction344XxBase<Voltage>, IAcFilter
    {
        #region Property

        public Command FilterSet { get; protected set; }

        #endregion

        #region Field

        private readonly Command[] _filters =
        {
            new Command("Det:Band 3", "", 3),
            new Command("Det:Band 20", "", 20),
            new Command("Det:Band 200", "", 200)
        };

        #endregion

        public AcVoltMeas(IeeeBase inDevice) : base(inDevice, "VOLT:AC")
        {
            RangeStorage = new Range();
        }

        #region IAcFilter Members

        public void SetFilter(MeasPoint<Frequency> filterFreq)
        {
            Array.Sort(_filters);
            Array.Reverse(_filters);
            FilterSet = _filters.FirstOrDefault(q => q.Value < (double) filterFreq
                .MainPhysicalQuantity.GetNoramalizeValueToSi());
        }

        #endregion

        public override void Setting()
        {
            base.Setting();
            _device.WriteLine(FilterSet.StrCommand);
        }

        #region Nested type: Range

        public class Range : RangeBaseDevice<Voltage>
        {
            public Range()
            {
                Ranges.Ranges = new[]
                {
                    new PhysicalRange<Voltage>(new MeasPoint<Voltage>(0),
                        new MeasPoint<Voltage>(100, UnitMultiplier.Mili))
                };
            }
        }

        #endregion
    }

    public class AcCurrentMeas : MeasureFunction344XxBase<Data.Model.PhysicalQuantity.Current>, IAcFilter
    {
        #region Property

        public Command FilterSet { get; protected set; }

        #endregion

        #region Field

        private readonly Command[] _filters =
        {
            new Command("Det:Band 3", "", 3),
            new Command("Det:Band 20", "", 20),
            new Command("Det:Band 200", "", 200)
        };

        #endregion

        public AcCurrentMeas(IeeeBase inDevice) : base(inDevice, "CURR:AC")
        {
            RangeStorage = new Range();
        }

        #region IAcFilter Members

        public void SetFilter(MeasPoint<Frequency> filterFreq)
        {
            Array.Sort(_filters);
            Array.Reverse(_filters);

            FilterSet = _filters.FirstOrDefault(q => q.Value < (double) filterFreq
                .MainPhysicalQuantity.GetNoramalizeValueToSi());
        }

        #endregion

        public override void Setting()
        {
            base.Setting();
            _device.WriteLine(FilterSet.StrCommand);
        }

        #region Nested type: Range

        public class Range : RangeBaseDevice<Data.Model.PhysicalQuantity.Current>
        {
            public Range()
            {
                Ranges.Ranges = new[]
                {
                    new PhysicalRange<Data.Model.PhysicalQuantity.Current>(
                        new MeasPoint<Data.Model.PhysicalQuantity.Current>(0),
                        new MeasPoint<Data.Model.PhysicalQuantity.Current>(100, UnitMultiplier.Mili))
                };
            }
        }

        #endregion
    }

    public class DcVoltMeas : MeasureFunction344XxBase<Voltage>
    {
        public DcVoltMeas(IeeeBase inDevice) : base(inDevice, "VOLT:DC")
        {
            RangeStorage = new Range();
        }

        public override void Setting()
        {
            base.Setting();
            _device.WriteLine($"{FunctionName}:NPLC 100");
        }

        #region Nested type: Range

        public class Range : RangeBaseDevice<Voltage>
        {
            public Range()
            {
                Ranges.Ranges = new[]
                {
                    new PhysicalRange<Voltage>(new MeasPoint<Voltage>(0),
                        new MeasPoint<Voltage>(100, UnitMultiplier.Mili))
                };
            }
        }

        #endregion
    }

    public class DcCurrentMeas : MeasureFunction344XxBase<Data.Model.PhysicalQuantity.Current>
    {
        public DcCurrentMeas(IeeeBase inDevice) : base(inDevice, "CURR:DC")
        {
            RangeStorage = new Range();
        }

        #region IMeterPhysicalQuantity<Current> Members

        public override void Setting()
        {
            base.Setting();
            _device.WriteLine($"{FunctionName}:NPLC 100");
        }

        #endregion

        #region Nested type: Range

        public class Range : RangeBaseDevice<Data.Model.PhysicalQuantity.Current>
        {
            public Range()
            {
                Ranges.Ranges = new[]
                {
                    new PhysicalRange<Data.Model.PhysicalQuantity.Current>(
                        new MeasPoint<Data.Model.PhysicalQuantity.Current>(0),
                        new MeasPoint<Data.Model.PhysicalQuantity.Current>(100, UnitMultiplier.Mili))
                };
            }
        }

        #endregion
    }

    public class Resistance2W : MeasureFunction344XxBase<Resistance>
    {
        public Resistance2W(IeeeBase inDevice) : base(inDevice, "RES")
        {
            RangeStorage = new Range();
        }

        public override void Setting()
        {
            base.Setting();
            _device.WriteLine($"{FunctionName}:NPLC 100");
        }

        #region Nested type: Range

        public class Range : RangeBaseDevice<Resistance>
        {
            public Range()
            {
                Ranges.Ranges = new[]
                {
                    new PhysicalRange<Resistance>(new MeasPoint<Resistance>(0),
                        new MeasPoint<Resistance>(100, UnitMultiplier.Mili))
                };
            }
        }

        #endregion
    }

    public class Resistance4W : MeasureFunction344XxBase<Resistance>
    {
        public Resistance4W(IeeeBase inDevice) : base(inDevice, "FRES")
        {
            RangeStorage = new Range();
        }

        public override void Setting()
        {
            base.Setting();
            _device.WriteLine($"{FunctionName}:NPLC 100");
        }

        #region Nested type: Range

        public class Range : RangeBaseDevice<Resistance>
        {
            public Range()
            {
                Ranges.Ranges = new[]
                {
                    new PhysicalRange<Resistance>(new MeasPoint<Resistance>(0),
                        new MeasPoint<Resistance>(100, UnitMultiplier.Mili))
                };
            }
        }

        #endregion
    }
}