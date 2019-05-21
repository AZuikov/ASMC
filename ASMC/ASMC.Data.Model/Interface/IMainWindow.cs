using System.Collections.Generic;

namespace ASMC.Data.Model.Interface
{
    public interface IMainWindow
    {
        int BarcodeValue { set; }
        IEnumerable<IParametrs> Parameters { get; set; }
    }

    
}
