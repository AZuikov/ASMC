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
using ASMC.Devices.Interface.Multimetr.Mode;
using static ASMC.Devices.HelpDeviceBase;
using IAcCurrent = ASMC.Devices.Interface.Multimetr.Mode.IAcCurrent;
using IAcVoltage = ASMC.Devices.Interface.Multimetr.Mode.IAcVoltage;
using IDcCurrent = ASMC.Devices.Interface.Multimetr.Mode.IDcCurrent;
using IDcVoltage = ASMC.Devices.Interface.Multimetr.Mode.IDcVoltage;
using IResistance2W = ASMC.Devices.Interface.Multimetr.Mode.IResistance2W;

namespace ASMC.Devices.IEEE
{
    public class Belvar_V7_40_1 : IAcVoltageComplexPhysicalQuantity, IDcVoltage, IDcCurrent, IAcCurrentComplexPhysicalQuantity, IResistance2W, IProtocolStringLine
    {
        public IMeterPhysicalQuantity<Voltage,Frequency> AcVoltage { get; }
        public IMeterPhysicalQuantity<Voltage> DcVoltage { get; }
        public IMeterPhysicalQuantity<Current> DcCurrent { get; }
        public IMeterPhysicalQuantity<Current,Frequency> AcCurrent { get; }
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
            AcVoltage = new AcVolt(_device);
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
                deviceName= deviceName.Replace(invalidSymbol, '_');
            }
            var fileName = @$"{Environment.CurrentDirectory}\acc\{deviceName}.acc";

            if (!File.Exists(fileName)) return;
            this.FillRangesDevice(fileName);
        }

        public string StringConnection { get; set; }
    }

    public class DcVolt : MeasureFunctionV_7_40_1_SimplePhysicalQuantity<Voltage>
    {
        public DcVolt(IeeeBase inDevice) : base(inDevice, MeasureFunctionCode.Dcv)
        {
            RangeStorage = new RangeDevice();
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

    public class AcVolt : MeasureFunctionV_7_40_1_ComplexPhysicalQuantity<Voltage,Frequency>
    {
        public AcVolt(IeeeBase inDevice) : base(inDevice, MeasureFunctionCode.Acv)
        {
            RangeStorage = new RangeDevice();
            FunctionCodes = MeasureFunctionCode.Dcv;
            FunctionName = "Измерение переменного напряжения";
            allRangesThisMode = new[]
            {
                RangeCodes.Range200mV,
                RangeCodes.Range2V,
                RangeCodes.Range20V,
                RangeCodes.Range200V,
                RangeCodes.Range2000V,

            };
        }

        public class RangeDevice : RangeDeviceBase<Voltage,Frequency>
        {
            [AccRange("Mode: Volt AC", typeof(MeasPoint<Voltage,Frequency>))]
            public override RangeStorage<PhysicalRange<Voltage,Frequency>> Ranges { get; set; }
        }
    }

    public class Resist2W: MeasureFunctionV_7_40_1_SimplePhysicalQuantity<Resistance>
    {
        public Resist2W(IeeeBase inDevice) : base(inDevice, MeasureFunctionCode.Resist)
        {
            RangeStorage = new RangeDevice();
            FunctionName = "Измерение электрического сопротивления";
            allRangesThisMode = new[]
            {
                RangeCodes.Range200Ohm,
                RangeCodes.Range2kOhm,
                RangeCodes.Range20kOhm,
                RangeCodes.Range200kOhm,
                RangeCodes.Range2000kOhm,
                RangeCodes.Range20MOhm

            };
        }

        public class RangeDevice : RangeDeviceBase<Resistance>
        {
            [AccRange("Mode: Ohms 2W", typeof(MeasPoint<Resistance>))]
            public override RangeStorage<PhysicalRange<Resistance>> Ranges { get; set; }
        }
    }

    public class DcCurr : MeasureFunctionV_7_40_1_SimplePhysicalQuantity<Current>
    {
        public DcCurr(IeeeBase inDevice) : base(inDevice, MeasureFunctionCode.Dci)
        {
            RangeStorage = new RangeDevice();
            FunctionName = "Измерение постоянного тока";
            allRangesThisMode = new[]
            {
                RangeCodes.Range200uA,
                RangeCodes.Range2mA,
                RangeCodes.Range20mA,
                RangeCodes.Range200mA,
                RangeCodes.Range2000mA,

            };
        }

        public class RangeDevice : RangeDeviceBase<Current>
        {
            [AccRange("Mode: DC Current", typeof(MeasPoint<Current>))]
            public override RangeStorage<PhysicalRange<Current>> Ranges { get; set; }
        }
    }

    public class AcCurr : MeasureFunctionV_7_40_1_ComplexPhysicalQuantity<Current,Frequency>
    {
        public AcCurr(IeeeBase inDevice) : base(inDevice, MeasureFunctionCode.Aci)
        {
            RangeStorage = new RangeDevice();
            FunctionName = "Измерение переменного тока";
            allRangesThisMode = new[]
            {
                RangeCodes.Range200uA,
                RangeCodes.Range2mA,
                RangeCodes.Range20mA,
                RangeCodes.Range200mA,
                RangeCodes.Range2000mA,

            };
        }

        public class RangeDevice : RangeDeviceBase<Current, Frequency>
        {
            [AccRange("Mode: AC Current", typeof(MeasPoint<Current, Frequency>))]
            public override RangeStorage<PhysicalRange<Current, Frequency>> Ranges { get; set; }
        }
    }


    public abstract class MeasureFunctionV_7_40_1_SimplePhysicalQuantity<T> : MeasureFunctionV_7_40_1Base, IMeterPhysicalQuantity<T> where T : class, IPhysicalQuantity<T>, new()
    {
       

        public IRangePhysicalQuantity<T> RangeStorage { get; protected set; }

        public MeasPoint<T> GetValue()
        {
            decimal value = GetDecimalValFromDevice();
            Value = new MeasPoint<T>(value);
            return Value;
        }

        public MeasPoint<T> Value { get; protected set; }


        protected MeasureFunctionV_7_40_1_SimplePhysicalQuantity(IeeeBase inDevice, MeasureFunctionCode function) : base(inDevice, function)
        {
        }

        public void Setting()
        {
            string firstCommandPart = $"{BeginCommand}{(int)FunctionCodes}B";
            int rangeNumb = (int)RangeCodes.AutoRange;//на всякий случай устанавливаем автоматический выбор предела измерения, если не удасться выбрать подходящий ниже.

            foreach (RangeCodes multRange in allRangesThisMode)
            {
                //todo проверить работоспособность!!!
                if (RangeStorage.SelectRange.End.MainPhysicalQuantity.Value == (decimal) multRange.GetDoubleValue())
                {
                    rangeNumb = (int)multRange;
                    break;
                }
            }

            string coomandToSend = $"{firstCommandPart}{rangeNumb}{EndCommand}";
            _device.WriteLine(coomandToSend);
        }
    }

    public abstract class
        MeasureFunctionV_7_40_1_ComplexPhysicalQuantity<T, T1> : MeasureFunctionV_7_40_1Base, IMeterPhysicalQuantity<T,T1>
        where T : class, IPhysicalQuantity<T>, new()  where T1 : class, IPhysicalQuantity<T1>, new()
    {
        protected MeasureFunctionV_7_40_1_ComplexPhysicalQuantity(IeeeBase inDevice, MeasureFunctionCode function) : base(inDevice, function)
        {
        }

        public void Setting()
        {
            throw new NotImplementedException();
        }

        public MeasPoint<T> GetValue()
        {
            decimal val = GetDecimalValFromDevice();
            Value = new MeasPoint<T>(val);
            return Value;
        }

        public MeasPoint<T> Value { get; protected set; }
        public IRangePhysicalQuantity<T, T1> RangeStorage { get; protected set; }
    }

    public abstract class MeasureFunctionV_7_40_1Base
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

        protected readonly string BeginCommand = "F";//символ измерительной функиции
        protected readonly string EndCommand = "D0E";//окончание команды
        /// <summary>
        /// Все пределы измерения, доступные в данном режиме.
        /// </summary>
        protected RangeCodes[] allRangesThisMode;

        public MeasureFunctionV_7_40_1Base(IeeeBase inDevice, MeasureFunctionCode function)
        {
            _device = inDevice;
            FunctionCodes = function;
            
        }

        public void Getting()
        {
            //получаем информацию о режиме измерения, пределе измерения, измеренное значение
            throw new NotImplementedException();
        }

       

        protected decimal GetDecimalValFromDevice()
        {
            string readStr = _device.ReadRawString(12); //одна посылка 12 байт
            Regex regex = new Regex(@"[-+\S]\d*E[-+]\d");
            readStr = regex.Match(readStr).Value;
            decimal value = (decimal)StrToDouble(readStr);
            return value;
        }

        protected enum RangeCodes
        {
            [DoubleValue(20000000)]Range20MOhm = 0,
            [DoubleValue(2000)]Range2000V = 1,
            [DoubleValue(200)]Range200V = 2,
            [DoubleValue(20)]Range20V = 3,
            [DoubleValue(2)]Range2V = 4,
            [DoubleValue(0.2)]Range200mV = 5,
            [DoubleValue(0)]AutoRange = 7,
            [DoubleValue(0.0002)]Range200uA = Range200mV,
            [DoubleValue(0.002)]Range2mA = Range2V,
            [DoubleValue(0.02)]Range20mA = Range20V,
            [DoubleValue(0.2)]Range200mA = Range200V,
            [DoubleValue(2)]Range2000mA = Range2000V,
            [DoubleValue(200)]Range200Ohm = Range200mV,
            [DoubleValue(2000)]Range2kOhm = Range2V,
            [DoubleValue(20000)]Range20kOhm = Range20V,
            [DoubleValue(200000)]Range200kOhm = Range200V,
            [DoubleValue(2000000)] Range2000kOhm = Range2000V,

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