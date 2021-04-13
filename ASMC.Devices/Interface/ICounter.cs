using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.SourceAndMeter;

namespace ASMC.Devices.Interface
{
    public interface ICounter: IReferenceClock, IProtocolStringLine
    {
    }

    public interface ICounterInput<TPhysicalQuantity> :IProtocolStringLine, IMeterPhysicalQuantity<TPhysicalQuantity> 
        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
    {
        public string NameOfChanel { get; }
    }


}
