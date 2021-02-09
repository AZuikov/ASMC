using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.Multimetr.Mode;
using ASMC.Devices.Interface.SourceAndMeter;
using static ASMC.Devices.HelpDeviceBase;

namespace ASMC.Devices.IEEE
{
   public class Belvar_V7_40_1 :  IAcVoltage, IDcVoltage, IDcCurrent, IAcCurrent, IResistance2W, IProtocolStringLine
    {
        public IMeterPhysicalQuantity<Voltage> AcVoltage { get; }
        public IMeterPhysicalQuantity<Voltage> DcVoltage { get; }
        public IMeterPhysicalQuantity<Current> DcCurrent { get; }
        public IMeterPhysicalQuantity<Current> AcCurrent { get; }
        public IMeterPhysicalQuantity<Resistance> Resistance2W { get; }
        public string UserType { get; }

        #region Field

        private readonly IeeeBase _device;

        #endregion

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
            throw new NotImplementedException();
        }

        public string StringConnection { get; set; }
    }

  

   public class DcVolt : MeasureFunctionV_7_40_1Base<Voltage>
   {
       public FunctionAndRanges[] dcv = new[]
       {
           new FunctionAndRanges{FunctionCode = FunctionAndRanges.MeasureFunctionCode.Dcv, RangeCode = FunctionAndRanges.RangeCodes.Range2000}, 
           new FunctionAndRanges{FunctionCode = FunctionAndRanges.MeasureFunctionCode.Dcv, RangeCode = FunctionAndRanges.RangeCodes.Range200}, 
           new FunctionAndRanges{FunctionCode = FunctionAndRanges.MeasureFunctionCode.Dcv, RangeCode = FunctionAndRanges.RangeCodes.Range20}, 
           new FunctionAndRanges{FunctionCode = FunctionAndRanges.MeasureFunctionCode.Dcv, RangeCode = FunctionAndRanges.RangeCodes.Range2}, 
           new FunctionAndRanges{FunctionCode = FunctionAndRanges.MeasureFunctionCode.Dcv, RangeCode = FunctionAndRanges.RangeCodes.Range200m}, 
       };
       public DcVolt(IeeeBase inDevice) : base(inDevice, "Измерение постоянного напряжения")
       {
           
       }
   }

   public abstract class MeasureFunctionV_7_40_1Base<T> : IMeterPhysicalQuantity<T>
       where T : class, IPhysicalQuantity<T>, new()
   {
       protected IeeeBase _device;
       
       protected string FunctionName;
       
       readonly string EndCommand = "D0E";
       

       public MeasureFunctionV_7_40_1Base(IeeeBase inDevice, string functionName)
       {
           _device = inDevice;
           FunctionName = functionName;
           
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
           decimal value =(decimal)StrToDouble(readStr);
           Value = new MeasPoint<T>(value);
           return Value;

       }

       public MeasPoint<T> Value { get; protected set; }
   }

   public class FunctionAndRanges<T> where T : PhysicalQuantity<T>, new()
   {
       
       public FunctionAndRanges(MeasureFunctionCode function, RangeCodes range , MeasPoint<T> maxValue)
       {
           MaxValue = maxValue;
           FunctionCode = function;
           RangeCode = range;

       }

       public enum RangeCodes
       {
           Range20M = 0,
           Range2000 = 1,
           Range200 = 2,
           Range20 = 3,
           Range2 = 4,
           Range200m = 5,
           AVP = 7
       }

       public enum MeasureFunctionCode
       {
           Dci = 1,
           Resist = 2,
           Aci = 3,
           Dcv = 4,
           Acv = 6
       }

        public MeasureFunctionCode FunctionCode { get; protected set; }
       public RangeCodes RangeCode { get; protected set; }
       public MeasPoint<T> MaxValue { get; protected set; }

   }
}
