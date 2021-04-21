using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AP.Extension;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;
using ASMC.Devices.Model;

namespace ASMC.Devices.IEEE.Keysight.Multimeter
{
    public abstract class BaseDigitalMultimetr344xx : IDigitalMultimetr344xx, IFrontRearPanel
    {
        #region Property

        public IMeterPhysicalQuantity<Resistance> Resistance4W { get; set; }

        #endregion

        #region Field

        protected readonly IeeeBase _device;

        #endregion

        protected BaseDigitalMultimetr344xx()
        {
            _device = new IeeeBase();
            AcVoltage = new AcVoltMeas(_device);
            DcVoltage = new DcVoltMeas(_device);
            DcCurrent = new DcCurrentMeas(_device);
            AcCurrent = new AcCurrentMeas(_device);
            Frequency = new Freq(_device);
            Resistance4W = new Resist4W(_device);
            Resistance2W = new Resist2W(_device);
        }

        #region IDigitalMultimetr344xx Members

        public IMeterPhysicalQuantity<Resistance> Resistance2W { get; }
        public IMeterPhysicalQuantity<Voltage> DcVoltage { get; }
        public IMeterPhysicalQuantity<Data.Model.PhysicalQuantity.Current> DcCurrent { get; }

        /// <inheritdoc />
        public IAcFilter<Voltage, Frequency> AcVoltage { get; }

        /// <inheritdoc />
        public IAcFilter<Data.Model.PhysicalQuantity.Current, Frequency> AcCurrent { get; }


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
        public IMeterPhysicalQuantity<Frequency, Voltage> Frequency { get; }

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

        public class AcCurrentMeas : MultiPointMeasureFunction344XxBase<Data.Model.PhysicalQuantity.Current, Frequency>,
            IAcFilter<Data.Model.PhysicalQuantity.Current, Frequency>
        {
            #region Property

            public Command FilterSet { get; protected set; }

            #endregion

            #region Field

            private readonly IeeeBase device;

            #endregion


            public AcCurrentMeas(IeeeBase inDevice) : base(inDevice)
            {
                device = inDevice;
                RangeStorage = new RangeDevice();
                Filter = new Fil();
            }

            #region IAcFilter<Current,Frequency> Members

            public override void Setting()
            {
                base.Setting();
                device.WriteLine(Filter.FilterSelect.StrCommand);
            }

            /// <inheritdoc />
            public IFilter<Data.Model.PhysicalQuantity.Current, Frequency> Filter { get; }

            #endregion

            /// <inheritdoc />
            protected override string GetFunctionName()
            {
                return "CURR:AC";
            }

            #region Nested type: Fil

            public class Fil : IFilter<Data.Model.PhysicalQuantity.Current, Frequency>
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

                #region IFilter<Current,Frequency> Members

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
                #region Property

                /// <inheritdoc />
                [AccRange("Mode: AC Current", typeof(MeasPoint<Data.Model.PhysicalQuantity.Current, Frequency>))]
                public override RangeStorage<PhysicalRange<Data.Model.PhysicalQuantity.Current, Frequency>> Ranges
                {
                    get;
                    set;
                }

                #endregion
            }

            #endregion
        }

        #endregion

      

        #region Nested type: AcVoltMeas

        public class AcVoltMeas : MultiPointMeasureFunction344XxBase<Voltage, Frequency>, IAcFilter<Voltage, Frequency>
        {
            public AcVoltMeas(IeeeBase inDevice) : base(inDevice)
            {
                RangeStorage = new RangeDevice();
                Filter = new Filt();
            }

            #region IAcFilter<Voltage,Frequency> Members

            public override void Setting()
            {
                base.Setting();
                Device.WriteLine(Filter.FilterSelect.StrCommand);
            }

            /// <inheritdoc />
            public IFilter<Voltage, Frequency> Filter { get; }

            #endregion

            /// <inheritdoc />
            protected override string GetFunctionName()
            {
                return "VOLT:AC";
            }

            #region Nested type: Filt

            public class Filt : IFilter<Voltage, Frequency>
            {
                #region IFilter<Voltage,Frequency> Members

                public ICommand FilterSelect { get; protected set; }

                public ICommand[] Filters { get; } =
                {
                    new Command("Det:Band 3", "", 3),
                    new Command("Det:Band 20", "", 20),
                    new Command("Det:Band 200", "", 200)
                };

                public void SetFilter(MeasPoint<Frequency> filterFreq)
                {
                    Array.Sort(Filters);
                    FilterSelect = Filters.LastOrDefault(q => q.Value < (double) filterFreq
                        .MainPhysicalQuantity.GetNoramalizeValueToSi());
                }

                public void SetFilter(MeasPoint<Voltage, Frequency> filterFreq)
                {
                    Array.Sort(Filters);
                    FilterSelect = Filters.LastOrDefault(q => q.Value < (double) filterFreq
                        .AdditionalPhysicalQuantity.GetNoramalizeValueToSi());
                }

                public void SetFilter(ICommand filter)
                {
                    FilterSelect = Filters.FirstOrDefault(q => q == filter);
                }

                #endregion
            }

            #endregion

            #region Nested type: RangeDevice

            public class RangeDevice : RangeDeviceBase<Voltage, Frequency>
            {
                #region Property

                /// <inheritdoc />
                [AccRange("Mode: Volt AC", typeof(MeasPoint<Voltage, Frequency>))]
                public override RangeStorage<PhysicalRange<Voltage, Frequency>> Ranges { get; set; }

                #endregion
            }

            #endregion
        }

        #endregion

        #region Nested type: DcCurrentMeas

        public class DcCurrentMeas : MeasureFunction344XxBase<Data.Model.PhysicalQuantity.Current>
        {
            public DcCurrentMeas(IeeeBase inDevice) : base(inDevice)
            {
                RangeStorage = new RangeDevice();
            }

            /// <inheritdoc />
            protected override string GetFunctionName()
            {
                return "CURR:DC";
            }

            public override void Setting()
            {
                base.Setting();
                Device.WriteLine($"{GetFunctionName()}:NPLC 100");
            }

            #region Nested type: RangeDevice

            public class RangeDevice : RangeDeviceBase<Data.Model.PhysicalQuantity.Current>
            {
                #region Property

                /// <inheritdoc />
                [AccRange("Mode: DC Current", typeof(MeasPoint<Data.Model.PhysicalQuantity.Current>))]
                public override RangeStorage<PhysicalRange<Data.Model.PhysicalQuantity.Current>> Ranges { get; set; }

                #endregion
            }

            #endregion
        }

        #endregion

        #region Nested type: DcVoltMeas

        public class DcVoltMeas : MeasureFunction344XxBase<Voltage>
        {
            public DcVoltMeas(IeeeBase inDevice) : base(inDevice)
            {
                RangeStorage = new RangeDevice();
            }

            /// <inheritdoc />
            protected override string GetFunctionName()
            {
                return "VOLT:DC";
            }

            public override void Setting()
            {
                base.Setting();
                Device.WriteLine($"{GetFunctionName()}:NPLC 100");
            }

            #region Nested type: RangeDevice

            public class RangeDevice : RangeDeviceBase<Voltage>
            {
                #region Property

                /// <inheritdoc />
                [AccRange("Mode: Volt DC", typeof(MeasPoint<Voltage>))]
                public override RangeStorage<PhysicalRange<Voltage>> Ranges { get; set; }

                #endregion
            }

            #endregion
        }

        #endregion

        #region Nested type: Freq

        public class Freq : MultiPointMeasureFunction344XxBase<Frequency, Voltage>
        {
            public Freq(IeeeBase device) : base(device)
            {
                RangeStorage = new RangeDevice();
            }
            /// <inheritdoc />
            protected override void SetRangeCommand()
            {
                //todo по этой логике мультиметр ставит предел по напряжению с максимальным значением (например 750 В), а измерить нужно например на меньшем пределе (например 1 В)!!! измерение не произойдет корректно

                Device.WriteLine($"{GetRangeNameCommand()} {RangeStorage.SelectRange.End.AdditionalPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            }
            /// <inheritdoc />
            protected override string GetRangeNameCommand()
            {
                return GetFunctionName() +":VOLT"+ ":RANG";
            }

            /// <inheritdoc />
            protected override string GetFunctionName()
            {
                return "FREQ";
            }

            #region Nested type: RangeDevice

            public class RangeDevice : RangeDeviceBase<Frequency, Voltage>
            {
                #region Property

                [AccRange("Mode: Hertz", typeof(MeasPoint<Frequency, Voltage>))]
                public override RangeStorage<PhysicalRange<Frequency, Voltage>> Ranges { get; set; }

                #endregion
            }

            #endregion
        }

        #endregion

        #region Nested type: MeasureFunction344XxBase

        public abstract class MeasureFunction344XxBase
        {
            #region Property

            protected IeeeBase Device { get; }

            protected static string Function => "FUNC";

            #endregion

            protected MeasureFunction344XxBase(IeeeBase device)
            {
                Device = device;
            }

            protected virtual string GetRangeNameCommand()
            {
                return GetFunctionName() + ":RANG";
            }

            protected void SetCurrentMode()
            {
                Device.WriteLine($"{Function} \"{GetFunctionName()}\"");
            }

            /// <summary>
            ///     Позволяет получить текст команды функции режима.
            /// </summary>
            /// <returns></returns>
            protected abstract string GetFunctionName();

            protected abstract bool SetAutoRange(bool isAutoRange);
        }

        public abstract class MeasureFunction344XxBase<T> : MeasureFunction344XxBase, IMeterPhysicalQuantity<T>
            where T : class, IPhysicalQuantity<T>, new()
        {
            /// <inheritdoc />
            protected MeasureFunction344XxBase(IeeeBase device) : base(device)
            {
            }

            #region IMeterPhysicalQuantity<T> Members

            /// <inheritdoc />
            public virtual void Getting()
            {
                SetCurrentMode();
                var answer = Device.QueryLine($"{GetRangeNameCommand()}?");
                RangeStorage.SetRange(ConvertStringToMeasPoint(answer));
                GetValue();
            }

            /// <inheritdoc />
            public virtual void Setting()
            {
                SetCurrentMode();
                if (!SetAutoRange(RangeStorage.IsAutoRange))
                    Device.WriteLine(
                        $"{GetRangeNameCommand()} {RangeStorage.SelectRange.End.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            }

            /// <inheritdoc />
            public IRangePhysicalQuantity<T> RangeStorage { get; set; }

            /// <inheritdoc />
            public MeasPoint<T> GetValue()
            {
                Device.WriteLine("TRIG:SOUR BUS");
                Device.WriteLine("INIT");
                Device.WriteLine("*TRG");
                var answer = Device.QueryLine("FETCH?");
                Value = ConvertStringToMeasPoint(answer);
                return Value;
            }

            /// <inheritdoc />
            public MeasPoint<T> Value { get; private set; }

            #endregion

            /// <inheritdoc />
            protected override bool SetAutoRange(bool isAutoRange)
            {
                Device.WriteLine(isAutoRange ? $"{GetRangeNameCommand()}:auto on" : $"{GetRangeNameCommand()}:auto off");

                return isAutoRange;
            }

            #region Methods

            protected MeasPoint<T> ConvertStringToMeasPoint(string value)
            {
                var numb = (decimal) HelpDeviceBase.StrToDouble(value);
                return new MeasPoint<T>(numb);
            }

            #endregion Methods
        }

        #endregion

        #region Nested type: MultiPointMeasureFunction344XxBase

        public abstract class MultiPointMeasureFunction344XxBase<T, T2> : MeasureFunction344XxBase,
            IMeterPhysicalQuantity<T, T2>
            where T : class, IPhysicalQuantity<T>, new() where T2 : class, IPhysicalQuantity<T2>, new()
        {
            /// <inheritdoc />
            protected MultiPointMeasureFunction344XxBase(IeeeBase device) : base(device)
            {
            }

            #region IMeterPhysicalQuantity<T,T2> Members

            /// <inheritdoc />
            public virtual void Getting()
            {
                SetCurrentMode();
                var answer = Device.QueryLine($"{GetRangeNameCommand()}?");
                RangeStorage.SetRange(ConvertStringToMeasPoint(answer));
                GetValue();
            }


            /// <inheritdoc />
            public virtual void Setting()
            {
                SetCurrentMode();
                if (!SetAutoRange(RangeStorage.IsAutoRange))
                    SetRangeCommand();
            }

            protected virtual void SetRangeCommand()
            {
                Device.WriteLine(
                    $"{GetRangeNameCommand()} {RangeStorage.SelectRange.End.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            }
            /// <inheritdoc />
            public MeasPoint<T> GetValue()
            {
                Device.WriteLine("TRIG:SOUR BUS");
                Device.WriteLine("INIT");
                Device.WriteLine("*TRG");
                var answer = Device.QueryLine("FETCH?");
                Value = ConvertStringToMeasPoint(answer);
                return Value;
            }

            /// <inheritdoc />
            public MeasPoint<T> Value { get; private set; }

            /// <inheritdoc />
            public IRangePhysicalQuantity<T, T2> RangeStorage { get; protected set; }

            #endregion

            protected override bool SetAutoRange(bool isAutoRange)
            {
                Device.WriteLine(RangeStorage.IsAutoRange ? $"{GetRangeNameCommand()}:auto on" : $"{GetRangeNameCommand()}:auto off");
                return isAutoRange;
            }

            #region Methods

            protected MeasPoint<T, T2> ConvertStringToMeasPoint(string value)
            {
                var numb = (decimal) HelpDeviceBase.StrToDouble(value);
                return new MeasPoint<T, T2>(numb, 0);
            }

            #endregion Methods
        }

        #endregion

        #region Nested type: Resist2W

        public class Resist2W : MeasureFunction344XxBase<Resistance>
        {
            public Resist2W(IeeeBase inDevice) : base(inDevice)
            {
                RangeStorage = new RangeDevice();
            }

            /// <inheritdoc />
            protected override string GetFunctionName()
            {
                return "RES";
            }

            public override void Setting()
            {
                base.Setting();
                Device.WriteLine($"{GetFunctionName()}:NPLC 100");
            }

            #region Nested type: RangeDevice

            public class RangeDevice : RangeDeviceBase<Resistance>
            {
                #region Property

                [AccRange("Mode: Ohms 2W", typeof(MeasPoint<Resistance>))]
                public override RangeStorage<PhysicalRange<Resistance>> Ranges { get; set; }

                #endregion
            }

            #endregion
        }

        #endregion

        #region Nested type: Resist4W

        public class Resist4W : MeasureFunction344XxBase<Resistance>
        {
            public Resist4W(IeeeBase inDevice) : base(inDevice)
            {
                RangeStorage = new RangeDevice();
            }

            /// <inheritdoc />
            protected override string GetFunctionName()
            {
                return "FRES";
            }

            public override void Setting()
            {
                base.Setting();
                Device.WriteLine($"{GetFunctionName()}:NPLC 100");
            }

            #region Nested type: RangeDevice

            public class RangeDevice : RangeDeviceBase<Resistance>
            {
                #region Property

                [AccRange("Mode: Ohms 4W", typeof(MeasPoint<Resistance>))]
                public override RangeStorage<PhysicalRange<Resistance>> Ranges { get; set; }

                #endregion
            }

            #endregion
        }

        #endregion

        /// <inheritdoc />
        public bool IsFrontTerminal
        {
            get => IsFrontTerminalActive();
        }

        protected enum Terminals344xx
        {
            [StringValue("FRON")] Front,
            [StringValue("REAR")] Rear
        }
        protected bool IsFrontTerminalActive()
        {

            string answer = _device.QueryLine("ROUT:TERM?");
            return answer.Equals(Terminals344xx.Front.GetStringValue());
        }
    }
}