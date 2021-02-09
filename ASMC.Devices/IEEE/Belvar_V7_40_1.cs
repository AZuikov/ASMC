using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.Multimetr.Mode;
using ASMC.Devices.Interface.SourceAndMeter;

namespace ASMC.Devices.IEEE
{
   public class Belvar_V7_40_1 :  IAcVoltage, IDcVoltage, IDcCurrent, IAcCurrent, IResistance2W, IProtocolStringLine
    {
        public IMeterPhysicalQuantity<Current> AcVoltage { get; }
        public IMeterPhysicalQuantity<Voltage> DcVoltage { get; }
        public IMeterPhysicalQuantity<Current> DcCurrent { get; }
        public IMeterPhysicalQuantity<Current> AcCurrent { get; }
        public IMeterPhysicalQuantity<Resistance> Resistance2W { get; }
        public string UserType { get; }
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

   public abstract class MeasureFunctionV_7_40_1Base<T> : IMeterPhysicalQuantity<T>
       where T : class, IPhysicalQuantity<T>, new()
   {
       protected IeeeBase _device;
       protected string ActivateThisModeCommand;
       protected string FunctionName;
       protected string RangeCommand;

        public void Getting()
       {
           throw new NotImplementedException();
       }

       public void Setting()
       {
           throw new NotImplementedException();
       }

       public IRangePhysicalQuantity<T> RangeStorage { get; }
       public MeasPoint<T> GetValue()
       {
           string readStr = _device.ReadRawString(12); //одна посылка 12 байт
       }

       public MeasPoint<T> Value { get; }
   }
}
