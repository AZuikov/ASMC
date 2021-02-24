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
            UserType = "В7-40/1";
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
            FunctionCodes = MeasureFunctionCode.Dcv;
            FunctionName = "Измерение постоянного напряжения";
            allRangesThisMode = new[]
            {
                RangeCodes.Range200mV,
                RangeCodes.Range2V,
                RangeCodes.Range20V,
                RangeCodes.Range200V,
                RangeCodes.Range2000V,

            };
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

        /// <summary>
        /// Наимнование функции измерения физической величины.
        /// </summary>
        protected string FunctionName;
        
        /// <summary>
        /// Номер измерительной функции мультиметра
        /// </summary>
        protected MeasureFunctionCode FunctionCodes { get; set; }

        private readonly string BeginCommand = "F";//символ измерительной функиции
        private readonly string EndCommand = "D0E";//окончание команды
        /// <summary>
        /// Все пределы измерения, доступные в данном режиме.
        /// </summary>
        protected RangeCodes[] allRangesThisMode;

        public MeasureFunctionV_7_40_1Base(IeeeBase inDevice, MeasureFunctionCode function)
        {
            _device = inDevice;
            FunctionCodes = function;
            FunctionName = FunctionCodes.GetStringValue();
        }

        public void Getting()
        {
            //получаем информацию о режиме измерения, пределе измерения, измеренное значение
            throw new NotImplementedException();
        }

        public void Setting()
        {
        string firstCommandPart =$"{BeginCommand}{(int)FunctionCodes}B";
        string rangeNumb = RangeCodes.AutoRange.ToString();//на всякий случай устанавливаем автоматический выбор предела измерения, если не удасттся выбрать подходящий ниже.
        
        foreach (RangeCodes multRange in allRangesThisMode)
        {
            //todo проверить работоспособность!!!
            if (RangeStorage.SelectRange.End.MainPhysicalQuantity.Value == (decimal) multRange.GetDoubleValue())
                rangeNumb = multRange.ToString();
        }
        _device.WriteLine($"{firstCommandPart}{rangeNumb}{EndCommand}");
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
            Range20MOhm = 0,
            Range2000V = 1,
            Range200V = 2,
            Range20V = 3,
            Range2V = 4,
            Range200mV = 5,
            AutoRange = 7,
            Range200uA = Range200mV,
            Range2mA = Range2V,
            Range20mA = Range20V,
            Range200mA = Range200V,
            Range2000mA = Range2000V,
            Range200Ohm = Range200mV,
            Range2kOhm = Range2V,
            Range20kOhm = Range20V,
            Range200kOhm = Range200V,
            Range2000kOhm = Range2000V,

        }

        public enum MeasureFunctionCode
        {
            [StringValue("I")] Dci = 1,
            [StringValue("R")] Resist = 2,
            [StringValue("I")] Aci = 3,
            [StringValue("U")] Dcv = 4,
            [StringValue("V")] Acv = 6
        }
    }
}