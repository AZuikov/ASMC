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
        

        void SetVoltageLevel(MeasPoint<Voltage> inPoint);
        void SetMaxVoltageLevel();
        MeasPoint<Voltage> GetVoltageLevel();
        
        void SetCurrentLevel(MeasPoint<Current> inPoint);
        void SetMaxCurrentLevel();
        MeasPoint<Current> GetCurrentLevel();
        
        void OutputOn();
        void OutputOff();
        
        void SetHighVoltageRange();
        void SetLowVoltageRange();
        MeasPoint<Voltage> GetVoltageRange();
        
        MeasPoint<Voltage> GetMeasureVoltage();
        MeasPoint<Current> GetMeasureCurrent();

    }
}
