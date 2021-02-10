using System;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE;

namespace ASMC.Devices.Interface
{
    /// <summary>
    /// Интерфейс осциллографа.
    /// </summary>
    public interface IOscilloscope
    {
        public  IOscillChanel[] Chanels { get; }
        /// <summary>
        /// Выбирает канал, с которым дальше работаем
        /// </summary>
        void ChanelOn(int ChanelNumber);
        void ChanelOff(int ChanelNumber);
        

    }
    /// <summary>
    /// Интерфейс канала осциллографа.
    /// </summary>
   public interface IOscillChanel:IOscillCoupling, IDeviceExtendFunc
    {
        public int Number { get;  }
       public bool IsEnable { get; set; }
       public MeasPoint<Voltage> VerticalOffset { get; set; }
       public MeasPoint<Voltage> VerticalScale { get; set; }
       public MeasPoint<Resistance> Impedance { get; set; }
       
       public int Probe { get; set; }

       

    }

   /// <summary>
   /// Полоса пропускания.
   /// </summary>
   public interface IOscillBandWidth
   {
       MeasPoint<Frequency> BandWidth { get; set; }
   }

   public interface IOscillCoupling
   {
        public Coupling coupling { get; set; }
   }

   public enum Coupling
   {
        AC,
        DC,
        AC_DC,
        GND
   }

   public interface IOscillCursorMeasure
   {
        public IOscillChanel _chanelSource { get; set; }
   }

   public interface IOscillBaseMeasure
   {
       

       /// <summary>
       /// Получение значениея измеренного параметра с канала осциллографа.
       /// </summary>
       /// <param name="inChanel">Номер канала осциллографа.</param>
       /// <param name="inMeasParam">Наименование измереяемого параметра</param>
       /// <param name="measNum">Номер измерения канала (опционально, может не применятся у конкретного осциллографа).</param>
       /// <returns></returns>
       IMeasPoint<T> GetParametr<T>(IOscillChanel inChanel, MeasParam inMeasParam, int? measNum) where T : class, IPhysicalQuantity, new();
      
   }

   public enum CursorFunctionType
   {
       HORizontal,
       VERTical,
       PAIRed,
       HRATio,
       VRATio, 
       PPCount,
       NPCount,
       RECount, 
       FECount, 
       MEAN, 
       RMS, 
       RTIMe, 
       FTIMe,
       PEAK, 
       UPEakvalue, 
       LPEakvalue, 
       BWIDth
    }

   public class MeasParam : Command
   {
       public MeasParam(string inStrCommand, string inDescription, double value, Type type) : base(inStrCommand, inDescription, value)
       {
           Type = type;
       }
       public Type Type { get; }

   }

   public enum TriggerType
   {
       EDGE,
       WIDTh,
       TV,
       BUS,
       LOGic
    }

   public interface IOscTrigger: IDeviceExtendFunc
    {
      
        IOscillChanel SourceChanel { get; set; }
       
       TriggerType triggerType { get; set; }
       

   }

   public abstract class BaseEdgeTrigger: IOscTrigger
    {
       public enum EdgeType
       {
           /// <summary>
           /// Спад.
           /// </summary>
           Fall,
           /// <summary>
           /// Фронт.
           /// </summary>
           Rise,
           /// <summary>
           /// Спад или фронт (должен срабатывать при переходе через 0).
           /// </summary>
           Either
           
       }

       MeasPoint<Voltage> Level { get; set; }
       public IeeeBase Device { get; }
       public EdgeType Type { get; set; }
       public abstract void Getting();
       public abstract void Setting();
       public IOscillChanel SourceChanel { get; set; }
       public TriggerType triggerType { get; set; }
    }

   public interface IDeviceExtendFunc
   {
       public IeeeBase Device { get; }
       /// <summary>
       /// Получение всех параметров канала с прибора.
       /// </summary>
       public void Getting();
       /// <summary>
       /// Запись всех параметров канала в прибор.
       /// </summary>
       public void Setting();
    }

}
