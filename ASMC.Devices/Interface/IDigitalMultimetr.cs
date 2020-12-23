using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.Interface
{
    /// <summary>
    /// Предоставляет интерфейс цифрового мультиметра.
    /// </summary>
    interface IDigitalMultimetr
    {
        MeasPoint<Voltage> GetMeasureDcVoltage();
        void SetRangeDcVoltage(MeasPoint<Voltage> pointInRange);
        MeasPoint<Voltage> GetMeasureAcVoltage();
        void SetRangeAcVoltage(MeasPoint<Voltage> pointInRange);
        MeasPoint<Current> GetMeasureDcCurrent();
        void SetRangeDcCurrent(MeasPoint<Current> pointInRange);
        MeasPoint<Current> GetMeasureAcCurrent();
        void SetRangeAcCurrent(MeasPoint<Current> pointInRange);
        MeasPoint<Resistance> GetMeasureResistance();
        void SetRangeResistance(MeasPoint<Resistance> pointInRange);
        MeasPoint<Resistance> GetMeasureResistance4W();
        void SetRangeResistance4W(MeasPoint<Resistance> pointInRange);
        MeasPoint<Frequency> GetMeasureFrequency();
        MeasPoint<Capacity> GetMeasureCapacity();
        void SetRangeCapacity(MeasPoint<Capacity> pointInRange);

    }
}
