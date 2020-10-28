using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Data.Model
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AccRangeAttribute : Attribute
    {
        public string Mode;
        public Type MeasPointType;
        public AccRangeAttribute(string mode, Type measPointType)
        {
            Mode = mode;
            MeasPointType = measPointType;
        }
    }
}
