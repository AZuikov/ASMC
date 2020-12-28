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
    interface IDmmAcMeasure
    {
        MeasPoint<Voltage> GetMeasureAcVoltage(MeasPoint<Voltage> valueToRangeSetup);
        void SetRangeAcVoltage(MeasPoint<Voltage> valueToRangeSetup);
        MeasPoint<Voltage> GetMeasureAcVoltage();

        MeasPoint<Current> GetMeasureAcCurrent(MeasPoint<Current> valueToRangeSetup);
        void SetRangeAcCurrent(MeasPoint<Current> valueToRangeSetup);
        MeasPoint<Current> GetMeasureAcCurrent();

        MeasPoint<Frequency> GetMeasureFrequency(MeasPoint<Frequency> valueToRangeSetup);
        MeasPoint<Frequency> GetMeasureFrequency();
        void SetRangeFrequency(MeasPoint<Frequency> valueToRangeSetup);

        MeasPoint<Capacity> GetMeasureCapacity(MeasPoint<Capacity> valueToRangeSetup);
        MeasPoint<Capacity> GetMeasureCapacity();
        void SetRangeCapacity(MeasPoint<Capacity> valueToRangeSetup);

    }

    interface IDmmBaseMeasure
    {
        MeasPoint<Voltage> GetMeasureVoltage();
        void SetRangeVoltage(MeasPoint<Voltage> valueToRangeSetup);
        MeasPoint<Current> GetMeasureDcCurrent();
        void SetRangeDcCurrent(MeasPoint<Current> valueToRangeSetup);
    }

    interface IdmmDcOnlyMeasure
    {
        MeasPoint<Resistance> GetMeasureResistance();
        void SetRangeResistance(MeasPoint<Resistance> valueToRangeSetup);
        MeasPoint<Resistance> GetMeasureResistance4W();
        void SetRangeResistance4W(MeasPoint<Resistance> valueToRangeSetup);
    }

    interface IDmmBase<T> where T : class, IPhysicalQuantity<T>, new()
    {
        void SetFunctionActive(ICommand command);
        void SetRange(MeasPoint<T> pointInRange);
        MeasPoint<T> GetMeasureValue();
    }

}
