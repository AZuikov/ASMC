using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Devices.IEEE.Keysight.ElectronicLoad
{
    public interface IElectronicLoad
    {
        /// <summary>
        /// Устанавливает выбранный на нагрузке модуль в качестве рабочего (активного).
        /// </summary>
        void SetThisModuleAsWorking();
        void OutputOn();
        void OutputOff();
        
        void SetResistanceMode();
        MeasPoint<Resistance> GetResistnceLevel();
        void SetResistanceLevel(MeasPoint<Resistance> inPoint);

        void SetVoltageMode();
        MeasPoint<Voltage> GetVoltageLevel();
        void SetVoltageLevel(MeasPoint<Voltage> inPoint);

        void SetCurrentMode();
        MeasPoint<Current> GetCurrentLevel();
        void SetCurrentLevel(MeasPoint<Current> inPoint);

        MeasPoint<Current> GetMeasureCurrent();
        MeasPoint<Voltage> GetMeasureVoltage();

    }
}
