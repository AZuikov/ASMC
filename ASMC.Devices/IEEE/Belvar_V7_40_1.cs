using AP.Extension;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.SourceAndMeter;
using ASMC.Devices.Model;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static ASMC.Devices.HelpDeviceBase;
using IAcCurrent = ASMC.Devices.Interface.Multimetr.Mode.IAcCurrent;
using IAcVoltage = ASMC.Devices.Interface.Multimetr.Mode.IAcVoltage;
using IDcCurrent = ASMC.Devices.Interface.Multimetr.Mode.IDcCurrent;
using IDcVoltage = ASMC.Devices.Interface.Multimetr.Mode.IDcVoltage;
using IResistance2W = ASMC.Devices.Interface.Multimetr.Mode.IResistance2W;

namespace ASMC.Devices.IEEE
{
    public class Belvar_V7_40_1 : IAcVoltage, IDcVoltage, IDcCurrent, IAcCurrent, IResistance2W, IProtocolStringLine
    {
        public IMeterPhysicalQuantity<Voltage> AcVoltage { get; }
        public IMeterPhysicalQuantity<Voltage> DcVoltage { get; }
        public IMeterPhysicalQuantity<Current> DcCurrent { get; }
        public IMeterPhysicalQuantity<Current> AcCurrent { get; }
        public IMeterPhysicalQuantity<Resistance> Resistance2W { get; }
        public string UserType { get; }

        #region Field

        private readonly IeeeBase _device;

        #endregion Field

        public Belvar_V7_40_1()
        {
            _device = new IeeeBase();
            DcVoltage = new DcVolt(_device);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool IsTestConnect { get; }

        public async Task InitializeAsync()
        {
            string deviceName = (string)UserType.Clone();
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var invalidSymbol in invalidChars)
            {
                deviceName.Replace(invalidSymbol, '_');
            }
            var fileName = @$"{Environment.CurrentDirectory}\acc\{deviceName}.acc";

            if (!File.Exists(fileName)) return;
            this.FillRangesDevice(fileName);
        }

        public string StringConnection { get; set; }
    }

    public class DcVolt : MeasureFunctionV_7_40_1Base<Voltage>
    {
        public DcVolt(IeeeBase inDevice) : base(inDevice, MeasureFunctionCode.Dcv)
        {
            RangeStorage = new RangeDevice();
        }

        public class RangeDevice : RangeDeviceBase<Voltage>
        {
            [AccRange("Mode: Volt DC", typeof(MeasPoint<Voltage>))]
            public override RangeStorage<PhysicalRange<Voltage>> Ranges { get; set; }
        }
    }

    public abstract class MeasureFunctionV_7_40_1Base<T> : IMeterPhysicalQuantity<T> where T : class, IPhysicalQuantity<T>, new()
    {
        protected IeeeBase _device;

        protected string FunctionName;

        public RangeCodes rangeCode { get; set; }
        protected MeasureFunctionCode FunctionCodes { get; set; }

        private readonly string EndCommand = "D0E";

        public MeasureFunctionV_7_40_1Base(IeeeBase inDevice, MeasureFunctionCode function)
        {
            _device = inDevice;
            FunctionCodes = function;
            FunctionName = FunctionCodes.GetStringValue();
        }

        public void Getting()
        {
            throw new NotImplementedException();
        }

        public void Setting()
        {
        }

        public IRangePhysicalQuantity<T> RangeStorage { get; protected set; }

        public MeasPoint<T> GetValue()
        {
            string readStr = _device.ReadRawString(12); //одна посылка 12 байт
            Regex regex = new Regex(@"[-+\S]\d*E[-+]\d");
            readStr = regex.Match(readStr).Value;
            decimal value = (decimal)StrToDouble(readStr);
            Value = new MeasPoint<T>(value);
            return Value;
        }

        public MeasPoint<T> Value { get; protected set; }

        public enum RangeCodes
        {
            Range20M = 0,
            Range2000 = 1,
            Range200 = 2,
            Range20 = 3,
            Range2 = 4,
            Range200m = 5,
            AutoRange = 7
        }

        public enum MeasureFunctionCode
        {
            [StringValue("Измерение постоянного тока")] Dci = 1,
            [StringValue("Измерение электрического сопротивления")] Resist = 2,
            [StringValue("Измерение переменного тока")] Aci = 3,
            [StringValue("Измерение постоянного напряжения")] Dcv = 4,
            [StringValue("Измерение переменного напряжения")] Acv = 6
        }
    }
}