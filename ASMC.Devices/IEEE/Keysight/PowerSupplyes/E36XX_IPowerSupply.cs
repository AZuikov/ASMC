using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.IEEE.Keysight.PowerSupplyes
{
    /// <summary>
    /// Интерфейс источника питания серии E36 Keysight
    /// </summary>
    public interface E36XX_IPowerSupply
    {
        

        void SetVoltage(MeasPoint<Voltage> inPoint);
        void SetCurrent(MeasPoint<Current> inPoint);
        void OutputOn();
        void OutputOff();
        void SetHighVoltageRange( );
        void SetLowVoltageRange( );
        MeasPoint<Voltage> GetMeasureVoltage();
        MeasPoint<Current> GetMeasureCurrent();

    }
}
