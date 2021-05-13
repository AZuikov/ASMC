using AP.Extension;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.Multimetr.Mode;
using ASMC.Devices.Interface.SourceAndMeter;
using ASMC.Devices.Model;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static ASMC.Devices.HelpDeviceBase;
using IDcCurrent = ASMC.Devices.Interface.Multimetr.Mode.IDcCurrent;
using IDcVoltage = ASMC.Devices.Interface.Multimetr.Mode.IDcVoltage;
using IResistance2W = ASMC.Devices.Interface.Multimetr.Mode.IResistance2W;

namespace ASMC.Devices.IEEE
{
    public class Belvar_V7_40_1 : IAcVoltageComplexPhysicalQuantity, IDcVoltage, IDcCurrent, IAcCurrentComplexPhysicalQuantity, IResistance2W, IProtocolStringLine
    {
        public IMeterPhysicalQuantity<Voltage, Frequency> AcVoltage { get; }
        public IMeterPhysicalQuantity<Voltage> DcVoltage { get; }
        public IMeterPhysicalQuantity<Current> DcCurrent { get; }
        public IMeterPhysicalQuantity<Current, Frequency> AcCurrent { get; }
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
            Resistance2W = new Resist2W(_device);
            DcCurrent = new DcCurr(_device);
            AcCurrent = new AcCurr(_device);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool IsTestConnect { get; }

        public async void Initialize()
        {
            string deviceName = (string)UserType.Clone();
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var invalidSymbol in invalidChars)
            {
                deviceName = deviceName.Replace(invalidSymbol, '_');
            }
            var fileName = @$"{Environment.CurrentDirectory}\acc\{deviceName}.acc";

            if (!File.Exists(fileName)) return;
            this.FillRangesDevice(fileName);
        }

        public string StringConnection
        {
            get => _device.StringConnection;
            set => _device.StringConnection = value;
        }
    }

    public class DcVolt : MeasureFunctionV_7_40_1_SimplePhysicalQuantity<Voltage>
    {
        public DcVolt(IeeeBase inDevice) : base(inDevice, MeasureFunctionCode.Dcv)
        {
            RangeStorage = new RangeDevice();
            FunctionName = "Измерение постоянного напряжения";
            allRangesThisMode = new[]
            {
                new Command("2000 В","1",2000),
                new Command("200 В","2",200),
                new Command("20 В","3",20),
                new Command("2 В","4",2),
                new Command("200 мВ","5",0.200),
                new Command("авто выбор предела","7",0)
            };
        }

        public class RangeDevice : RangeDeviceBase<Voltage>
        {
            [AccRange("Mode: Volt DC", typeof(MeasPoint<Voltage>))]
            public override RangeStorage<PhysicalRange<Voltage>> Ranges { get; set; }
        }
    }

    public class AcVolt : MeasureFunctionV_7_40_1_ComplexPhysicalQuantity<Voltage, Frequency>
    {
        public AcVolt(IeeeBase inDevice) : base(inDevice, MeasureFunctionCode.Acv)
        {
            RangeStorage = new RangeDevice();
            FunctionCodes = MeasureFunctionCode.Acv;
            FunctionName = "Измерение переменного напряжения";
            allRangesThisMode = new[]
            {
                new Command("2000 В","1",2000),
                new Command("200 В","2",200),
                new Command("20 В","3",20),
                new Command("2 В","4",2),
                new Command("200 мВ","5",0.2),
                new Command("автовыбор предела","7",0)
            };
        }

        public class RangeDevice : RangeDeviceBase<Voltage, Frequency>
        {
            [AccRange("Mode: Volt AC", typeof(MeasPoint<Voltage, Frequency>))]
            public override RangeStorage<PhysicalRange<Voltage, Frequency>> Ranges { get; set; }
        }
    }

    public class Resist2W : MeasureFunctionV_7_40_1_SimplePhysicalQuantity<Resistance>
    {
        public Resist2W(IeeeBase inDevice) : base(inDevice, MeasureFunctionCode.Resist)
        {
            RangeStorage = new RangeDevice();
            FunctionName = "Измерение электрического сопротивления";
            allRangesThisMode = new[]
            {
                new Command("20 МОм","0",20000000),
                new Command("2000 кОм","1",2000000),
                new Command("200 кОм","2",200000),
                new Command("20 кОм","3",20000),
                new Command("2 кОм","4",2000),
                new Command("200 Ом","5",200),
                new Command("автовыбор предела","7",0)
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
                new Command("2000 мА","1",2),
                new Command("200 мА","2",0.200),
                new Command("20 мА","3",0.020),
                new Command("2 мА","4",0.002),
                new Command("200 мкА","5",0.000200),
                new Command("автовыбор предела","7",0)
            };
        }

        public class RangeDevice : RangeDeviceBase<Current>
        {
            [AccRange("Mode: DC Current", typeof(MeasPoint<Current>))]
            public override RangeStorage<PhysicalRange<Current>> Ranges { get; set; }
        }
    }

    public class AcCurr : MeasureFunctionV_7_40_1_ComplexPhysicalQuantity<Current, Frequency>
    {
        public AcCurr(IeeeBase inDevice) : base(inDevice, MeasureFunctionCode.Aci)
        {
            RangeStorage = new RangeDevice();
            FunctionName = "Измерение переменного тока";
            allRangesThisMode = new[]
            {
                new Command("2000 мА","1",2),
                new Command("200 мА","2",0.200),
                new Command("20 мА","3",0.020),
                new Command("2 мА","4",0.002),
                new Command("200 мкА","5",0.0002),
                new Command("автовыбор предела","7",0)
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
            var rangeNumb = new Command("автовыбор предела", "7", 0);//на всякий случай устанавливаем автоматический выбор предела измерения, если не удасться выбрать подходящий ниже.

            foreach (var multRange in allRangesThisMode)
            {
                if (RangeStorage.SelectRange.End.MainPhysicalQuantity.Value == (decimal)multRange.Value)
                {
                    rangeNumb = multRange;
                    break;
                }
            }

            string coomandToSend = $"{firstCommandPart}{rangeNumb.Description}{EndCommand}";
            _device.WriteLine(coomandToSend);
        }
    }

    public abstract class
        MeasureFunctionV_7_40_1_ComplexPhysicalQuantity<T, T1> : MeasureFunctionV_7_40_1Base, IMeterPhysicalQuantity<T, T1>
        where T : class, IPhysicalQuantity<T>, new() where T1 : class, IPhysicalQuantity<T1>, new()
    {
        protected MeasureFunctionV_7_40_1_ComplexPhysicalQuantity(IeeeBase inDevice, MeasureFunctionCode function) : base(inDevice, function)
        {
        }

        public void Setting()
        {
            string firstCommandPart = $"{BeginCommand}{(int)FunctionCodes}B";
            var rangeNumb = new Command("авто выбор предела", "7", 0);//на всякий случай устанавливаем автоматический выбор предела измерения, если не удасться выбрать подходящий ниже.

            if (RangeStorage.SelectRange != null)// если null значит при выборе предела измерения в файле точности ничего подходящего не нашлось
            {
                foreach (var multRange in allRangesThisMode)
                {
                    //todo проверить работоспособность!!!
                    if (RangeStorage.SelectRange.End.MainPhysicalQuantity.Value == (decimal)multRange.Value)
                    {
                        rangeNumb = multRange;
                        break;
                    }
                }
            }

            string coomandToSend = $"{firstCommandPart}{rangeNumb.Description}{EndCommand}";
            _device.WriteLine(coomandToSend);
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
        protected Command[] allRangesThisMode
        {
            get
            {
                return ranges;
            }
            set
            {
                ranges = value;
            }
        }

        private Command[] ranges;

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
            string readStr = "";
            for (int i = 0; i <= 15; i++)//если уж 10 раз неудачное считывание, то проблема в приборе
            {
                readStr = _device.ReadRawString(12); //одна посылка 12 байт
                if (readStr.Contains("<") || readStr.Contains(">"))//значит там превышение предела измерения и нужно еще подождать
                {
                    Thread.Sleep(1000);
                    continue;
                }
                if (readStr.Length == 12) break;
            }
            if (readStr.Length != 12) throw new IOException($"{_device.UserType} считанное значение имеет неверный формат: [{readStr}]");

            Regex regex = new Regex(@"[-+\S]\d*E[-+]\d");
            readStr = regex.Match(readStr).Value;
            decimal value = (decimal)StrToDouble(readStr);
            return value;
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