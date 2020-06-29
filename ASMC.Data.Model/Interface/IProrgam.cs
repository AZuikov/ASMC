using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Data.Model.Interface
{
    public interface IProrgam    
    {
        string Type
        {
            get; set;
        }
        string Grsi
        {
            get; set;
        }
        string Range
        {
            get;
        }

        string Accuracy
        {
            get;
        }
        AbstraktOperation AbstraktOperation
        {
            get;
        }
    }
}
