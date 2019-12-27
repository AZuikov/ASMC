using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model.Devises.Parametr;

namespace ASMC.Data.Model.Interface
{
    public interface IParametrScada : IMeasuredParametr
    {
        string DatebaseName { get; set; }
        string Procedure { get; set; }
        Tuple<string, object>[] Parameters { get; set; }
    }
}
