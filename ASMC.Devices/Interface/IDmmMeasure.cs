using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.Interface
{
   

    public interface IDigitalMultimetrMeasureMode<T> 
    {
        T Range { get; set; }

        /// <summary>
        /// Активирует режим измерения физ. величины.
        /// </summary>
        /// <param name="command"></param>
        void SetThisFunctionActive();
        
        /// <summary>
        /// Получить измеренное значение физической величины с прибора.
        /// </summary>
        /// <returns>Значение измеренной физ. величины.</returns>
        T GetMeasureValue();
    }

    public interface IDigitalMultimetrGroupMode
    {
        object[] mode { get; set; }
    }

    public interface IDigitalMultimetrModeDc
    {
        IDigitalMultimetrMeasureMode<MeasPoint<Voltage>> DcVoltage { get; set; }

        IDigitalMultimetrMeasureMode<MeasPoint<Current>> DcCurrent { get; set; }
    }

   
    public interface IDigitalMultimetrModeAc
    {
        IDigitalMultimetrMeasureMode<MeasPoint<Voltage, Frequency>> AcVoltage { get; set; }
        IDigitalMultimetrMeasureMode<MeasPoint<Current, Frequency>> AcCurrent { get; set; }

    }

   

    public interface IDigitalMultimetrModeResistance
    {
        IDigitalMultimetrMeasureMode<MeasPoint<Resistance>> Resistance2W { get; set; }
        IDigitalMultimetrMeasureMode<MeasPoint<Resistance>> Resistance4W { get; set; }
    }

   


}
