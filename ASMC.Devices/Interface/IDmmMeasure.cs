using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

namespace ASMC.Devices.Interface.Multimetr.Mode
{
   

    public interface IMeasureMode<T> 
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

    

    public interface IDc
    {
        IMeasureMode<MeasPoint<Voltage>> DcVoltage { get; set; }
    }
    public interface IDcCurrent
    {
        IMeasureMode<MeasPoint<Current>> DcCurrent { get; set; }
    }


    public interface IAcVoltage
    {
        IMeasureMode<MeasPoint<Voltage, Frequency>> AcVoltage { get; set; }
    }
    public interface IAcCurrent
    {
        IMeasureMode<MeasPoint<Current, Frequency>> AcCurrent { get; set; }

    }

    
    public interface IResistance: IResistance2W
    {

    }
    
    
    public interface IGroupResistance
    {
        IResistance Resistance { get; set; }
    }
    public interface IResistance2W
    {
        IMeasureMode<MeasPoint<Resistance>> Resistance2W { get; set; }
    }
    public interface IResistance4W
    {
        IMeasureMode<MeasPoint<Resistance>> Resistance4W { get; set; }
    }

    public interface ICapacity
    {
        IMeasureMode<MeasPoint<Capacity>> Capacity { get; set; }
    }

    public interface IFrequency
    {
        IMeasureMode<MeasPoint<Frequency>> Frequency { get; set; }
    }

    public interface ITime
    {
        IMeasureMode<MeasPoint<Time>> Time { get; set; }
    }

    public interface ITemperature
    {
        IMeasureMode<MeasPoint<Temperature>> Temperature { get; set; }
    }



}
