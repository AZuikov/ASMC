using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE.Keysight.PowerSupplies;

namespace ASMC.Devices.IEEE.Keysight.PowerSupplyes
{
    /// <summary>
    /// Интерфейс источника питания серии E364x Keysight
    /// </summary>
    public interface E36XX_IPowerSupply
    {
       

        #region Methods

        MeasPoint<Current> GetCurrentLevel();

        MeasPoint<Current> GetMeasureCurrent();

        MeasPoint<Voltage> GetMeasureVoltage();

        MeasPoint<Voltage> GetVoltageLevel();

        MeasPoint<Voltage> GetVoltageRange();

        void OutputOff();

        void OutputOn();

        void SetCurrentLevel(MeasPoint<Current> inPoint);

        void SetMaxCurrentLevel();

        void SetMaxVoltageLevel();

        
        void SetRange(MeasPoint<Voltage, Current> inRange);
        /// <summary>
        /// Текущий предел источника.
        /// </summary>
        /// <returns>Точка, содержащая напряжение и ток.</returns>
        MeasPoint<Voltage, Current> GetRange();
        /// <summary>
        /// Массив возможных пределов для источника питания.
        /// </summary>
        /// <returns>Все возможные предлеы (зависят от модели).</returns>
        MeasPoint<Voltage, Current>[] GetAllRanges();

        void SetVoltageLevel(MeasPoint<Voltage> inPoint);

        #endregion Methods
    }
}