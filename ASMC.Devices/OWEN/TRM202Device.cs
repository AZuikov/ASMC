using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Devices.OWEN
{
    class TRM202Device : OwenProtocol
    {
        /// <summary>
        /// Тип входного датчика или сигнала для входа 1 (2)
        /// </summary>
        enum in_t
        {
            r385 = 1,
            r_385 ,
            r391  ,
            r_391 ,
            r_21  ,
            r426  ,
            r_426 ,
            r_23  ,
            r428  ,
            r_428 ,
            E_A1  ,
            E_A2  ,
            E_A3  ,
            E__b  ,
            E__j  ,
            E__K  ,
            E__L  ,
            E__n  ,
            E__r  ,
            E__S  ,
            E__t  ,
            i0_5  ,
            i0_20 ,
            i4_20 ,
            U_50  ,
            U0_1
        }

        public TRM202Device()
        {
            
        }
    }
}
