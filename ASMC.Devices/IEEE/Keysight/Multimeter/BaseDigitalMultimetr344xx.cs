﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AP.Extension;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.SourceAndMeter;
using ASMC.Devices.Model;

namespace ASMC.Devices.IEEE.Keysight.Multimeter
{
    public abstract class BaseDigitalMultimetr344xx : IDigitalMultimetr344xx
    {
        #region Property

        public IMeterPhysicalQuantity<Resistance> Resistance4W { get; set; }

        #endregion

        #region Field

        private readonly IeeeBase _device;

        #endregion

        protected BaseDigitalMultimetr344xx()
        {
            _device = new IeeeBase();
            AcVoltage = new AcVoltMeas(_device);
            DcVoltage = new DcVoltMeas(_device);
            DcCurrent = new DcCurrentMeas(_device);
            AcCurrent = new AcCurrentMeas(_device);
            Resistance4W = new Resist4W(_device);
            Resistance2W = new Resist2W(_device);
        }

        #region IDigitalMultimetr344xx Members

        public IMeterPhysicalQuantity<Resistance> Resistance2W { get; set; }
        public IMeterPhysicalQuantity<Voltage> DcVoltage { get; set; }
        public IMeterPhysicalQuantity<Data.Model.PhysicalQuantity.Current> DcCurrent { get; set; }

        /// <inheritdoc />
        public IAcFilter<Voltage> AcVoltage { get; }

        /// <inheritdoc />
        public IAcFilter<Data.Model.PhysicalQuantity.Current> AcCurrent { get; }


        /// <inheritdoc />
        public string UserType { get; protected set; }

        /// <inheritdoc />
        public void Dispose()
        {
            _device.Dispose();
        }

        /// <inheritdoc />
        public bool IsTestConnect { get; }

        /// <inheritdoc />
        public async Task InitializeAsync()
        {
            var fileName = @$"{Environment.CurrentDirectory}\acc\{UserType}.acc";
            if (!File.Exists(fileName)) return;
            this.FillRangesDevice(fileName);
        }

        /// <inheritdoc />
        public string StringConnection
        {
            get => _device.StringConnection;
            set
            {
                _device.StringConnection = value;
#pragma warning disable 4014
                InitializeAsync();
#pragma warning restore 4014
            }
        }

        /// <inheritdoc />
        public IMeterPhysicalQuantity<Capacity> Capacity { get; }

        /// <inheritdoc />
        public IMeterPhysicalQuantity<Frequency> Frequency { get; }

        #endregion

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

        /// <summary>
        ///     Вызов самотестирования.
        /// </summary>
        /// <returns></returns>
        public bool SelfTest()
        {
            return _device.SelfTest("+0");
        }

        #region Nested type: AcCurrentMeas

        public class AcCurrentMeas : MultiPointMeasureFunction344XxBase<Data.Model.PhysicalQuantity.Current>,
            IAcFilter<Data.Model.PhysicalQuantity.Current>
        {
            #region Property

            public Command FilterSet { get; protected set; }

            #endregion

            #region Field

            private readonly IeeeBase device;

            #endregion


            public AcCurrentMeas(IeeeBase inDevice) : base(inDevice, "CURR:AC")
            {
                device = inDevice;
                RangeStorage = new RangeDevice();
            }

            #region IAcFilter<Current> Members

            public override void Setting()
            {
                base.Setting();
                device.WriteLine(Filter.FilterSelect.StrCommand);
            }

            /// <inheritdoc />
            public IFilter<Data.Model.PhysicalQuantity.Current> Filter { get; }

            #endregion

            #region Nested type: Fil

            public class Fil : IFilter<Data.Model.PhysicalQuantity.Current>
            {
                public Fil()
                {
                    Filters = new ICommand[]
                    {
                        new Command("Det:Band 3", "", 3),
                        new Command("Det:Band 20", "", 20),
                        new Command("Det:Band 200", "", 200)
                    };
                    Array.Sort(Filters);
                }

                #region IFilter<Current> Members

                /// <inheritdoc />
                public void SetFilter(MeasPoint<Frequency> filterFreq)
                {
                    FilterSelect = Filters.LastOrDefault(q => q.Value < (double) filterFreq
                        .MainPhysicalQuantity.GetNoramalizeValueToSi());
                }

                /// <inheritdoc />
                public void SetFilter(MeasPoint<Data.Model.PhysicalQuantity.Current, Frequency> filterFreq)
                {
                    FilterSelect = Filters.LastOrDefault(q => q.Value < (double) filterFreq
                        .AdditionalPhysicalQuantity.GetNoramalizeValueToSi());
                }

                /// <inheritdoc />
                public void SetFilter(ICommand filter)
                {
                    FilterSelect = Filters.LastOrDefault(q => q.Value < filter.Value);
                }

                /// <inheritdoc />
                public ICommand FilterSelect { get; private set; }

                /// <inheritdoc />
                public ICommand[] Filters { get; protected set; }

                #endregion
            }

            #endregion

            #region Nested type: RangeDevice

            public class RangeDevice : RangeDeviceBase<Data.Model.PhysicalQuantity.Current, Frequency>
            {
                /// <inheritdoc />
                [AccRange("Mode: AC Current", typeof(MeasPoint<Data.Model.PhysicalQuantity.Current,Frequency>))]
                public override RangeStorage<PhysicalRange<Data.Model.PhysicalQuantity.Current,Frequency>> Ranges { get; set; }
            }

            #endregion
        }

        #endregion

        #region Nested type: AcVoltMeas

        public class AcVoltMeas :  MultiPointMeasureFunction344XxBase<Voltage>, IAcFilter<Voltage>
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
                RangeStorage = new RangeDevice();
            }

            #region IAcFilter<Voltage> Members

            public override void Setting()
            {
                base.Setting();
                _device.WriteLine(FilterSet.StrCommand);
            }

            /// <inheritdoc />
            public IFilter<Voltage> Filter { get; }

            #endregion

            public void SetFilter(MeasPoint<Frequency> filterFreq)
            {
                Array.Sort(_filters);
                FilterSet = _filters.LastOrDefault(q => q.Value < (double) filterFreq
                    .MainPhysicalQuantity.GetNoramalizeValueToSi());
            }

            #region Nested type: RangeDevice

            public class RangeDevice : RangeDeviceBase<Voltage,Frequency>
            {
                /// <inheritdoc />
                [AccRange("Mode: Volt AC", typeof(MeasPoint<Voltage, Frequency>))]
                public override RangeStorage<PhysicalRange<Voltage,Frequency>> Ranges { get; set; }             
            }

            #endregion

        }

        #endregion

        #region Nested type: DcCurrentMeas

        public class DcCurrentMeas : MeasureFunction344XxBase<Data.Model.PhysicalQuantity.Current>
        {
            public DcCurrentMeas(IeeeBase inDevice) : base(inDevice, "CURR:DC")
            {
                RangeStorage = new RangeDevice();
            }

            public override void Setting()
            {
                base.Setting();
                _device.WriteLine($"{FunctionName}:NPLC 100");
            }

            #region Nested type: RangeDevice

            public class RangeDevice : RangeDeviceBase<Data.Model.PhysicalQuantity.Current>
            {
                public RangeDevice()
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

        #endregion

        #region Nested type: DcVoltMeas

        public class DcVoltMeas : MeasureFunction344XxBase<Voltage>
        {
            public DcVoltMeas(IeeeBase inDevice) : base(inDevice, "VOLT:DC")
            {
                RangeStorage = new RangeDevice();
            }

            public override void Setting()
            {
                base.Setting();
                _device.WriteLine($"{FunctionName}:NPLC 100");
            }

            #region Nested type: RangeDevice

            public class RangeDevice : RangeDeviceBase<Voltage>
            {
                /// <inheritdoc />
                [AccRange("Mode: Volt DC", typeof(MeasPoint<Voltage>))]
                public override RangeStorage<PhysicalRange<Voltage>> Ranges { get; set; }
            }

            #endregion
        }

        #endregion

     

        #region Nested type: Resist2W

        public class Resist2W : MeasureFunction344XxBase<Resistance>
        {
            public Resist2W(IeeeBase inDevice) : base(inDevice, "RES")
            {
                RangeStorage = new RangeDevice();
            }

            public override void Setting()
            {
                base.Setting();
                _device.WriteLine($"{FunctionName}:NPLC 100");
            }

            #region Nested type: RangeDevice

            public class RangeDevice : RangeDeviceBase<Resistance>
            {
                public RangeDevice()
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

        #endregion

        #region Nested type: Resist4W

        public class Resist4W : MeasureFunction344XxBase<Resistance>
        {
            public Resist4W(IeeeBase inDevice) : base(inDevice, "FRES")
            {
                RangeStorage = new RangeDevice();
            }

            public override void Setting()
            {
                base.Setting();
                _device.WriteLine($"{FunctionName}:NPLC 100");
            }

            #region Nested type: RangeDevice

            public class RangeDevice : RangeDeviceBase<Resistance>
            {
                public RangeDevice()
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

        #endregion


        #region Nested type: MeasureFunction344XxBase

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
                var numb = (decimal)HelpDeviceBase.StrToDouble(value);
                return new MeasPoint<T>(numb);
            }

            #endregion Methods
        }

        public abstract class MultiPointMeasureFunction344XxBase<T> : IMeterPhysicalQuantity<T,Frequency>
        where T : class, IPhysicalQuantity<T>, new()
        {
            #region Field

            protected IeeeBase _device;
            protected string ActivateThisModeCommand;
            protected string FunctionName;
            protected string RangeCommand;

            #endregion

            protected MultiPointMeasureFunction344XxBase(IeeeBase inDevice, string inFunctionName)
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

            protected MeasPoint<T,Frequency> ConvertStringToMeasPoint(string value)
            {
                var numb = (decimal)HelpDeviceBase.StrToDouble(value);
                return new MeasPoint<T,Frequency>(numb,0);
            }

            #endregion Methods

            /// <inheritdoc />
            public IRangePhysicalQuantity<T, Frequency> RangeStorage { get; protected set; }
        }
        #endregion
    }
}