using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model;

namespace ASMC.Devices.Interface
{
    public interface ICounter: IReferenceClock, IProtocolStringLine
    {
    }
}
